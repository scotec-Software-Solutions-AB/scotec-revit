# Cross-Add-In Revit Context

This document describes the general problem of in-process communication between Revit add-ins
loaded in the same Revit instance, and the specific challenges this creates when one of those
add-ins needs access to a Revit document context. It then explains how `RevitContextTracker` and
`IRevitScopeFactory` — provided by Scotec.Revit — solve this for add-ins built on that framework.

---

## The Scenario

Revit loads all add-ins in the same process and executes them on the same main thread. This makes
in-process communication between add-ins both possible and common: one add-in (the *triggering*
add-in) can call into another add-in (the *responding* add-in) synchronously while its own entry
point is still on the call stack. Shared interfaces, C# events, and callbacks are all typical
mechanisms.

This is a general Revit add-in pattern and is not specific to Scotec.Revit. The triggering add-in
may or may not use Scotec.Revit; the framework it uses is irrelevant to the communication itself.

The problem arises for the **responding** add-in when it is built on Scotec.Revit and needs access
to a Revit document context to do its work. For the Revit API to be used legally, both add-ins must
be executing on the Revit main thread — the synchronous call guarantees this. What is not guaranteed
is which document is the correct one to work with.

**Example — a command in add-in A calls a shared coordination service:**

```
Add-in A: IExternalCommand.Execute
  → calls a shared coordination interface
	Add-in B (Scotec.Revit): responds — needs IRevitContext
```

**Example — an event handler in add-in A raises a C# event:**

```
Add-in A: DocumentChanged event handler
  → raises a C# event
	Add-in B (Scotec.Revit): subscriber executes — needs IRevitContext
```

In both cases, add-in B executes synchronously on the Revit main thread. It does not have an active
`RevitCommand`, `RevitEventHandler`, or `RevitTask` of its own. The standard per-invocation DI scope
created by those entry points does not exist for add-in B.

---

## The Problems

### No Active Scope in the Responding Add-In

Scotec.Revit automatically creates and populates a per-invocation DI scope for each of its entry
points. `IRevitContext` and `IRevitUiContext` are registered in that scope and are available to any
service resolved within it.

Add-in B, receiving a notification from add-in A, has none of those entry points active. Constructor
injection of `IRevitContext` is not available. The obvious alternative — resolving
`IGlobalRevitContext` or `IGlobalRevitUiContext` and constructing a context from them — is tempting
but introduces a correctness problem.

### The Document Identity Problem

`UIApplication.ActiveUIDocument` is not always the document that add-in A's entry point is working
with. Its reliability depends on the **kind** of entry point that is currently active:

| Entry point | `ActiveUIDocument` | Reliable? |
|---|---|---|
| `IExternalCommand.Execute` | The document the user invoked the command against | ✅ |
| `IExternalEventHandler.Execute` | The document active when the external event was raised | ✅ |
| `IExternalCommandAvailability.IsCommandAvailable` | The currently active document | ✅ |
| Revit event handler (e.g. `DocumentChanged`, `DocumentSaving`) | Not updated to the document being processed | ❌ |
| DMU updater (`IUpdater.Execute`) | The document being updated — not necessarily the active one | ❌ |

If add-in A is running a DMU updater or an event handler, constructing a context from
`ActiveUIDocument` in add-in B silently binds it to the wrong document. The error will not be
apparent until Revit API calls produce unexpected results or access elements in the wrong model.

### Nesting

The problem is compounded by nesting. Revit fires `DocumentChanged` synchronously after every
successful `transaction.Commit()`. A DMU updater fires synchronously during `transaction.Commit()`.
Both therefore execute while the enclosing command is still on the call stack. In that state,
`ActiveUIDocument` still reflects the command's document — but the document being processed by
the nested handler or updater may be a different one. Add-in B cannot know which of these entry
points is the immediate caller without additional information from the framework.

---

## The Solution

Scotec.Revit provides two components that work together to resolve this for add-ins built on the
framework:

- **`RevitContextTracker`** tracks which kind of Revit entry point is currently active across all
  Scotec.Revit-based add-ins in the same process.
- **`IRevitScopeFactory`** creates a child DI scope for add-in B, with or without `IRevitContext`,
  based solely on what the tracker reports at the moment of the call.

Together they allow add-in B to call a single method and receive a correctly populated scope,
without needing to know anything about add-in A, its framework, or its entry points.

### RevitContextTracker

`RevitContextTracker` maintains two process-wide environment variables. Because they are environment
variables, every add-in loaded in the same Revit process sees the same values.

| Property | Environment variable | Meaning |
|---|---|---|
| `RevitContextTracker.IsActive` | `Scotec.Revit.Context.Active` (integer) | `true` when at least one Revit entry point in any add-in built on Scotec.Revit is active. Incremented on entry, decremented on exit. Handles arbitrary nesting correctly. |
| `RevitContextTracker.IsCommandActive` | `Scotec.Revit.Context.Command` (boolean string) | `true` only when the active entry point is a document-stable command or external event (`IExternalCommand`, `IExternalEventHandler`, `IExternalCommandAvailability`) in an add-in built on Scotec.Revit. Event handlers and DMU updaters never set this. |

The distinction between the two properties is the key to correctness:

- `IsCommandActive == true` → `ActiveUIDocument` is reliable → a context can be constructed safely.
- `IsActive == true` with `IsCommandActive == false` → only handlers or updaters are running →
  `ActiveUIDocument` may refer to a different document → a context cannot be constructed safely.

Both properties are readable by any code in any add-in:

```csharp
if (RevitContextTracker.IsCommandActive)
{
	// ActiveUIDocument is the document the user is actively working with.
}
```

### IRevitScopeFactory

`IRevitScopeFactory` is the primary API for add-in B. Inject it as a constructor dependency and
call `CreateScope()` when a scope is needed. The returned `IServiceScope` is owned by the caller
and must be disposed when the operation is complete.

`IRevitScopeFactory` inherits `IServiceScopeFactory`. The framework also installs a forwarding
rule so that resolving `IServiceScopeFactory` from the container returns `RevitScopeFactory` — not
Autofac's built-in `AutofacServiceScopeFactory`. This means existing code that injects
`IServiceScopeFactory` automatically receives the Revit-aware factory with no changes required.
Inject `IRevitScopeFactory` specifically only when the `CreateScope(Action<IServiceCollection>?)`
overload — which allows additional services to be registered into the new scope — is needed.

`IRevitScopeFactory` is registered automatically by `RevitApp` and `RevitDbApp`. No manual
registration is required.

```csharp
public class CoordinationService
{
	private readonly IRevitScopeFactory _scopeFactory;

	public CoordinationService(IRevitScopeFactory scopeFactory)
	{
		_scopeFactory = scopeFactory;
	}

	public void RespondToSignal()
	{
		using var scope = _scopeFactory.CreateScope();
		var context = scope.ServiceProvider.GetService<IRevitContext>();

		if (context is null)
		{
			// No reliable Revit context is available at this moment.
			// See "The Handler Limitation" section below.
			return;
		}

		var doc = context.Document;
		// Perform Revit work against doc.
	}
}
```

`CreateScope()` also accepts an optional `Action<IServiceCollection>` delegate to register
additional services into the new scope:

```csharp
using var scope = _scopeFactory.CreateScope(services =>
{
	services.AddTransient<IMyOperation, MyOperation>();
});
var operation = scope.ServiceProvider.GetRequiredService<IMyOperation>();
```

---

## How the Factory Decides What to Create

`CreateScope()` evaluates the following conditions in order. The first matching branch applies.

| # | Condition | Scope returned |
|---|---|---|
| 1 | `IsActive == false` — no entry point active anywhere in the process | Plain scope — no `IRevitContext` |
| 2 | `IRevitContext` is already resolvable from the current scope | Plain child scope — `IRevitContext` inherited from the enclosing scope |
| 3 | This add-in's own Command-kind entry point is active but has not yet registered its scope | Plain scope — no `IRevitContext`; the entry point will register the context moments later |
| 4a | A Command-kind entry point in a **different** add-in is active (`IsCommandActive == true`) | Scope with a fresh `IRevitContext` (and `IRevitUiContext` if UI context is registered) |
| 4b | Only Handler-kind entry points are active (`IsCommandActive == false`) | Plain scope — no `IRevitContext` |

Branch 4a is the path taken when the cross-add-in scenario applies correctly. Branch 4b is the
safety guard that prevents the wrong document from being captured silently.

---

## The Handler Limitation (Branch 4b)

When add-in A's active entry point is a Revit event handler or DMU updater, `CreateScope()` returns
a plain scope with no `IRevitContext`. This is intentional. The document being processed is
identified by the event arguments passed to that specific handler, not by `ActiveUIDocument`. The
framework has no way to reach across add-in A's scope and retrieve that document.

The general approach is the same in both cases: add-in A includes the document's path in the signal
payload, and add-in B uses that path to resolve the correct `Document` object independently.
`Document.PathName` is a stable, process-unique identifier for open documents and is safe to pass
as part of any in-process signal.

**Add-in A — inside the handler, include the path in the signal payload:**

```csharp
[RevitEventHandlerExecute]
private void OnDocumentChanged(IRevitContext context, DocumentChangedEventArgs args)
{
	var signal = new WorkRequest
	{
		DocumentPath = context.Document!.PathName
		// additional payload
	};

	_coordinator.Dispatch(signal);
}
```

Add-in B then resolves the `Document` from the received path. Two approaches are possible,
depending on the expected usage pattern.

### Option 1: Query Revit's Open Document List

Use `IGlobalRevitContext.Application.Documents` to find the document by path at the time the
signal arrives. No additional state is required.

```csharp
public class CoordinationService
{
	private readonly IGlobalRevitContext _globalContext;

	public CoordinationService(IGlobalRevitContext globalContext)
	{
		_globalContext = globalContext;
	}

	public void OnWorkRequested(WorkRequest request)
	{
		var document = _globalContext.Application.Documents
			.Cast<Document>()
			.FirstOrDefault(d => d.PathName == request.DocumentPath);

		if (document is null)
			return;

		// Use document directly.
	}
}
```

This approach requires no additional infrastructure. It is appropriate when signals are infrequent
or when add-in B does not otherwise need to track open documents.

### Option 2: Maintain a Local Document Map

For add-ins that receive frequent signals or already track document lifecycle events for other
reasons, maintaining an in-process map avoids iterating the full document list on every signal.
This is application-level code that the developer implements — no such registry exists in
Scotec.Revit.

**The map:**

```csharp
public class OpenDocumentMap
{
	private readonly ConcurrentDictionary<string, Document> _documents = new();

	public void Add(Document document)
		=> _documents[document.PathName] = document;

	public void Remove(Document document)
		=> _documents.TryRemove(document.PathName, out _);

	public bool TryGet(string path, [NotNullWhen(true)] out Document? document)
		=> _documents.TryGetValue(path, out document);
}
```

**Lifecycle handlers that keep the map current:**

```csharp
public class DocumentOpenedTracker : RevitDocumentOpenedHandler
{
	private readonly OpenDocumentMap _map;

	public DocumentOpenedTracker(ControlledApplication application, OpenDocumentMap map)
		: base(application)
	{
		_map = map;
	}

	[RevitEventHandlerExecute]
	private void OnOpened(IRevitContext context)
		=> _map.Add(context.Document!);
}

public class DocumentClosingTracker : RevitDocumentClosingHandler
{
	private readonly OpenDocumentMap _map;

	public DocumentClosingTracker(ControlledApplication application, OpenDocumentMap map)
		: base(application)
	{
		_map = map;
	}

	[RevitEventHandlerExecute]
	private void OnClosing(IRevitContext context)
		=> _map.Remove(context.Document!);
}
```

Register `OpenDocumentMap` and both handlers as singletons in `ConfigureServices`. The responding
service then injects `OpenDocumentMap` directly:

```csharp
public void OnWorkRequested(WorkRequest request)
{
	if (!_map.TryGet(request.DocumentPath, out var document))
		return;

	// Use document directly.
}
```

---

## Constraints and Limitations

- **Revit API calls must be made on the Revit UI thread.** Communication between add-ins can
  happen on any thread. However, any code that touches the Revit API must run on the Revit UI
  thread. If a signal arrives on a background thread, the Revit API work must be marshalled onto
  the Revit UI thread — for example via a `Dispatcher`. `IRevitScopeFactory` does not perform
  any thread marshalling and must only be called from the Revit UI thread.

- **Assembly isolation affects in-process communication contracts.** When add-ins run in separate
  `AssemblyLoadContext` instances, each context has its own copy of every assembly it loads. A
  type from one context is not the same type as its counterpart in another, so a direct in-process
  call across isolated add-ins will fail with a cast exception if the contract type is not loaded
  from a shared context. Loading contract assemblies into a shared context resolves this, but it
  introduces a coupling between the add-ins and reduces their independence. The preferred approach
  for truly independent add-ins is to communicate through an out-of-process or OS-level channel
  — named pipes, shared memory, TCP/IP, or similar — which avoids type identity problems entirely
  and does not require any shared assembly infrastructure. See
  [Revit Add-in Isolation](RevitAddinIsolation.md) for shared context configuration when
  in-process communication is required despite the coupling trade-off.

- **The tracker only reflects activity in Scotec.Revit-based add-ins.** `RevitContextTracker`
  is maintained by Scotec.Revit entry points. If the triggering add-in does not use Scotec.Revit,
  its entry points are invisible to the tracker — `IsActive` and `IsCommandActive` will both be
  `false` even while that add-in is inside `IExternalCommand.Execute`. Branch 4b will apply and
  no context will be auto-created. In that case, passing document identity through the signal
  payload (see above) is the only option.

- **Branch 4b returns a plain scope, not an error.** Add-in B must always check whether
  `IRevitContext` is available in the returned scope before using it. Calling
  `GetRequiredService<IRevitContext>()` without that check will throw if the factory chose
  Branch 4b.
