# RevitCommand Usage Guide

This document provides a detailed guide on how to use the `RevitCommand` base class in the `Scotec.Revit` framework. It covers transaction modes, dependency injection (DI) scope creation, and how to register additional services for your command.

---

## Overview

`RevitCommand` is an abstract base class for implementing Revit external commands. It provides:

- Transaction management (with multiple modes)
- Failure handling hooks
- Dependency injection (DI) scope creation for each command execution
- Extensibility for registering custom services

---

## Transaction Modes

The `RevitCommand` class supports several transaction modes, controlled via the `RevitTransactionModeAttribute` or the (deprecated) `NoTransaction` property.

### Supported Modes

| Mode                          | Description                                                                                  | Behavior                                                                                  |
|-------------------------------|----------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------|
| `None`                        | No transaction is created.                                                                   | Command executes without transaction. User manages transactions manually if needed.        |
| `SingleTransaction`           | A single transaction is created for the command.                                             | All changes are committed if the command succeeds.                                        |
| `TransactionGroup`            | A transaction group is created, allowing multiple transactions to be grouped.                | Group is assimilated and committed if the command succeeds.                               |
| `SingleTransactionRollback`   | A single transaction is created, but changes are rolled back after execution.                | All changes are discarded, even if the command succeeds.                                  |
| `TransactionGroupRollback`    | A transaction group is created, but changes are rolled back after execution.                 | All changes are discarded, even if the command succeeds.                                  |

### How to Specify Transaction Mode

#### Using Attribute

Apply the `RevitTransactionModeAttribute` to your command class:

```csharp
[RevitTransactionMode(Mode = RevitTransactionMode.SingleTransaction)] 
public class MyCommand : RevitCommand { // ... }
```


> **Note:** If `NoTransaction` is set to `true`, it overrides the attribute and disables transaction management.

---

## Working with Transaction Modes

- **None:** No transaction is started. You must handle transactions manually if you need them.
- **SingleTransaction:** The framework starts a transaction, executes your command, and commits if successful.
- **TransactionGroup:** The framework starts a transaction group, executes your command, assimilates and commits if successful.
- **Rollback Modes:** The framework starts a transaction or group, executes your command, but always rolls back/discards changes.

**Example: Single Transaction Mode**

```csharp
[RevitTransactionMode(Mode = RevitTransactionMode.SingleTransaction)] 
public class MyCommand : RevitCommand 
{ 
    protected override string CommandName => "My Command";

    protected override Result OnExecute(ExternalCommandData commandData, IServiceProvider services)
    {
        // Perform changes to the document
        return Result.Succeeded;
    }
}
```


---

## Dependency Injection (DI) Scope

Each command execution creates a new DI scope using Autofac and Microsoft.Extensions.DependencyInjection.

### What is Registered by Default?

- The current `Document` (if available)
- The current `View` (if available)
- The `UIApplication`
- The `JournalData`

### Registering Additional Services

To add custom services to the DI scope, override the `ConfigureServices(IServiceCollection services)` method in your command class:

```csharp
public class MyCommand : RevitCommand 
{ 
    protected override string CommandName => "My Command";
    protected override void ConfigureServices(IServiceCollection services)
    {
        // Register custom services
        services.AddSingleton<IMyService, MyService>();
        services.AddTransient<MyDependency>();
    }

    protected override Result OnExecute(ExternalCommandData commandData, IServiceProvider services)
    {
        var myService = services.GetService<IMyService>();
        // Use your service
        return Result.Succeeded;
    }
}
```


### DI Scope Creation Flow

1. The framework creates a new DI scope for each command execution.
2. Default Revit-related services are registered.
3. `ConfigureServices` is called, allowing you to add custom services.
4. The DI container is populated and resolved for use in your command.

---

## Summary

- Use `RevitCommand` as your base for Revit external commands.
- Specify transaction mode via attribute for clear, modern behavior.
- Override `ConfigureServices` to register custom services for your command's DI scope.
- Use the injected `IServiceProvider` in `OnExecute` to access your services.

---

**For further details, see the source code and XML documentation in `Scotec.Revit\RevitCommand.cs`.**