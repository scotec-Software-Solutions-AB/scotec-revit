# RevitCommandAvailability Usage Guide

This document provides a detailed guide on how to use the `RevitCommandAvailability` base class in the `Scotec.Revit` framework. It covers dependency injection (DI) scope creation, how to implement availability logic, and how to register additional services.

## Overview

`RevitCommandAvailability` is an abstract base class for controlling whether a Revit external command is enabled in the ribbon UI. It implements the `IExternalCommandAvailability` interface and provides:

- Dependency injection (DI) scope creation for each availability check
- Extensibility for registering custom services
- Automatic parameter resolution for `IsCommandAvailable` methods

## Implementing Availability Logic

### The Recommended Approach: Custom `IsCommandAvailable` Overload

The preferred way to implement availability logic is to declare an `IsCommandAvailable` method with whatever parameters your check needs. The framework discovers it automatically at runtime, resolves all parameters from the DI container, and invokes it. `UIApplication` and `CategorySet` are passed through directly without going to DI.

**Example: injecting a document and a custom service**

```csharp
public class MyCommandAvailability : RevitCommandAvailability
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IMyService, MyService>();
    }

    private bool IsCommandAvailable(Document document, CategorySet selectedCategories, IMyService myService)
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

The custom overload may be `private`, `protected`, or `public`. The framework uses reflection to discover it.

#### Optional (Nullable) Parameters

Parameters with a nullable type are treated as optional. If the service is not registered in the DI scope, the framework passes `null` instead of throwing. Non-nullable parameters are required and will cause an exception if not registered.

```csharp
private bool IsCommandAvailable(Document? document, IMyService? optionalService)
{
    // document and optionalService will be null if not registered or not available.
    if (document is null)
    {
        return false;
    }

    return optionalService?.IsFeatureEnabled() ?? true;
}
```

> **Important:** Only one custom `IsCommandAvailable` overload should be declared per class. If the class declares a method named `IsCommandAvailable` that returns `bool` and whose parameter list matches neither of the two standard signatures (see below), the framework treats it as the custom overload and invokes it.

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

1. **Custom `IsCommandAvailable` overload** — any `bool`-returning method named `IsCommandAvailable` whose parameters match neither standard signature. All parameters are resolved from DI.
2. **`IsCommandAvailable(UIApplication, CategorySet)`** — called directly if overridden in the derived class.
3. **`IsCommandAvailable(UIApplication, CategorySet, IServiceProvider)`** *(obsolete)* — called for backward compatibility if neither of the above is found.

### Obsolete Overload

`IsCommandAvailable(UIApplication, CategorySet, IServiceProvider)` is obsolete and retained for backward compatibility only. It will not be called if either of the above overloads is present in the derived class. Migrate to the standard `IsCommandAvailable(UIApplication, CategorySet)` or the custom overload pattern.

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

    private bool IsCommandAvailable(Document document, IMyService myService, MyDependency dep)
    {
        return document is not null && myService.IsFeatureEnabled();
    }
}
```

### DI Scope Creation Flow

1. The framework creates a new DI scope for each availability check.
2. Default Revit-related services are registered in the scope.
3. `ConfigureServices` is called to register additional services.
4. `IsCommandAvailable` is invoked with parameters resolved from the scope.
5. The scope is disposed.

## Summary

| Feature                        | How to use                                                                     |
|-------------------------------|--------------------------------------------------------------------------------|
| Custom availability logic      | Declare `IsCommandAvailable` with DI-resolvable parameters                    |
| Standard availability logic    | Override `IsCommandAvailable(UIApplication, CategorySet)`                     |
| Register additional services   | Override `ConfigureServices(IServiceCollection services)`                     |
| Connect to a command           | Set `AvailabilityClassName` on `PushButtonData` to the availability type name |
