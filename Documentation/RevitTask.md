# RevitTask Usage Guide

The `RevitTask` class provides a robust and convenient way to execute
operations within the Autodesk Revit API context using the
`IExternalEventHandler` mechanism. It is designed to help you safely
schedule and run code that interacts with the Revit API, regardless of
whether your calling code is on the Revit UI thread, a background
thread, or from an external UI (such as WPF or WinForms).

`RevitTask` supports two usage modes:

| Mode | When to use |
|---|---|
| **Direct** (`Func`/`Action`) | Simple callbacks that receive `UIApplication` directly; no DI container required |
| **DI-based** (`Delegate`) | Callbacks whose parameters are resolved automatically from the DI container |

## Understanding IExternalEventHandler and the Revit API Context

### Why is IExternalEventHandler Needed?

The Revit API enforces strict rules:
**All operations that modify or query the Revit database must be executed within a valid Revit API context.**

A valid API context exists only:

- During execution of an `IExternalCommand`
- Inside certain Revit event handlers
- Inside an `IExternalEventHandler.Execute` method

If you attempt to access the Revit API outside this context (for example, from a WPF button click handler, a background
thread, a timer, or an async continuation), you will encounter exceptions or undefined behavior.

The `IExternalEventHandler` interface, together with `ExternalEvent`, is Revit's official mechanism for marshaling code
execution into the correct API context.

## Typical Use Cases

- Running Revit API code in response to UI actions in a WPF/WinForms add-in.
- Scheduling database operations from asynchronous or background workflows.
- Decoupling business/UI logic from direct Revit API calls.
- Building modeless UI tools.


## How RevitTask Supports Safe API Access

`RevitTask` encapsulates the `IExternalEventHandler` pattern and provides a simple, awaitable API for running code in
the Revit context.

### Key Features

- Schedules your delegate to run in the Revit API context, regardless of the calling thread.
- Supports synchronous and asynchronous workflows via `Task` and `Task<TResult>`.
- Propagates exceptions correctly.
- Avoids direct Revit API access from invalid contexts.
- Supports DI-based parameter resolution with optional and required service conventions (DI mode).


## Basic Usage (Direct Mode)

Direct mode requires no DI container. The callback receives a `UIApplication` and all Revit API access goes through it.

### Creating a RevitTask

```csharp
using Scotec.Revit;
var revitTask = new RevitTask("MyRevitOperation");
```

### Running a Task with a Result

```csharp
int elementCount = await revitTask.Run(app =>
{
    var doc = app.ActiveUIDocument.Document;
    return doc.GetElementIds().Count;
});
```

### Running a Task without a Result

```csharp
await revitTask.Run(app =>
{
    TaskDialog.Show("Revit", "Operation executed in Revit context!");
});
```

### Disposing

```csharp
revitTask.Dispose();
```


## DI-Based Usage

When a `Scotec.Revit` add-in is running, `RevitTask` can create a scoped DI lifetime scope for each execution.
Parameters of the delegate are resolved automatically from that scope. This mode requires the add-in to have a
registered DI container (provided by `RevitApp` or `RevitDbApp`).

### What Is Registered by Default

The following services are registered in the scope before your delegate is invoked:

| Type            | Registered when            |
|-----------------|----------------------------|
| `UIApplication` | Always                     |
| `Application`   | Always                     |
| `UIDocument`    | Active document is open    |
| `Document`      | Active document is open    |
| `View`          | Active view is available   |

### Running a DI-Based Task with a Result

Declare a `Delegate` whose parameters are services. The framework resolves them from the scope:

```csharp
Delegate action = (Document doc, IMyService myService) =>
{
    myService.ProcessDocument(doc);
    return doc.Title;
};

string title = await revitTask.Run<string>(action);
```

### Running a DI-Based Task without a Result

```csharp
Delegate action = (Document doc, IMyService myService) =>
{
    myService.ProcessDocument(doc);
};

await revitTask.Run(action);
```

### Registering Additional Services

Pass an optional `configureServices` callback to add services that are not registered in the root container:

```csharp
Delegate action = (Document doc, ITransientHelper helper) =>
{
    helper.Execute(doc);
};

await revitTask.Run(action, services =>
{
    services.AddTransient<ITransientHelper, TransientHelper>();
});
```

### Optional Parameters

The framework distinguishes between required and optional parameters using the same conventions as `RevitCommand`
and `RevitApp`:

| Convention | Example | Behaviour |
|---|---|---|
| Nullable annotation | `IMyService? service` | Optional — receives `null` if not registered |
| Default value of `null` | `IMyService service = null` | Optional — receives `null` if not registered |
| No annotation, no default | `IMyService service` | Required — throws if not registered |

```csharp
Delegate action = (Document doc, IMyService myService, IOptionalLogger? logger) =>
{
    logger?.Log("Starting");
    myService.ProcessDocument(doc);
};

await revitTask.Run(action);
```


## Threading and Context

Revit is single-threaded with respect to its API. Even if your code runs on the main Windows UI thread, that does not
automatically mean you are inside a valid Revit API context. `RevitTask` ensures your delegate is executed inside a
proper `IExternalEventHandler.Execute` call.


## Error Handling

Exceptions thrown inside your delegate are captured and propagated to the returned `Task`.

```csharp
try
{
    await revitTask.Run(app =>
    {
        // Revit API code
    });
}
catch (Exception ex)
{
    // Handle exception
}
```


## Best Practices

### Reuse Instead of Recreate (Recommended)

The Revit API does not require `ExternalEvent` to be a singleton. However:

- There is no benefit to creating many instances.
- Lifecycle management becomes more complex.
- Disposing and recreating repeatedly is unnecessary.

Recommended pattern:

- Create one `RevitTask` per logical scope (e.g., per ViewModel, per tool window, or per add-in).
- Reuse it for multiple operations.
- Dispose it when the scope ends.

### Do Not Create Excessive Instances

While the API does not document a strict limit, `ExternalEvent` objects are managed internally by Revit. Creating many
instances provides no advantage and may complicate resource management.

### Always Dispose

Call `Dispose()` when the instance is no longer required.


## Design Considerations

### Direct Mode vs. DI Mode

Use **direct mode** when:

- The add-in does not use a DI container.
- The callback only needs `UIApplication` and derives everything else from it.
- You want the simplest possible integration.

Use **DI mode** when:

- Services registered via `RevitApp` / `RevitDbApp` need to be injected into the callback.
- You want to keep the callback decoupled from `UIApplication`.
- You need optional service injection.

### When a Singleton Makes Sense

Using a single shared `RevitTask` instance (application-wide singleton) can be beneficial when:

- Your add-in has multiple modeless windows that need coordinated access to the Revit API.
- You implement a centralized task queue or dispatcher service.
- You want strict serialization of all API access through one coordination layer.

This pattern simplifies debugging and ensures predictable execution order.

### When Scoped Instances Are Better

Scoped instances (e.g., per ViewModel or per tool window) are appropriate when:

- Tools are independent and have separate lifecycles.
- Windows can be opened and closed dynamically.
- You want clearer ownership and disposal semantics.

### Avoid Per-Operation Instantiation

Creating a new `RevitTask` for every single operation adds unnecessary object management and provides no architectural
benefit. Prefer reuse within a logical lifecycle boundary.


## Example: WPF Integration

```csharp
public class MyViewModel : IDisposable
{
    private readonly RevitTask _revitTask = new RevitTask("WPF Integration");

    // Direct mode — no DI container required
    public async Task ShowInfoAsync()
    {
        await _revitTask.Run(app =>
        {
            TaskDialog.Show("Info", app.ActiveUIDocument.Document.Title);
        });
    }

    // DI mode — services injected automatically
    public async Task ProcessDocumentAsync()
    {
        Delegate action = (Document doc, IMyService myService) =>
        {
            myService.ProcessDocument(doc);
        };

        await _revitTask.Run(action);
    }

    public void Dispose()
    {
        _revitTask.Dispose();
    }
}
```
