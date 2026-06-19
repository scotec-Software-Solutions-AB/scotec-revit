# `IRevitContext` / `IRevitUiContext` vs. Singleton Document Providers

## The Problem with Singleton Document Providers

A common pattern in Revit add-ins is a singleton service that caches the active document by listening to Revit events:

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
		_activeUiDocument = ((UIApplication)sender).ActiveUIDocument;
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
| **Thread safety** | `volatile` guards the reference, but Revit API objects are only valid on the Revit main thread — a background thread that reads the cached reference cannot call any API on it | Not shared; confined to the event call stack on the Revit main thread |
| **Revit API lifetime** | `Document` / `UIDocument` can become invalid between event and consumption | Consumed within the scope created by the event — same call stack |
| **Coupling** | Consumers get `null` until `ViewActivated` has fired at least once; no document is available at startup until the user activates a view | No ordering dependency; context is injected by the framework at the right moment |
| **Subscription management** | Must implement `IDisposable`, ensure the container disposes it, and unsubscribe during shutdown to prevent callbacks firing during teardown | No manual subscription; the framework subscribes and unsubscribes automatically |
| **Testability** | Requires mocking `UIControlledApplication` and raising `ViewActivated` in the right sequence to put the provider in a testable state | Inject a mock `IRevitContext` — one line |

## Why the Scoped Context Wins

A Revit event handler has a precise, bounded lifetime: **start of event callback → end of event callback**.
`IRevitContext` and `IRevitUiContext` model exactly that with a per-invocation DI scope:

```
Revit fires event
  → HandleEvent creates child DI scope
  → CreateContext captures sender/args into IRevitContext
  → Services resolve IRevitContext from scope (always the right document)
  → Handler logic executes
  → Scope disposed → context disposed
```

The singleton provider tries to simulate this by watching events, but it is a simulation that can be
wrong whenever:

- `ViewActivated` does not fire when a document is closed. The cached reference remains set after closure, pointing to an invalid `Document` object.
- The document is closed before the consumer acts on the cached reference.
- A handler is invoked while the active document has changed since the last `ViewActivated` fired.

## The Design Principle

> `CurrentDocumentProvider` answers *"what was the last document Revit told me about?"*
>
> `IRevitContext` answers *"what is the document for the event currently executing?"*

Only the second question has a guaranteed correct answer.
The first is eventually-consistent at best.

## Usage Example

Instead of injecting a singleton provider:

```csharp
// ❌ Singleton — may be stale
public class MyService(ICurrentDocumentProvider provider)
{
	public void DoWork()
	{
		var doc = provider.ActiveDocument; // could be null or already closed
	}
}
```

Inject the scoped context directly into a handler. The recommended approach uses `AddHandler()` with a
delegate whose parameters are resolved from the per-invocation DI scope:

```csharp
// ✅ Scoped — always matches the current event
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
The `AddHandler` delegate receives `IRevitContext` resolved from the per-invocation DI scope.

Alternatively, use the `[RevitEventHandlerExecute]` attribute on a method with any combination of
DI-resolved parameters:

```csharp
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

The framework creates the `IRevitContext` from the event arguments, registers it in the per-invocation
DI scope, and disposes the scope — and the context — when the handler returns.
No global state, no manual subscription management, no staleness.
