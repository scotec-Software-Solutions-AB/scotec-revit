# RevitCommand Usage Guide

This document provides a detailed guide on how to use the `RevitCommand` base class in the `Scotec.Revit` framework. It covers transaction modes, dependency injection (DI) scope creation, how to implement command logic, and how to hook into the command lifecycle with `BeforeExecute` and `AfterExecute`.

## Overview

`RevitCommand` is an abstract base class for implementing Revit external commands. It provides:

- Transaction management (with multiple modes)
- Failure handling hooks
- Dependency injection (DI) scope creation for each command execution
- Extensibility for registering custom services
- Automatic parameter resolution for `OnExecute`, `BeforeExecute`, and `AfterExecute` methods

## Transaction Modes

The `RevitCommand` class supports several transaction modes, controlled via the `RevitTransactionModeAttribute` or the (deprecated) `NoTransaction` property.

### Supported Modes

| Mode                          | Description                                                                                  | Behavior                                                                                  |
|-------------------------------|----------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|
| `None`                        | No transaction is created.                                                                   | Command executes without transaction. User manages transactions manually if needed.        |
| `Transaction`                 | A single transaction is created for the command.                                             | All changes are committed if the command succeeds.                                        |
| `TransactionGroup`            | A transaction group is created, allowing multiple transactions to be grouped.                | Group is assimilated and committed if the command succeeds.                               |
| `TransactionWithRollback`     | A single transaction is created, but changes are rolled back after execution.                | All changes are discarded, even if the command succeeds.                                  |
| `TransactionGroupWithRollback`| A transaction group is created, but changes are rolled back after execution.                 | All changes are discarded, even if the command succeeds.                                  |
| `ReadOnly`                    | No transaction is started; the command runs in read-only mode.                               | No changes are allowed; command can only read data from the document.                     |

### How to Specify Transaction Mode

There are two ways to configure the transaction mode for a command:

#### Option 1: `RevitTransactionModeAttribute` (recommended)

Apply the attribute to your command class:

```csharp
[RevitTransactionMode(Mode = RevitTransactionMode.Transaction)]
public class MyCommand : RevitCommand
{
    // ...
}
```

#### Option 2: Override the `TransactionMode` Property

Override the `TransactionMode` property in your command class. This is useful when the mode needs to be determined at runtime or when you prefer a code-based approach over attributes:

```csharp
public class MyCommand : RevitCommand
{
    protected override RevitTransactionMode TransactionMode => RevitTransactionMode.Transaction;

    // ...
}
```

The `TransactionMode` property is used as the fallback when no `RevitTransactionModeAttribute` is applied to the class. Its default value is `RevitTransactionMode.Transaction`.

> **Note:** `RevitTransactionMode.ReadOnly` is not supported by the `TransactionMode` property. Read-only mode can only be applied through the source generator. If `ReadOnly` is returned, the framework treats it as `RevitTransactionMode.None`.

**Priority:** The `RevitTransactionModeAttribute` takes precedence over the `TransactionMode` property if both are present.

> **Note:** If `NoTransaction` is set to `true`, it overrides both the attribute and the property, and disables transaction management. This property is deprecated — use the attribute or `TransactionMode` instead.

## Implementing Command Logic

### The Recommended Approach: Custom `OnExecute` Overload

The preferred way to implement a command is to declare an `OnExecute` method with whatever parameters your command needs. The framework discovers it automatically at runtime, resolves all parameters from the DI container, and invokes it. `ExternalCommandData` and `ElementSet` are passed through directly without going to DI.

**Example: injecting a document and a custom service**

```csharp
[RevitTransactionMode(Mode = RevitTransactionMode.Transaction)]
public class MyCommand : RevitCommand
{
    protected override string CommandName => "My Command";

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IMyService, MyService>();
    }

    private Result OnExecute(Document document, IMyService myService)
    {
        // document and myService are resolved from the DI scope automatically.
        myService.DoWork(document);
        return Result.Succeeded;
    }
}
```

Any combination of DI-registered types is valid as parameters, including:

- `ExternalCommandData` — passed directly
- `ElementSet` — passed directly
- `Document`, `UIDocument`, `UIApplication`, `Application`, `View` — registered by the framework
- `IServiceProvider` — registered by the framework
- Any type registered via `ConfigureServices`

The custom overload may be `private`, `protected`, or `public`. The framework uses reflection to discover it.

#### Optional (Nullable) Parameters

Parameters with a nullable type are treated as optional. If the service is not registered in the DI scope, the framework passes `null` instead of throwing. Non-nullable parameters are required and will cause an exception if not registered.

```csharp
private Result OnExecute(Document document, IMyService myService, IOptionalService? optionalService)
{
    // optionalService will be null if IOptionalService is not registered in the DI scope.
    optionalService?.Notify();
    myService.DoWork(document);
    return Result.Succeeded;
}
```

> **Note:** The same optional parameter rule applies to `BeforeExecute` and `AfterExecute`.

> **Important:** Only one custom `OnExecute` overload should be declared per class. If the class declares a method named `OnExecute` that returns `Result` and whose parameter list matches neither of the two standard signatures (see below), the framework treats it as the custom overload and invokes it.

### Standard Override: `OnExecute(ExternalCommandData, ElementSet)`

If you do not need DI-resolved parameters, you can override the standard `OnExecute(ExternalCommandData, ElementSet)` overload directly:

```csharp
[RevitTransactionMode(Mode = RevitTransactionMode.Transaction)]
public class MyCommand : RevitCommand
{
    protected override string CommandName => "My Command";

    protected override Result OnExecute(ExternalCommandData commandData, ElementSet elements)
    {
        var document = commandData.Application.ActiveUIDocument?.Document;
        // Perform changes to the document
        return Result.Succeeded;
    }
}
```

### Dispatch Priority

The framework selects the method to invoke using the following priority:

1. **Custom `OnExecute` overload** — any `Result`-returning method named `OnExecute` whose parameters match neither standard signature. All parameters are resolved from DI.
2. **`OnExecute(ExternalCommandData, ElementSet)`** — called directly if overridden in the derived class.
3. **`OnExecute(ExternalCommandData, IServiceProvider)`** *(obsolete)* — called for backward compatibility if neither of the above is found.

### Obsolete Overload

`OnExecute(ExternalCommandData, IServiceProvider)` is obsolete and retained for backward compatibility only. It will not be called if either of the above overloads is present in the derived class. Migrate to the standard `OnExecute(ExternalCommandData, ElementSet)` or the custom overload pattern.

## Command Lifecycle Hooks

Two optional lifecycle methods can be declared on a command class to run code outside the transaction boundary:

| Method          | When it runs                                    | Return type |
|-----------------|-------------------------------------------------|-------------|
| `BeforeExecute` | Before the transaction or transaction group is opened | `void`      |
| `AfterExecute`  | After the transaction or transaction group is closed  | `void`      |

Both methods follow the same discovery and parameter-resolution rules as custom `OnExecute` overloads: declare them with any DI-resolvable parameters and the framework will find and invoke them automatically. `ExternalCommandData` and `ElementSet` are passed through directly.

```csharp
[RevitTransactionMode(Mode = RevitTransactionMode.Transaction)]
public class MyCommand : RevitCommand
{
    protected override string CommandName => "My Command";

    private void BeforeExecute(UIApplication uiApplication)
    {
        // Runs before the transaction is opened.
        // Suitable for read-only setup, pre-checks, or UI preparation.
    }

    private Result OnExecute(Document document, IMyService myService)
    {
        myService.DoWork(document);
        return Result.Succeeded;
    }

    private void AfterExecute(UIApplication uiApplication)
    {
        // Runs after the transaction has been committed or rolled back.
        // Suitable for post-processing, notifications, or cleanup.
    }
}
```

> **Note:** `BeforeExecute` and `AfterExecute` are optional. The command will execute normally if they are not declared.

## Dependency Injection (DI) Scope

Each command execution creates a new DI scope using Autofac and `Microsoft.Extensions.DependencyInjection`.

### What Is Registered by Default

| Type             | Registered when            |
|------------------|----------------------------|
| `UIApplication`  | Always                     |
| `Application`    | Always                     |
| `UIDocument`     | Active document is open    |
| `Document`       | Active document is open    |
| `View`           | Active view is available   |

### Registering Additional Services

Override `ConfigureServices(IServiceCollection services)` to add custom services to the scope:

```csharp
[RevitTransactionMode(Mode = RevitTransactionMode.TransactionGroup)]
public class MyCommand : RevitCommand
{
    protected override string CommandName => "My Command";

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IMyService, MyService>();
        services.AddTransient<MyDependency>();
    }

    private Result OnExecute(Document document, IMyService myService, MyDependency dep)
    {
        myService.DoWork(document);
        return Result.Succeeded;
    }
}
```

### DI Scope Creation Flow

1. The framework creates a new DI scope for each command execution.
2. Default Revit-related services are registered in the scope.
3. `ConfigureServices` is called to register additional services.
4. `BeforeExecute` is invoked (if declared), with parameters resolved from the scope.
5. The transaction or transaction group is opened (according to the configured mode).
6. `OnExecute` is invoked with parameters resolved from the scope.
7. The transaction is committed or rolled back.
8. `AfterExecute` is invoked (if declared), with parameters resolved from the scope.

## Summary

| Feature                        | How to use                                                                 |
|-------------------------------|----------------------------------------------------------------------------|
| Transaction mode (attribute)   | Apply `[RevitTransactionMode(...)]` to your command class                 |
| Transaction mode (property)    | Override `TransactionMode` in your command class                          |
| Custom command logic           | Declare `OnExecute` with DI-resolvable parameters                         |
| Standard command logic         | Override `OnExecute(ExternalCommandData, ElementSet)`                     |
| Pre-transaction setup          | Declare `BeforeExecute` with DI-resolvable parameters                     |
| Post-transaction cleanup       | Declare `AfterExecute` with DI-resolvable parameters                      |
| Register additional services   | Override `ConfigureServices(IServiceCollection services)`                 |
