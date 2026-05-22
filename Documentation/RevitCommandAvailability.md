# RevitCommandAvailability Usage Guide

This document provides a detailed guide on how to use the `RevitCommandAvailability` base class in the `Scotec.Revit` framework. It covers dependency injection (DI) scope creation, how to implement availability logic, and how to register additional services.

## Overview

`RevitCommandAvailability` is an abstract base class for controlling whether a Revit external command is enabled in the ribbon UI. It implements the `IExternalCommandAvailability` interface and provides:

- Dependency injection (DI) scope creation for each availability check
- Extensibility for registering custom services
- Explicit `[RevitCommandAvailabilityCheck]` attribute for free-named methods with automatic DI parameter resolution
- Backward-compatible automatic parameter resolution for `IsCommandAvailable` methods (without attribute)

## Implementing Availability Logic

### The Recommended Approach: `[RevitCommandAvailabilityCheck]` Attribute

The preferred way to implement availability logic is to declare a method with the `[RevitCommandAvailabilityCheck]` attribute. The method can have any name and any combination of DI-resolvable parameters. The framework discovers it automatically at runtime, resolves all parameters from the DI container, and invokes it. `UIApplication` and `CategorySet` are passed through directly without going to DI.

**Example: injecting a document and a custom service**

```csharp
public class MyCommandAvailability : RevitCommandAvailability
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IMyService, MyService>();
    }

    [RevitCommandAvailabilityCheck]
    private bool CheckAvailability(Document document, CategorySet selectedCategories, IMyService myService)
    {
        // document, selectedCategories, and myService are resolved automatically.
        return document is not null && myService.IsFeatureEnabled();
    }
}
```

Any combination of DI-registered types is valid as parameters, including:

- `UIApplication` — passed directly
- `CategorySet` — passed directly
- `Document`, `UIDocument`, `Application`, `View` — registered by the framework
- `IServiceProvider` — registered by the framework
- Any type registered via `ConfigureServices`

The attributed method may be `private`, `protected`, or `public`. Only one method per type hierarchy may carry `[RevitCommandAvailabilityCheck]` — the framework throws `InvalidOperationException` at runtime if more than one is found.

#### Optional (Nullable) Parameters

Parameters with a nullable type are treated as optional. If the service is not registered in the DI scope, the framework passes `null` instead of throwing. Non-nullable parameters are required and will cause an exception if not registered.

```csharp
[RevitCommandAvailabilityCheck]
private bool CheckAvailability(Document? document, IMyService? optionalService)
{
    // document and optionalService will be null if not registered or not available.
    if (document is null)
    {
        return false;
    }

    return optionalService?.IsFeatureEnabled() ?? true;
}
```

### Fallback: Custom `IsCommandAvailable` Overload (method name–based, no attribute)

If no `[RevitCommandAvailabilityCheck]` method is found, the framework falls back to discovering a method named `IsCommandAvailable` whose parameter list differs from both standard signatures. This approach still works but is less explicit than using the attribute.

```csharp
public class MyCommandAvailability : RevitCommandAvailability
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IMyService, MyService>();
    }

    private bool IsCommandAvailable(Document document, CategorySet selectedCategories, IMyService myService)
    {
        return document is not null && myService.IsFeatureEnabled();
    }
}
```

### Standard Override: `IsCommandAvailable(UIApplication, CategorySet)`

If you do not need DI-resolved parameters, override the standard `IsCommandAvailable(UIApplication, CategorySet)` overload directly:

```csharp
public class MyCommandAvailability : RevitCommandAvailability
{
    protected override bool IsCommandAvailable(UIApplication uiApplication, CategorySet selectedCategories)
    {
        return uiApplication.ActiveUIDocument is not null;
    }
}
```

### Dispatch Priority

The framework selects the method to invoke using the following priority:

1. **`[RevitCommandAvailabilityCheck]` attribute** — any `bool`-returning method marked with the attribute. All parameters are resolved from DI. Throws `InvalidOperationException` if more than one such method exists in the type hierarchy.
2. **Custom `IsCommandAvailable` overload** — any `bool`-returning method named `IsCommandAvailable` whose parameters match neither standard signature. All parameters are resolved from DI.
3. **`IsCommandAvailable(UIApplication, CategorySet)`** — called directly if overridden in the derived class.
4. **`IsCommandAvailable(UIApplication, CategorySet, IServiceProvider)`** *(obsolete)* — called for backward compatibility if none of the above is found.

### Obsolete Overload

`IsCommandAvailable(UIApplication, CategorySet, IServiceProvider)` is obsolete and retained for backward compatibility only. It will not be called if any of the above overloads is present. Migrate to `[RevitCommandAvailabilityCheck]` or the standard `IsCommandAvailable(UIApplication, CategorySet)` overload.

> **Note:** If an unhandled exception occurs during the availability check, the framework catches it and returns `false`, keeping the ribbon button disabled rather than crashing Revit.

## Dependency Injection (DI) Scope

Each availability check creates a new DI scope using Autofac and `Microsoft.Extensions.DependencyInjection`.

### What Is Registered by Default

| Type             | Registered when            |
|------------------|----------------------------|
| `UIApplication`  | Always                     |
| `Application`    | Always                     |
| `CategorySet`    | Always                     |
| `UIDocument`     | Active document is open    |
| `Document`       | Active document is open    |
| `View`           | Active view is available   |

### Registering Additional Services

Override `ConfigureServices(IServiceCollection services)` to add custom services to the scope:

```csharp
public class MyCommandAvailability : RevitCommandAvailability
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IMyService, MyService>();
        services.AddTransient<MyDependency>();
    }

    [RevitCommandAvailabilityCheck]
    private bool CheckAvailability(Document document, IMyService myService, MyDependency dep)
    {
        return document is not null && myService.IsFeatureEnabled();
    }
}
```

### DI Scope Creation Flow

1. The framework creates a new DI scope for each availability check.
2. Default Revit-related services are registered in the scope.
3. `ConfigureServices` is called to register additional services.
4. The `[RevitCommandAvailabilityCheck]`-attributed method is invoked (or the appropriate fallback).
5. The scope is disposed.

## Summary

| Feature                        | How to use                                                                                     |
|-------------------------------|------------------------------------------------------------------------------------------------|
| Custom availability logic      | Mark a `bool`-returning method with `[RevitCommandAvailabilityCheck]`                         |
| Fallback availability logic    | Declare `IsCommandAvailable` with DI-resolvable parameters (name-based, no attribute)         |
| Standard availability logic    | Override `IsCommandAvailable(UIApplication, CategorySet)`                                     |
| Register additional services   | Override `ConfigureServices(IServiceCollection services)`                                     |
| Connect to a command           | Set `AvailabilityClassName` on `PushButtonData` to the availability type name                 |
