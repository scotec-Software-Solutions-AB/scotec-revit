<<<<<<< HEAD
﻿# RevitCommandAvailability Usage Guide
=======
# RevitCommandAvailability Usage Guide
>>>>>>> origin/main

This document provides a detailed guide on how to use the `RevitCommandAvailability` base class in the `Scotec.Revit` framework. It covers dependency injection (DI) scope creation, how to implement availability logic, and how to register additional services.

## Overview

`RevitCommandAvailability` is an abstract base class for controlling whether a Revit external command is enabled in the ribbon UI. It implements the `IExternalCommandAvailability` interface and provides:

- Dependency injection (DI) scope creation for each availability check
- Extensibility for registering custom services
- Explicit `[RevitCommandAvailabilityCheck]` attribute for free-named methods with automatic DI parameter resolution
- Backward-compatible virtual overrides (`IsCommandAvailable(UIApplication, CategorySet)` and the obsolete `IsCommandAvailable(UIApplication, CategorySet, IServiceProvider)`)

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

<<<<<<< HEAD
- `UIApplication` â€” passed directly
- `CategorySet` â€” passed directly
- `Document`, `UIDocument`, `Application`, `View` â€” registered by the framework
- `IServiceProvider` â€” registered by the framework
- Any type registered via `ConfigureServices`

The attributed method may be `private`, `protected`, or `public`. Only one method per type hierarchy may carry `[RevitCommandAvailabilityCheck]` â€” the framework throws `InvalidOperationException` at runtime if more than one is found.

#### Optional Parameters

The framework distinguishes between required and optional parameters using two conventions â€” not on whether the type is inherently nullable. This applies to all types, including interfaces and classes, which are reference types and therefore always nullable at runtime.

| Convention | Example | Behaviour |
|---|---|---|
| Nullable annotation | `IMyService? service` | Optional â€” receives `null` if not registered |
| Default value of `null` | `IMyService service = null` | Optional â€” receives `null` if not registered |
| No annotation, no default | `IMyService service` | Required â€” throws if not registered |
=======
- `UIApplication` — passed directly
- `CategorySet` — passed directly
- `Document`, `UIDocument`, `Application`, `View` — registered by the framework
- `IServiceProvider` — registered by the framework
- Any type registered via `ConfigureServices`

The attributed method may be `private`, `protected`, or `public`. Only one method per type hierarchy may carry `[RevitCommandAvailabilityCheck]` — the framework throws `InvalidOperationException` at runtime if more than one is found.

#### Optional Parameters

The framework distinguishes between required and optional parameters using two conventions — not on whether the type is inherently nullable. This applies to all types, including interfaces and classes, which are reference types and therefore always nullable at runtime.

| Convention | Example | Behaviour |
|---|---|---|
| Nullable annotation | `IMyService? service` | Optional — receives `null` if not registered |
| Default value of `null` | `IMyService service = null` | Optional — receives `null` if not registered |
| No annotation, no default | `IMyService service` | Required — throws if not registered |
>>>>>>> origin/main

```csharp
[RevitCommandAvailabilityCheck]
private bool CheckAvailability(
<<<<<<< HEAD
    Document document,              // required (T)      â€” throws if not registered
    IMyService? optionalService,    // optional (T?)     â€” null if not registered
    ILogging logging = null)        // optional (= null) â€” null if not registered
=======
    Document document,              // required (T)      — throws if not registered
    IMyService? optionalService,    // optional (T?)     — null if not registered
    ILogging logging = null)        // optional (= null) — null if not registered
>>>>>>> origin/main
{

    logging?.Log("Checking availability");
    return optionalService?.IsFeatureEnabled() ?? true;
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

<<<<<<< HEAD
1. **`[RevitCommandAvailabilityCheck]` attribute** â€” any `bool`-returning method marked with the attribute. All parameters are resolved from DI. Throws `InvalidOperationException` if more than one such method exists in the type hierarchy.
2. **`IsCommandAvailable(UIApplication, CategorySet)`** â€” called directly if overridden in the derived class.
3. **`IsCommandAvailable(UIApplication, CategorySet, IServiceProvider)`** *(obsolete)* â€” called for backward compatibility if none of the above is found.
=======
1. **`[RevitCommandAvailabilityCheck]` attribute** — any `bool`-returning method marked with the attribute. All parameters are resolved from DI. Throws `InvalidOperationException` if more than one such method exists in the type hierarchy.
2. **`IsCommandAvailable(UIApplication, CategorySet)`** — called directly if overridden in the derived class.
3. **`IsCommandAvailable(UIApplication, CategorySet, IServiceProvider)`** *(obsolete)* — called for backward compatibility if none of the above is found.
>>>>>>> origin/main

### Obsolete Overload

`IsCommandAvailable(UIApplication, CategorySet, IServiceProvider)` is obsolete and retained for backward compatibility only. It will not be called if any of the above overloads is present. Migrate to `[RevitCommandAvailabilityCheck]` or the standard `IsCommandAvailable(UIApplication, CategorySet)` overload.

> **Note:** If an unhandled exception occurs during the availability check, the framework catches it and returns `false`, keeping the ribbon button disabled rather than crashing Revit.

## Dependency Injection (DI) Scope

<<<<<<< HEAD
By default, each availability check creates a new child DI lifetime scope using Autofac and `Microsoft.Extensions.DependencyInjection`.
=======
Each availability check creates a new DI scope using Autofac and `Microsoft.Extensions.DependencyInjection`.
>>>>>>> origin/main

### What Is Registered by Default

| Type             | Registered when            |
|------------------|----------------------------|
| `UIApplication`  | Always                     |
| `Application`    | Always                     |
| `CategorySet`    | Always                     |
| `UIDocument`     | Active document is open    |
| `Document`       | Active document is open    |
| `View`           | Active view is available   |

<<<<<<< HEAD
### Controlling Scope Creation: `UseNewScope`

The `UseNewScope` property controls whether a new child scope is created for each availability check:

| Value | Behaviour |
|-------|-----------|
| `true` *(default)* | A new child lifetime scope is created. Context objects and any services registered via `ConfigureServices` are available for injection. |
| `false` | No scope is created. Services are resolved directly from the root container. `ConfigureServices` is not called, and no context objects are registered. |

Override this property when scope creation is not needed — for example, when the check resolves only long-lived singleton services already in the root container:

```csharp
public class MyCommandAvailability : RevitCommandAvailability
{
    protected override bool UseNewScope => false;

    [RevitCommandAvailabilityCheck]
    private bool CheckAvailability(IMyRootService service)
    {
        return service.IsFeatureEnabled();
    }
}
```

The property is `virtual`, so the override may contain arbitrary logic:

```csharp
protected override bool UseNewScope => _settings.UseScopedChecks;
```

> **Note:** When `UseNewScope` is `false`, `UIApplication`, `Document`, `View`, and all other context objects listed in _What Is Registered by Default_ are **not** available for injection.

=======
>>>>>>> origin/main
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
| Standard availability logic    | Override `IsCommandAvailable(UIApplication, CategorySet)`                                     |
<<<<<<< HEAD
| Disable per-check scope        | Override `UseNewScope` and return `false`                                                   |
=======
>>>>>>> origin/main
| Register additional services   | Override `ConfigureServices(IServiceCollection services)`                                     |
| Connect to a command           | Set `AvailabilityClassName` on `PushButtonData` to the availability type name                 |
