# Revit Context Interfaces

This document explains the context interfaces in the `Scotec.Revit` framework, when to use each one, how they are registered, and the mistakes to avoid.

## Overview

The framework provides two distinct groups of context interfaces, each with a different lifetime and purpose.

| Interface | Lifetime | Available |
|---|---|---|
| `IRevitContext` | Scoped — per event invocation or command execution | Only inside an active Revit callback |
| `IRevitUiContext` | Scoped — per event invocation or command execution | Only inside an active Revit UI callback |
| `IGlobalRevitContext` | Singleton — entire add-in lifetime | Always, from any component |
| `IGlobalRevitUiContext` | Singleton — entire add-in lifetime | Always, from any component |

---

## Scoped Contexts

### `IRevitContext`

Provides access to `Application` and `Document` within the scope of a single Revit event handler or external command invocation. The context is created by the framework at the start of each invocation and disposed at the end.

```
Application   — always set
Document      — set when a document is open; null otherwise
```

Use `IRevitContext` when your service or handler needs to read or write document data.

```csharp
public class MyDocumentHandler : RevitEventHandler<..., IRevitContext>
{
	protected override void OnExecute(IRevitContext context)
	{
		var doc = context.Document;
		// Read or write document data here.
	}
}
```

`Document` may be `null` for commands that do not require an open document. Always null-check before use.

---

### `IRevitUiContext`

Extends `IRevitContext` with UI-layer access: `UiApplication`, `UiDocument`, and `ActiveView`. Use this when your handler or command also needs ribbon interaction, selection, or active view access.

```
UiApplication — always set
UiDocument    — set when a document is open; null otherwise
ActiveView    — reflects the view active at the moment of access; null when no document is open
```

```csharp
public class MyUiHandler : RevitEventHandler<..., IRevitUiContext>
{
	protected override void OnExecute(IRevitUiContext context)
	{
		var view = context.ActiveView;
		// Interact with the active view.
	}
}
```

Note that `ActiveView` is evaluated lazily — it reflects the view that is active when you read the property, not the view that was active when the event fired.

---

### Injection inside scoped services

Scoped contexts are registered automatically in the per-invocation DI scope. Inject them via the constructor of any service that is resolved within that scope.

```csharp
public class MyService
{
	private readonly IRevitUiContext _context;

	public MyService(IRevitUiContext context)
	{
		_context = context;
	}

	public void DoWork()
	{
		var doc = _context.Document;
		// ...
	}
}
```

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

`IRevitContext` and `IRevitUiContext` are scoped. Injecting them into a singleton service will either fail at resolution time or produce a captured, already-disposed context.

```csharp
// Wrong — IRevitContext is scoped; this singleton will capture a disposed context.
public class MySingletonService
{
	public MySingletonService(IRevitContext context) { ... }
}
```

Use `IGlobalRevitContext` or `IGlobalRevitUiContext` in singleton services, or restructure so document access happens in a scoped component.

---

### Accessing `Document` from the global context

Neither `IGlobalRevitContext` nor `IGlobalRevitUiContext` exposes a `Document` property. `ActiveDocument` on `IGlobalRevitUiContext` returns the currently open document, but it may be `null` and may change at any time. It is not safe for transactional document access.

```csharp
// Risky — ActiveDocument may be null or change between the null-check and the transaction.
var doc = _globalUiContext.ActiveDocument;
transaction.Start(); // doc may be stale or null
```

Transactional access and any work that reads or modifies document elements belongs inside a scoped `IRevitContext`, obtained from a command or event handler.

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

### Storing a scoped context beyond the invocation scope

A scoped context is disposed at the end of the invocation that created it. Storing it in a field and reading it later will throw `ObjectDisposedException`.

```csharp
// Wrong — context is disposed after the handler returns.
public class MyHandler : RevitEventHandler<..., IRevitContext>
{
	private IRevitContext? _stored;

	protected override void OnExecute(IRevitContext context)
	{
		_stored = context; // Never do this.
	}

	public void Later() => _ = _stored!.Document; // Throws ObjectDisposedException.
}
```

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
| Read or write document data inside a command or event handler | `IRevitContext` |
| Access the active view or UIDocument inside a command or event handler | `IRevitUiContext` |
| Read stable application metadata (language, version, paths) or iterate open documents from a singleton service | `IGlobalRevitContext` |
| Check ribbon state or command availability from a singleton service | `IGlobalRevitUiContext` |
| Handle Revit events | `RevitEventHandler<>` — not the context interfaces |
| Do transactional document work from a singleton | Restructure — use a scoped context |
