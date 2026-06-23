# Revit Context Interfaces

This document explains the context interfaces in the `Scotec.Revit` framework, when to use each one, how they are registered, and the mistakes to avoid.

## Overview

The framework provides two distinct groups of context interfaces, each with a different lifetime and purpose.

| Interface | Lifetime | Available |
|---|---|---|
| `IRevitContext` | Scoped — per command execution or event invocation | Only inside an active Revit callback |
| `IRevitUiContext` | Scoped — per command execution or event invocation | Only inside an active Revit UI callback |
| `IGlobalRevitContext` | Singleton — entire add-in lifetime | Always, from any component |
| `IGlobalRevitUiContext` | Singleton — entire add-in lifetime | Always, from any component |

The scoped interfaces — `IRevitContext` and `IRevitUiContext` — are the standard mechanism for accessing Revit data within any active Revit API context. The framework registers them automatically in a per-invocation DI scope across three integration points:

| Integration point | How the scope is created |
|---|---|
| `RevitCommand` | One scope per command execution |
| `RevitEventHandler<>` | One scope per event invocation |
| `RevitTask` | One scope per `Run` call (DI-based overloads only) |

All three integration points follow the same injection rules — the sections below use `RevitCommand` for most examples because it is the most common case, but everything applies equally to event handlers and `RevitTask`.

---

## Scoped Contexts

Scoped contexts are created by the framework at the start of each `RevitCommand` execution, `RevitEventHandler<>` invocation, or `RevitTask.Run` call (DI-based overloads), and disposed when it ends. They are the standard way to access `Application`, `Document`, `UIApplication`, and related objects inside any of those contexts.

### `IRevitContext`

Provides access to `Application` and `Document`. This is the minimum interface needed to read or write document data inside a command.

```
Application   — always set
Document      — set when a document is open; null otherwise
```

#### In a RevitCommand

Inject `IRevitContext` directly into the method marked with `[RevitCommandExecute]`:

```csharp
[RevitTransactionMode(RevitTransactionMode.Transaction)]
public class UpdateParameterCommand : RevitCommand
{
	protected override string TransactionName => "Update Parameter";

	[RevitCommandExecute]
	private Result Execute(IRevitContext context)
	{
		var doc = context.Document;
		if (doc is null)
			return Result.Cancelled;

		// Read or modify document elements here.
		return Result.Succeeded;
	}
}
```

`Document` may be `null` for commands that do not require an open document. Always null-check before use.

#### In a scoped service called from a command

Any service resolved within the command's DI scope can also receive `IRevitContext` by constructor injection:

```csharp
public class ParameterWriter
{
	private readonly IRevitContext _context;

	public ParameterWriter(IRevitContext context)
	{
		_context = context;
	}

	public void Write(ElementId id, string value)
	{
		var element = _context.Document?.GetElement(id);
		// Modify element...
	}
}
```

Register the service via `ConfigureServices` and inject it alongside the context:

```csharp
protected override void ConfigureServices(IServiceCollection services)
{
	services.AddTransient<ParameterWriter>();
}

[RevitCommandExecute]
private Result Execute(IRevitContext context, ParameterWriter writer)
{
	writer.Write(someId, "new value");
	return Result.Succeeded;
}
```

---

### `IRevitUiContext`

Extends `IRevitContext` with UI-layer access: `UiApplication`, `UiDocument`, and `ActiveView`. Use this when the command also needs ribbon interaction, element selection, or access to the active view.

```
UiApplication — always set
UiDocument    — set when a document is open; null otherwise
ActiveView    — reflects the view active at the moment of access; null when no document is open
```

#### In a RevitCommand

```csharp
[RevitTransactionMode(RevitTransactionMode.Transaction)]
public class PlaceElementCommand : RevitCommand
{
	protected override string TransactionName => "Place Element";

	[RevitCommandExecute]
	private Result Execute(IRevitUiContext context)
	{
		var view = context.ActiveView;
		if (view is null)
			return Result.Cancelled;

		var selection = context.UiDocument?.Selection;
		// Work with the current selection or active view.
		return Result.Succeeded;
	}
}
```

`IRevitUiContext` is always registered in the command scope — prefer it over `IRevitContext` when you need UI access. The two interfaces can be injected independently into different methods or services within the same scope.

Note that `ActiveView` is evaluated lazily — it reflects the view that is active when you read the property, not the view that was active when the command started executing.

#### In lifecycle methods

Both `[RevitCommandBeforeExecute]` and `[RevitCommandAfterExecute]` methods participate in the same DI scope and can receive `IRevitUiContext` as a parameter:

```csharp
[RevitTransactionMode(RevitTransactionMode.Transaction)]
public class MyCommand : RevitCommand
{
	protected override string TransactionName => "My Command";

	[RevitCommandBeforeExecute]
	private void Setup(IRevitUiContext context)
	{
		// Runs before the transaction opens.
		// Suitable for pre-checks or reading current selection.
		var selected = context.UiDocument?.Selection.GetElementIds();
	}

	[RevitCommandExecute]
	private Result Execute(IRevitUiContext context, IMyService service)
	{
		service.DoWork(context.Document);
		return Result.Succeeded;
	}

	[RevitCommandAfterExecute]
	private void Cleanup(IRevitUiContext context)
	{
		// Runs after the transaction has been committed or rolled back.
		// Suitable for refreshing the UI or post-processing.
		context.UiDocument?.RefreshActiveView();
	}
}
```

---

### In event handlers

Event handlers are an equally important integration point. `RevitEventHandler<>` creates the same per-invocation DI scope and registers `IRevitContext` and `IRevitUiContext` using identical rules. The context type is declared as the third type parameter of `RevitEventHandler<TSender, TEventArgs, TContext>`.

```csharp
public class MyDocumentHandler : RevitEventHandler<Application, DocumentOpenedEventArgs, IRevitContext>
{
	protected override void OnExecute(IRevitContext context)
	{
		var doc = context.Document;
		// React to the document-opened event.
	}
}
```

```csharp
public class MyViewActivatedHandler : RevitEventHandler<UIApplication, ViewActivatedEventArgs, IRevitUiContext>
{
	protected override void OnExecute(IRevitUiContext context)
	{
		var view = context.ActiveView;
		// React to the view-activated event.
	}
}
```

Services registered via `ConfigureServices` on the handler class are resolved from the same scope and can receive `IRevitContext` or `IRevitUiContext` by constructor injection, just as in a command.

---

### In RevitTask

`RevitTask` implements `IExternalEventHandler` and is the standard way to execute Revit API code from a background thread, a modeless WPF window, or any other context that is not itself a command or event handler.

The direct-mode overloads (`Func<IRevitUiContext, TResult>` / `Action<IRevitUiContext>`) pass `IRevitUiContext` directly to your lambda — no DI scope is created:

```csharp
var revitTask = new RevitTask("ReadElements");

int count = await revitTask.Run(context =>
{
	return context.Document?.GetElementIds().Count ?? 0;
});
```

The DI-based overloads accept a `Delegate` whose parameters are resolved from a scoped lifetime. `IRevitContext` and `IRevitUiContext` are both registered in that scope, so they can be injected alongside any other service:

```csharp
await revitTask.Run(
	(IRevitUiContext context, IMyService service) =>
	{
		service.DoWork(context.Document);
	},
	configureServices: services => services.AddTransient<IMyService, MyService>()
);
```

> **Note:** `RevitTask` only provides `IRevitUiContext` (it wraps a `UIApplication`). `IRevitContext` is also resolvable from the scope because `IRevitUiContext` extends it and is registered under both keys.

---

## Global (Singleton) Contexts

Global contexts are singletons registered in the root DI container at add-in startup. They are always resolvable, regardless of whether a Revit callback is currently executing.

> **Important:** Global contexts are intended for lightweight checks — ribbon state, command availability, and service initialisation. They are not a substitute for the scoped `IRevitContext`. Document data should be accessed inside a scoped context, not through a singleton.

---

### `IGlobalRevitContext`

Provides singleton access to `Application`. Available in both `RevitApp`- and `RevitDbApp`-based add-ins.
In a `RevitApp` add-in it is satisfied by the same `GlobalRevitUiContext` instance that implements `IGlobalRevitUiContext`.

```
Application — available after ApplicationInitialized fires; throws before that
```

Typical uses from singleton services:

- Reading stable session metadata: `VersionNumber`, `Language`, `SharedParametersFilename`
- Iterating all currently open documents via `Application.Documents`

```csharp
public class MyService
{
    private readonly IGlobalRevitContext _context;

    public MyService(IGlobalRevitContext context)
    {
        _context = context;
    }

    public IEnumerable<string> GetOpenDocumentPaths()
    {
        foreach (Document doc in _context.Application.Documents)
            yield return doc.PathName;
    }
}
```

Note that `Application.Documents` reflects open documents at the moment of the call. Iterating it outside a Revit API context (i.e. not inside an event handler or command) requires care: documents may be opened or closed concurrently by the user.

---

### `IGlobalRevitUiContext`

Extends `IGlobalRevitContext` with `UiApplication`, `UiDocument`, `ActiveDocument`, and `ActiveView`.
Only available in `RevitApp`-based add-ins. Not registered in `RevitDbApp`-based add-ins.

```
UiApplication  — throws if the underlying UIApplication is no longer valid
UiDocument     — null when no document is open
ActiveDocument — null when no document is open
ActiveView     — null when no document is open; throws if the view is no longer valid
```

All document-related members reflect the state **at the moment of access**. They may change or become null between consecutive reads if the user opens or closes a document.

---

## Registration

Global contexts are registered automatically by the framework inside `RevitApp.OnConfigure` and `RevitDbApp.OnConfigure` through the `AddGlobalRevitContext` extension method. **No manual registration is required.**

### For `RevitApp`-based add-ins

`OnConfigure` calls:

```csharp
services.AddGlobalRevitContext(Application); // Application is UIControlledApplication
```

This registers a single `GlobalRevitUiContext` instance as both `IGlobalRevitUiContext` and `IGlobalRevitContext`.

### For `RevitDbApp`-based add-ins

`OnConfigure` calls:

```csharp
services.AddGlobalRevitContext(Application); // Application is ControlledApplication
```

This registers a `GlobalRevitContext` instance as `IGlobalRevitContext` only. `IGlobalRevitUiContext` is not available.

### Manual registration (advanced)

If you are building a host outside of `RevitApp` or `RevitDbApp`, call the extension methods explicitly in your `OnConfigure` override:

```csharp
// UI add-in
builder.ConfigureServices(services =>
{
	services.AddGlobalRevitContext(uiControlledApplication);
});

// DB add-in
builder.ConfigureServices(services =>
{
	services.AddGlobalRevitContext(controlledApplication);
});
```

---

## Common Mistakes

### Injecting a scoped context into a singleton

`IRevitContext` and `IRevitUiContext` are scoped to a single command execution or event invocation. Injecting them into a singleton service will either fail at resolution time or produce a captured, already-disposed context.

```csharp
// Wrong — IRevitContext is scoped; this singleton will capture a disposed context.
public class MySingletonService
{
	public MySingletonService(IRevitContext context) { ... }
}
```

Use `IGlobalRevitContext` or `IGlobalRevitUiContext` in singleton services, or restructure so document access happens inside a scoped component (command method or scoped service).

---

### Accessing `Document` from the global context

Neither `IGlobalRevitContext` nor `IGlobalRevitUiContext` exposes a `Document` property. `ActiveDocument` on `IGlobalRevitUiContext` returns the currently open document, but it may be `null` and may change at any time. It is not safe for transactional document access.

```csharp
// Risky — ActiveDocument may be null or change between the null-check and the transaction.
var doc = _globalUiContext.ActiveDocument;
transaction.Start(); // doc may be stale or null
```

Transactional access and any work that reads or modifies document elements belongs inside a scoped `IRevitContext`, obtained from a `[RevitCommandExecute]` method or a command's scoped service — not from the global context.

---

### Accessing `Application` before `ApplicationInitialized`

`GlobalRevitContext.Application` is not set until Revit fires the `ApplicationInitialized` event. Accessing it before that event throws `InvalidOperationException`. This can happen if a hosted service resolves `IGlobalRevitContext` during its own startup before the event has fired.

```csharp
// May throw if the hosted service starts before ApplicationInitialized.
public class MyHostedService : IHostedService
{
	public MyHostedService(IGlobalRevitContext context)
	{
		_ = context.Application; // Unsafe here — event may not have fired yet.
	}
}
```

Defer access to `Application` until after the `ApplicationInitialized` event, or access it only inside methods that are called at or after that point.

---

### Storing a scoped context beyond the execution scope

A scoped context is disposed at the end of the command execution, event invocation, or `RevitTask.Run` call that created it. Storing it in a field and reading it later will throw `ObjectDisposedException`.

```csharp
// Wrong — context is disposed when Execute() returns.
public class MyCommand : RevitCommand
{
	private IRevitContext? _stored;

	[RevitCommandExecute]
	private Result Execute(IRevitContext context)
	{
		_stored = context; // Never do this.
		return Result.Succeeded;
	}

	public void Later() => _ = _stored!.Document; // Throws ObjectDisposedException.
}
```

The same rule applies in event handlers and `RevitTask` lambdas. All work that requires the context must complete before the enclosing callback returns. If you need data later, copy it out into plain values — not the context object itself.

---

## Interface Hierarchy

```
IGlobalRevitContext
	└── IGlobalRevitUiContext

IRevitContext
	└── IRevitUiContext
```

The global and scoped hierarchies are independent. There is no inheritance relationship between them.

---

## Quick Reference

| Need | Use |
|---|---|
| Read or write document data inside a command | `IRevitContext` — inject into `[RevitCommandExecute]` or a scoped service |
| Access the active view, selection, or `UIDocument` inside a command | `IRevitUiContext` — inject into `[RevitCommandExecute]`, `[RevitCommandBeforeExecute]`, or `[RevitCommandAfterExecute]` |
| Read or write document data inside an event handler | `IRevitContext` — third type parameter of `RevitEventHandler<>` or constructor-injected into a scoped service |
| Access the active view or `UIDocument` inside an event handler | `IRevitUiContext` — third type parameter of `RevitEventHandler<>` |
| Execute Revit API code from a background thread or modeless UI | `RevitTask.Run` — receives `IRevitUiContext` directly (direct mode) or via DI (delegate mode) |
| Read stable application metadata (language, version, paths) or iterate open documents from a singleton service | `IGlobalRevitContext` |
| Check ribbon state or command availability from a singleton service | `IGlobalRevitUiContext` |
| Do transactional document work from a singleton | Restructure — use a scoped context inside a command, event handler, or `RevitTask.Run` |
