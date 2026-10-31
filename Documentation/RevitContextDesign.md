# `IRevitContext` / `IRevitUiContext` vs. Singleton Document Providers

## The Problem with Singleton Document Providers

The following implementation illustrates a pattern that looks convenient at first glance —
a singleton service that caches the active document by listening to Revit events,
intended to make the document available to commands and other services:

```csharp
internal class CurrentDocumentProvider : ICurrentDocumentProvider, IDisposable
{
	private volatile Document?   _activeDocument;
	private volatile UIDocument? _activeUiDocument;
	private readonly UIControlledApplication _application;

	public Document?   ActiveDocument   => _activeDocument;
	public UIDocument? ActiveUiDocument => _activeUiDocument;

	public CurrentDocumentProvider(UIControlledApplication application)
	{
		_application = application;
		_application.ViewActivated += OnViewActivated;
	}

	private void OnViewActivated(object sender, ViewActivatedEventArgs e)
	{
		_activeDocument   = e.Document;
		_activeUiDocument = new UIDocument(e.Document);
	}

	public void Dispose()
	{
		_application.ViewActivated -= OnViewActivated;
	}
}
```

This looks convenient, but it introduces a set of structural problems.

## Comparison

| Issue | `CurrentDocumentProvider` | `IRevitContext` / `IRevitUiContext` |
|---|---|---|
| **Staleness** | Holds the *last seen* document — may already be closed | Created from the *current event sender* — always correct for this invocation |
| **Thread safety** | `volatile` is a code smell here — all Revit API access must happen on the Revit main thread, and Revit events fire on that thread. Neither `volatile` nor locks can make a cached Revit object safe to use from a background thread; that is a Revit threading violation regardless | Not shared; confined to the event call stack on the Revit main thread |
| **Revit API lifetime** | `Document` / `UIDocument` can become invalid between event and consumption | Consumed within the scope created by the event — same call stack |
| **Coupling** | Consumers get `null` until `ViewActivated` has fired at least once; no document is available at startup until the user activates a view | No ordering dependency; context is injected by the framework at the right moment |
| **Subscription management** | Must implement `IDisposable`, ensure the container disposes it, and unsubscribe during shutdown to prevent callbacks firing during teardown | No manual subscription; the framework subscribes and unsubscribes automatically |
| **Testability** | Requires mocking `UIControlledApplication` and raising `ViewActivated` in the right sequence to put the provider in a testable state | Inject a mock `IRevitContext` — one line |

## Why the Scoped Context Wins

Both command execution and event handling have a precise, bounded lifetime:
**start of invocation → end of invocation**.
`IRevitUiContext` (commands) and `IRevitContext` / `IRevitUiContext` (event handlers) model exactly
that with a per-invocation DI scope.

**Command execution:**

```
Revit invokes IExternalCommand.Execute
  → framework creates RevitUiContext from commandData.Application
  → child DI scope opened; IRevitUiContext and IRevitContext registered
  → services resolve IRevitUiContext from scope (always the right document)
  → command logic executes
  → scope disposed → context disposed
```

**Event handling:**

```
Revit fires event
  → HandleEvent creates child DI scope
  → CreateContext captures sender/args into IRevitContext
  → services resolve IRevitContext from scope (always the right document)
  → handler logic executes
  → scope disposed → context disposed
```

The singleton provider tries to simulate this by watching events, but it is a simulation that can be
wrong whenever:

- `ViewActivated` does not fire when a document is closed — the cached reference stays set after closure, pointing to an invalid `Document` object that will throw on any API access.
- A handler runs while the active document has changed since the last `ViewActivated` fired.

## The Design Principle

> `CurrentDocumentProvider` answers *"what was the last document Revit told me about?"*
>
> `IRevitUiContext` in a command answers *"what is the document the user invoked this command against?"*

Only the second question has a guaranteed correct answer.
The first is eventually-consistent at best.

## Usage Example

Instead of injecting a singleton provider into a service that commands call:

```csharp
// ❌ Singleton — may be stale
public class MyService(ICurrentDocumentProvider provider)
{
	public Result DoWork()
	{
		var doc = provider.ActiveDocument; // could be null or already closed
	}
}
```

Inject `IRevitUiContext` directly into the command. The recommended approach uses a method
marked with `[RevitCommandExecute]` whose parameters are all resolved from the per-invocation DI scope:

```csharp
// ✅ Scoped — always the document the user invoked the command against
[RevitTransactionMode(RevitTransactionMode.Transaction)]
public class MyCommand : RevitCommand
{
	[RevitCommandExecute]
	protected Result Execute(IRevitUiContext context, IMyService service)
	{
		var doc = context.Document!; // document guaranteed valid here
		return service.DoWork(doc);
	}
}
```

The framework creates `IRevitUiContext` from `commandData.Application`, registers it in the
per-invocation DI scope alongside `IRevitContext`, and disposes the scope when the command returns.

For commands that only need document access without UI objects, inject `IRevitContext` instead:

```csharp
[RevitCommandExecute]
protected Result Execute(IRevitContext context, IMyService service)
{
	return service.DoWork(context.Document!);
}
```

---

### Event Handler Example

The same principle applies to event handlers. The recommended approach uses `[RevitEventHandlerExecute]`
with DI-resolved parameters:

```csharp
// ✅ Scoped — always matches the current event
public class MyDocumentOpenedHandler : RevitDocumentOpenedHandler
{
	private readonly IMyService _service;

	public MyDocumentOpenedHandler(ControlledApplication application, IMyService service)
		: base(application)
	{
		_service = service;
	}

	[RevitEventHandlerExecute]
	protected void Execute(IRevitContext context, DocumentOpenedEventArgs args)
	{
		_service.DoWork(context.Document!); // document guaranteed valid here
	}
}
```

Alternatively, use `AddHandler()` with a delegate:

```csharp
public class MyDocumentOpenedHandler : RevitDocumentOpenedHandler
{
	public MyDocumentOpenedHandler(ControlledApplication application, IMyService service)
		: base(application)
	{
		AddHandler((IRevitContext context) =>
			service.DoWork(context.Document!)); // document guaranteed valid here
	}
}
```

`RevitDocumentOpenedHandler` already subscribes to `Application.DocumentOpened` in its constructor.
The framework creates the `IRevitContext` from the event arguments, registers it in the per-invocation
DI scope, and disposes the scope when the handler returns.
