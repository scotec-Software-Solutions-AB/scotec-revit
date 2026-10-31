# RevitUpdater

`RevitUpdater` is the base class for implementing Revit DMU (Dynamic Model Updater) add-ins. It wraps `IUpdater`, handles registration and disposal automatically, and integrates with the framework's dependency injection (DI) system.

Each time Revit triggers the updater — after a transaction that matches the registered triggers commits — a new DI scope is created. Services registered in the application's DI container, as well as Revit objects available at that point, are resolved and injected into the execute method automatically.

## Implementing an Updater

### Required Members

Derive from `RevitUpdater` and implement the following abstract members:

| Member                        | Purpose                                                                 |
|-------------------------------|-------------------------------------------------------------------------|
| `GetUpdaterId()`              | Returns the unique `UpdaterId` for this updater.                        |
| `GetChangePriority()`         | Returns the `ChangePriority` for this updater.                          |
| `GetUpdaterName()`            | Returns a human-readable name for this updater.                         |
| `GetAdditionalInformation()`  | Returns additional descriptive information about this updater.          |
| `OnRegisterUpdater()`         | Called after registration. Register your `AddTrigger` calls here.      |

### Trigger Registration

`OnRegisterUpdater` is called immediately after the updater is registered with Revit. Use `UpdaterRegistry.AddTrigger` here to define which element changes should trigger execution.

```csharp
protected override void OnRegisterUpdater()
{
    UpdaterRegistry.AddTrigger(
        GetUpdaterId(),
        new ElementClassFilter(typeof(Wall)),
        Element.GetChangeTypeGeometry());
}
```

> **Note:** The updater's `Execute` method is called only when a committed transaction modifies elements that match the registered triggers. It is not called on every document change.

## Implementing the Execute Logic

### The Recommended Approach: `[RevitUpdaterExecute]` Attribute

The preferred way to implement execute logic is to declare a method with the `[RevitUpdaterExecute]` attribute. The method must return `void`. It can have any name and any combination of DI-resolvable parameters. The framework discovers it automatically at runtime, resolves all parameters from the DI scope, and invokes it. `UpdaterData` is passed through directly.

**Example: injecting context and a custom service**

```csharp
public class MyUpdater : RevitUpdater
{
    public MyUpdater(AddInId addInId) : base(addInId) { }

    public override UpdaterId GetUpdaterId() => new UpdaterId(AddInId, new Guid("..."));
    public override ChangePriority GetChangePriority() => ChangePriority.Structure;
    public override string GetUpdaterName() => "My Updater";
    public override string GetAdditionalInformation() => "Updates wall data on geometry change.";

    protected override void OnRegisterUpdater()
    {
        UpdaterRegistry.AddTrigger(
            GetUpdaterId(),
            new ElementClassFilter(typeof(Wall)),
            Element.GetChangeTypeGeometry());
    }

    [RevitUpdaterExecute]
    private void OnExecute(UpdaterData data, IRevitContext context, IMyService myService)
    {
        // data is passed through directly.
        // context and myService are resolved from the DI scope.
        foreach (var id in data.GetModifiedElementIds(Element.GetChangeTypeGeometry()))
        {
            var wall = context.Document.GetElement(id) as Wall;
            if (wall is not null)
            {
                myService.ProcessWall(wall);
            }
        }
    }
}
```

Any combination of DI-registered types is valid as parameters, including:

- `UpdaterData` — passed through directly
- `IRevitContext` — always registered; provides `Application` and `Document`
- `IServiceProvider` — registered by the framework
- Any type registered in the application’s DI container

Parameters declared as nullable (e.g. `IMyService?`) are treated as optional and receive `null` when not registered. Non-nullable parameters are required and will cause an exception if not registered.

### The Standard Approach: Override `OnExecute`

Override `OnExecute(UpdaterData data)` when DI parameter injection is not needed:

```csharp
public class MyUpdater : RevitUpdater
{
    public MyUpdater(AddInId addInId) : base(addInId) { }

    public override UpdaterId GetUpdaterId() => new UpdaterId(AddInId, new Guid("..."));
    public override ChangePriority GetChangePriority() => ChangePriority.Structure;
    public override string GetUpdaterName() => "My Updater";
    public override string GetAdditionalInformation() => "Updates wall data on geometry change.";

    protected override void OnRegisterUpdater()
    {
        UpdaterRegistry.AddTrigger(
            GetUpdaterId(),
            new ElementClassFilter(typeof(Wall)),
            Element.GetChangeTypeGeometry());
    }

    protected override void OnExecute(UpdaterData data)
    {
        var document = data.GetDocument();
        // Implement update logic here.
    }
}
```

> **Note:** `OnExecute(UpdaterData)` is only called when no method marked with `[RevitUpdaterExecute]` is found in the type hierarchy. Only one method per class hierarchy may carry the attribute — `InvalidOperationException` is thrown if more than one is found.

## Dependency Injection (DI) Scope

Each updater execution creates a new DI scope using Autofac and `Microsoft.Extensions.DependencyInjection`.

### What Is Registered by Default

| Type | Notes |
|------|-------|
| `UpdaterData` | Always registered — passed through directly |
| `IRevitContext` | Always registered — provides `Application` and `Document` |

### Registering Additional Services

Override `ConfigureServices(IServiceCollection services)` to add custom services to the scope:

```csharp
public class MyUpdater : RevitUpdater
{
    public MyUpdater(AddInId addInId) : base(addInId) { }

    // ... abstract members ...

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IMyService, MyService>();
    }

    [RevitUpdaterExecute]
    private void OnExecute(UpdaterData data, IRevitContext context, IMyService myService)
    {
        myService.ProcessChanges(data, context.Document);
    }
}
```

### DI Scope Creation Flow

1. The framework creates a new DI scope for each updater execution.
2. `UpdaterData` and `IRevitContext` are registered in the scope.
3. `ConfigureServices` is called to register additional services.
4. The method marked with `[RevitUpdaterExecute]` is invoked with parameters resolved from the scope.
5. If no attributed method is found, `OnExecute(UpdaterData)` is called instead.
6. The scope is disposed.

## Disposal and Cleanup

`RevitUpdater` implements `IDisposable`. When disposed, the updater is automatically unregistered from Revit's `UpdaterRegistry`. Dispose the updater instance in `OnShutdown` of your `RevitApp` or `RevitDbApp`.

```csharp
protected override bool OnShutdown(UIControlledApplication application)
{
    _myUpdater?.Dispose();
    return true;
}
```

## Summary

| Feature                        | How to use                                                                 |
|-------------------------------|----------------------------------------------------------------------------|
| Custom update logic (DI)       | Mark a `void` method with `[RevitUpdaterExecute]`                         |
| Standard update logic          | Override `OnExecute(UpdaterData data)`                                    |
| Register trigger conditions    | Use `UpdaterRegistry.AddTrigger` inside `OnRegisterUpdater()`             |
| Register additional services   | Override `ConfigureServices(IServiceCollection services)`                 |
| Cleanup / unregistration       | Dispose the updater instance in your application's `OnShutdown`           |
