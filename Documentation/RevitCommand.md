# RevitCommand Usage Guide

This document provides a detailed guide on how to use the `RevitCommand` base class in the `Scotec.Revit` framework. It covers transaction modes, dependency injection (DI) scope creation, how to implement command logic, and how to hook into the command lifecycle with `BeforeExecute` and `AfterExecute`.

## Overview

`RevitCommand` is an abstract base class for implementing Revit external commands. It provides:

- Transaction management (with multiple modes)
- Failure handling hooks
- Dependency injection (DI) scope creation for each command execution (configurable via `UseNewScope`)
- Extensibility for registering custom services
- Explicit lifecycle attributes (`[RevitCommandBeforeExecute]`, `[RevitCommandExecute]`, `[RevitCommandAfterExecute]`) for free-named methods with automatic DI parameter resolution
- Backward-compatible virtual overrides (`OnExecute(ExternalCommandData, ElementSet)` and the obsolete `OnExecute(ExternalCommandData, IServiceProvider)`)

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

### The Recommended Approach: `[RevitCommandExecute]` Attribute

The preferred way to implement a command is to declare a method with the `[RevitCommandExecute]` attribute. The method can have any name and any combination of DI-resolvable parameters. The framework discovers it automatically at runtime, resolves all parameters from the DI container, and invokes it. `ExternalCommandData` and `ElementSet` are passed through directly without going to DI.

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

    [RevitCommandExecute]
    private Result Execute(IRevitUiContext context, IMyService myService)
    {
        myService.DoWork(context.Document);
        return Result.Succeeded;
    }
}
```

Any combination of DI-registered types is valid as parameters, including:

- `ExternalCommandData` — passed directly
- `ElementSet` — passed directly
- `IRevitContext` — always registered; provides `Application` and `Document`
- `IRevitUiContext` — always registered for commands; extends `IRevitContext` with `UiApplication`, `UiDocument`, and `ActiveView`
- `IServiceProvider` — registered by the framework
- Any type registered via `ConfigureServices`

The attributed method may be `private`, `protected`, or `public`. Only one method per type hierarchy may carry `[RevitCommandExecute]` — the framework throws `InvalidOperationException` at runtime if more than one is found.

#### Optional Parameters

The framework distinguishes between required and optional parameters using two conventions — not on whether the type is inherently nullable. This applies to all types, including interfaces and classes, which are reference types and therefore always nullable at runtime.

| Convention | Example | Behaviour |
|---|---|---|
| Nullable annotation | `IMyService? service` | Optional — receives `null` if not registered |
| Default value of `null` | `IMyService service = null` | Optional — receives `null` if not registered |
| No annotation, no default | `IMyService service` | Required — throws if not registered |

```csharp
[RevitCommandExecute]
private Result Execute(
    IRevitUiContext context,         // required (T)   — always registered
    IMyService myService,           // required (T)   — throws if not registered
    IOptionalService? optionalA,    // optional (T?)  — null if not registered
    ILogging logging = null)        // optional (= null) — null if not registered
{
    optionalA?.Notify();
    logging?.Log("Executing");
    myService.DoWork(context.Document);
    return Result.Succeeded;
}
```

> **Note:** The same conventions apply to `[RevitCommandBeforeExecute]` and `[RevitCommandAfterExecute]`.

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

1. **`[RevitCommandExecute]` attribute** — any `Result`-returning method marked with the attribute. All parameters are resolved from DI. Throws `InvalidOperationException` if more than one such method exists in the type hierarchy.
2. **`OnExecute(ExternalCommandData, ElementSet)`** — called directly if overridden in the derived class.
3. **`OnExecute(ExternalCommandData, IServiceProvider)`** *(obsolete)* — called for backward compatibility if none of the above is found.

### Obsolete Overload

`OnExecute(ExternalCommandData, IServiceProvider)` is obsolete and retained for backward compatibility only. It will not be called if any of the above overloads is present. Migrate to `[RevitCommandExecute]` or the standard `OnExecute(ExternalCommandData, ElementSet)` overload.

Two optional lifecycle methods can be declared on a command class to run code outside the transaction boundary:

| Attribute                      | When it runs                                         | Return type |
|--------------------------------|------------------------------------------------------|-------------|
| `[RevitCommandBeforeExecute]`  | Before the transaction or transaction group is opened | `void`      |
| `[RevitCommandAfterExecute]`   | After the transaction or transaction group is closed  | `void`      |

Both methods follow the same discovery and parameter-resolution rules as `[RevitCommandExecute]`: apply the attribute to a method with any name and any DI-resolvable parameters. `ExternalCommandData` and `ElementSet` are passed through directly. Only one method per type hierarchy may carry each attribute — `InvalidOperationException` is thrown if more than one is found.

```csharp
[RevitTransactionMode(Mode = RevitTransactionMode.Transaction)]
public class MyCommand : RevitCommand
{
    protected override string CommandName => "My Command";

    [RevitCommandBeforeExecute]
    private void Setup(IRevitUiContext context)
    {
        // Runs before the transaction is opened.
        // Suitable for read-only setup, pre-checks, or UI preparation.
    }

    [RevitCommandExecute]
    private Result Execute(IRevitUiContext context, IMyService myService)
    {
        myService.DoWork(context.Document);
        return Result.Succeeded;
    }

    [RevitCommandAfterExecute]
    private void Cleanup(IRevitUiContext context)
    {
        // Runs after the transaction has been committed or rolled back.
        // Suitable for post-processing, notifications, or cleanup.
    }
}
```

> **Note:** `[RevitCommandBeforeExecute]` and `[RevitCommandAfterExecute]` are optional. The command will execute normally if they are not declared.

## Dependency Injection (DI) Scope

By default, each command execution creates a new child DI lifetime scope using Autofac and `Microsoft.Extensions.DependencyInjection`. This scope is disposed automatically when execution completes.

### Controlling Scope Creation: `UseNewScope`

The `UseNewScope` property controls whether a new child scope is created for each execution:

| Value   | Behaviour |
|---------|-----------|
| `true` *(default)* | A new child lifetime scope is created. Default Revit services and any services registered via `ConfigureServices` are available. |
| `false` | No scope is created. Services are resolved directly from the root container. `ConfigureServices` is not called and no Revit context objects are registered. |

Override this property when scope creation is not desired — for example, when the command resolves only long-lived singleton services that are already registered in the root container:

```csharp
public class MyCommand : RevitCommand
{
    protected override bool UseNewScope => false;

    [RevitCommandExecute]
    private Result Execute(IMyRootService service)
    {
        service.DoWork();
        return Result.Succeeded;
    }
}
```

The property is `virtual`, so the override may contain arbitrary logic:

```csharp
protected override bool UseNewScope => _settings.UseScopedExecution;
```

> **Note:** When `UseNewScope` is `false`, the types listed in _What Is Registered by Default_ are **not** available for injection, because they are only registered inside the per-execution scope.

### What Is Registered by Default

The following types are registered in the per-execution scope (only when `UseNewScope` is `true`).
Because commands always run from the Revit UI, both context interfaces are always available:

| Type | Notes |
|------|-------|
| `IRevitContext` | Always registered — provides `Application` and `Document` |
| `IRevitUiContext` | Always registered — extends `IRevitContext` with `UiApplication`, `UiDocument`, and `ActiveView` |

The `Document` and `UiDocument` properties may be `null` when no document is currently open.

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

    [RevitCommandExecute]
    private Result Execute(IRevitUiContext context, IMyService myService, MyDependency dep)
    {
        myService.DoWork(context.Document);
        return Result.Succeeded;
    }
}
```

### DI Scope Creation Flow

1. If `UseNewScope` is `true`, a new child DI scope is created; default Revit-related services are registered and `ConfigureServices` is called. Otherwise, the root container is used directly.
2. `BeforeExecute` is invoked (if declared), with parameters resolved from the scope or root container.
3. The transaction or transaction group is opened (according to the configured mode).
4. `OnExecute` is invoked with parameters resolved from the scope or root container.
5. The transaction is committed or rolled back.
6. `AfterExecute` is invoked (if declared), with parameters resolved from the scope or root container.
7. If a scope was created, it is disposed.

## Summary

| Feature                        | How to use                                                                 |
|-------------------------------|----------------------------------------------------------------------------|
| Transaction mode (attribute)   | Apply `[RevitTransactionMode(...)]` to your command class                 |
| Transaction mode (property)    | Override `TransactionMode` in your command class                          |
| Custom command logic           | Mark a `Result`-returning method with `[RevitCommandExecute]`             |
| Standard command logic         | Override `OnExecute(ExternalCommandData, ElementSet)`                     |
| Pre-transaction setup          | Mark a `void` method with `[RevitCommandBeforeExecute]`                   |
| Post-transaction cleanup       | Mark a `void` method with `[RevitCommandAfterExecute]`                    |
| Register additional services   | Override `ConfigureServices(IServiceCollection services)`                 |
| Disable per-execution scope    | Override `UseNewScope` and return `false`                                 |
