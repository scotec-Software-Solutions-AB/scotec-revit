# RevitTask Usage Guide

The `RevitTask` class provides a robust and convenient way to execute operations within the Autodesk Revit API context using the `IExternalEventHandler` mechanism. It is designed to help you safely schedule and run code that interacts with the Revit API, regardless of whether your calling code is on the main UI thread, a background thread, or from an external UI (such as WPF or WinForms).

## Understanding IExternalEventHandler and the Revit API Context

### Why is IExternalEventHandler Needed?

The Revit API enforces strict rules: **all operations that modify or query the Revit database must be executed within the Revit API context**. This context is only available during certain Revit events (such as command execution or external events). If you attempt to access the Revit API from outside this context (for example, from a WPF button click handler, a background thread, or a timer), you will encounter exceptions or undefined behavior.

The `IExternalEventHandler` interface, together with `ExternalEvent`, is Revit's official mechanism for marshaling code execution into the correct API context. You implement `IExternalEventHandler` and register it with an `ExternalEvent`. When you want to run code in the Revit context, you call `ExternalEvent.Raise()`. Revit will then invoke your handler's `Execute` method at a safe time, on the correct context.

### Typical Use Cases

- Running Revit API code in response to UI actions in a WPF/WinForms add-in.
- Scheduling database operations from asynchronous or background workflows.
- Decoupling business/UI logic from direct Revit API calls.

## How RevitTask Supports Safe API Access

`RevitTask` encapsulates the `IExternalEventHandler` pattern and provides a simple, awaitable API for running code in the Revit context. It manages the event handler, synchronization, and result propagation for you.

**Key Features:**
- Schedules your delegate to run in the Revit API context, regardless of the calling thread.
- Supports both synchronous and asynchronous workflows via `Task` and `Task<TResult>`.
- Handles synchronization and result passing automatically.
- Ensures all Revit API access is performed safely and correctly.


## Basic Usage

### 1. Creating a RevitTask
```csharp
using Scotec.Revit;
var revitTask = new RevitTask("MyRevitOperation");
```

### 2. Running a Task with a Result
```csharp
int elementCount = await revitTask.Run(app => 
    { 
        var doc = app.ActiveUIDocument.Document; 
        return doc.GetElementIds().Count; 
    });
```

### 3. Running a Task without a Result
```csharp
await revitTask.Run(app => 
    { 
        TaskDialog.Show("Revit", "Operation executed in Revit context!"); 
    });
```

### 4. Disposing the Task

Always dispose of the `RevitTask` instance when you are done:
```csharp
revitTask.Dispose();
```

## Threading and Context

- **Not just for background threads:**  
  Even if you are on the main UI thread (e.g., in a WPF button click), you cannot directly access the Revit API. You must marshal your code into the Revit context using `IExternalEventHandler`—which is exactly what `RevitTask` does for you.
- **Safe from any context:**  
  Whether your code is running on the UI thread, a background thread, or a timer, `RevitTask` ensures your delegate is executed in the correct Revit API context.

## Error Handling

- Exceptions thrown in your delegate will be propagated to the returned `Task` or `Task<TResult>`.
- Use `try/catch` in your async code to handle errors as needed.

## Best Practices

- **Reuse or Dispose:**  
  You can reuse a single `RevitTask` instance for multiple operations, or create/dispose per operation as needed.
- **Always Dispose:**  
  Call `Dispose()` when finished to release resources.
- **UI Integration:**  
  Use `RevitTask` to bridge between your UI logic and the Revit API safely.

## Example: Using RevitTask in a WPF Add-in
```csharp
public class MyViewModel : IDisposable 
{ 
    private readonly RevitTask _revitTask = new RevitTask("WPF Integration");
    public async Task UpdateModelAsync()
    {
        await _revitTask.Run(app =>
        {
            var doc = app.ActiveUIDocument.Document;
            // ... perform Revit API operations ...
        });
    }

    public void Dispose()
    {
        _revitTask.Dispose();
    }
}
```

**For further details, see the source code and XML documentation in `Scotec.Revit\RevitTask.cs`.**
