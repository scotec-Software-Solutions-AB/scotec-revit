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
| **Thread safety** | Needs `volatile`, possibly locks | Not shared; no concurrent access possible |
| **Revit API lifetime** | `Document` / `UIDocument` can become invalid between event and consumption | Consumed within the scope created by the event — same call stack |
| **Coupling** | Consumers depend on `DocumentActivatedEvent` having fired *first* | No ordering dependency; context is injected by the framework at the right moment |
| **Subscription leaks** | Must `Dispose()` to unsubscribe; forgetting causes a memory leak | No manual subscription; DI scope disposal handles cleanup |
| **Testability** | Requires wiring an event aggregator and firing events in sequence | Inject a mock `IRevitContext` — one line |

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

- Events arrive out of order.
- The document is closed before the consumer acts.
- A handler is invoked on a different activation state than expected.

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
