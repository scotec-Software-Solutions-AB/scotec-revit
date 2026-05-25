# RevitApp and RevitDbApp Usage Guide

This document provides a detailed guide on how to use the `RevitApp` and `RevitDbApp` base classes in the `Scotec.Revit` framework. It covers the add-in lifecycle, dependency injection (DI) integration, and how to implement startup and shutdown logic with automatic parameter injection.

## Overview

`RevitApp` and `RevitDbApp` are abstract base classes for implementing Revit external applications:

| Class        | Revit interface              | Use when                                              |
|--------------|------------------------------|-------------------------------------------------------|
| `RevitApp`   | `IExternalApplication`       | Add-in requires Revit UI access (`UIControlledApplication`) |
| `RevitDbApp` | `IExternalDBApplication`     | Add-in works at the database level only (`ControlledApplication`) |

Both classes provide:

- Host and DI container setup via `Microsoft.Extensions.Hosting`
- Explicit lifecycle attributes (`[RevitStartup]`, `[RevitShutdown]`) for free-named methods with automatic DI parameter resolution
- A virtual `OnStartup` / `OnShutdown` overload taking the application instance (`UIControlledApplication` for `RevitApp`, `ControlledApplication` for `RevitDbApp`) for simple cases without additional DI services
- Assembly resolution helpers for add-in isolation contexts

## Startup and Shutdown

### The Recommended Approach: `[RevitStartup]` and `[RevitShutdown]` Attributes

The preferred way to implement startup and shutdown logic is to declare methods decorated with `[RevitStartup]` or `[RevitShutdown]`. The methods may have any name and any combination of DI-resolvable parameters. The framework discovers them automatically at runtime, resolves all parameters from the DI container, and invokes them.

**Example: injecting services at startup**

```csharp
public class MyApp : RevitApp
{
    protected override void OnConfigure(IHostBuilder builder)
    {
        base.OnConfigure(builder);
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IMyService, MyService>();
        });
    }

    [RevitStartup]
    private bool Initialize(IMyService myService, ILogger<MyApp> logger)
    {
        logger.LogInformation("Add-in starting.");
        myService.Initialize();
        return true;
    }

    [RevitShutdown]
    private bool Teardown(IMyService myService, ILogger<MyApp> logger)
    {
        logger.LogInformation("Add-in shutting down.");
        myService.Cleanup();
        return true;
    }
}
```

The attributed method must return `bool`. It may be `private`, `protected`, or `public`. Only one method per type hierarchy may carry `[RevitStartup]` and only one may carry `[RevitShutdown]`.

#### Optional Parameters

The framework distinguishes between required and optional parameters using the same conventions as `RevitCommand`:

| Convention | Example | Behaviour |
|---|---|---|
| Nullable annotation | `IMyService? service` | Optional — receives `null` if not registered |
| Default value of `null` | `IMyService service = null` | Optional — receives `null` if not registered |
| No annotation, no default | `IMyService service` | Required — throws if not registered |

```csharp
[RevitStartup]
private bool Initialize(
    IMyService myService,           // required — throws if not registered
    IOptionalService? optionalA,    // optional — null if not registered
    ILogging logging = null)        // optional — null if not registered
{
    myService.Initialize();
    optionalA?.Register();
    logging?.Log("Started");
    return true;
}
```

### Standard Override: `OnStartup` / `OnShutdown` with the Application Instance

If you need the Revit application instance but no other DI services, override the standard overload directly.

**`RevitApp`** exposes `OnStartup(UIControlledApplication)` / `OnShutdown(UIControlledApplication)` — the most common case:

```csharp
public class MyApp : RevitApp
{
    protected override bool OnStartup(UIControlledApplication application)
    {
        application.CreateRibbonTab("My Tab");
        return true;
    }

    protected override bool OnShutdown(UIControlledApplication application)
    {
        return true;
    }
}
```

**`RevitDbApp`** exposes `OnStartup(ControlledApplication)` / `OnShutdown(ControlledApplication)`:

```csharp
public class MyDbApp : RevitDbApp
{
    protected override bool OnStartup(ControlledApplication application)
    {
        application.DocumentOpened += OnDocumentOpened;
        return true;
    }

    protected override bool OnShutdown(ControlledApplication application)
    {
        application.DocumentOpened -= OnDocumentOpened;
        return true;
    }

    private void OnDocumentOpened(object sender, DocumentOpenedEventArgs e) { }
}
```

> **Note:** The parameter-less `OnStartup()` / `OnShutdown()` overloads are obsolete. Migrate to the typed overloads above, or use a method marked with `[RevitStartup]` / `[RevitShutdown]` for full DI injection.

### Dispatch Priority

The framework selects the method to invoke using the following priority:

1. **`[RevitStartup]` / `[RevitShutdown]` attribute** — any `bool`-returning method marked with the attribute. All parameters are resolved from the DI root service provider.
2. **`OnStartup(UIControlledApplication)` / `OnShutdown(UIControlledApplication)`** (`RevitApp`) or **`OnStartup(ControlledApplication)` / `OnShutdown(ControlledApplication)`** (`RevitDbApp`) — called if overridden in the derived class; the application instance is resolved from DI.
3. **`OnStartup()` / `OnShutdown()`** *(obsolete)* — the virtual parameter-less overload, used when neither of the above is found.

## Services Available at Startup and Shutdown

The following services are registered in the DI container before `OnStartup` / `OnShutdown` are called.

### `RevitApp` (UI application)

| Type                       | Registered when |
|----------------------------|-----------------|
| `UIControlledApplication`  | Always          |
| `AddInId`                  | Always          |
| `ControlledApplication`    | Always          |

### `RevitDbApp` (DB application)

| Type                       | Registered when |
|----------------------------|-----------------|
| `ControlledApplication`    | Always          |
| `AddInId`                  | Always          |

You can inject these types directly into your `[RevitStartup]` / `[RevitShutdown]` methods, or into any service resolved from the container.

## Configuring the DI Container

Override `OnConfigure(IHostBuilder builder)` to register your own services:

```csharp
public class MyApp : RevitApp
{
    protected override void OnConfigure(IHostBuilder builder)
    {
        base.OnConfigure(builder);

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IMyService, MyService>();
            services.AddHostedService<MyBackgroundService>();
        });
    }

    [RevitStartup]
    private bool Initialize(IMyService myService)
    {
        myService.Initialize();
        return true;
    }
}
```

> **Important:** Always call `base.OnConfigure(builder)` — the base implementation registers the Revit-specific services listed in the table above.

## Using RevitDbApp

`RevitDbApp` follows the same pattern but derives from `IExternalDBApplication` and exposes a `ControlledApplication` rather than `UIControlledApplication`:

```csharp
public class MyDbApp : RevitDbApp
{
    protected override void OnConfigure(IHostBuilder builder)
    {
        base.OnConfigure(builder);

        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IDocumentTracker, DocumentTracker>();
        });
    }

    [RevitStartup]
    private bool Initialize(ControlledApplication application, IDocumentTracker tracker)
    {
        application.DocumentOpened += tracker.OnDocumentOpened;
        return true;
    }

    [RevitShutdown]
    private bool Cleanup(ControlledApplication application, IDocumentTracker tracker)
    {
        application.DocumentOpened -= tracker.OnDocumentOpened;
        return true;
    }
}
```

## Assembly Resolution

`RevitAppBase` provides two overridable hooks for resolving assemblies that the runtime cannot locate:

| Method | Triggered by |
|--------|--------------|
| `OnAssemblyResolve(ResolveEventArgs)` | `AppDomain.AssemblyResolve` event |
| `OnAssemblyResolve(AssemblyLoadContext, AssemblyName)` | `AssemblyLoadContext.Resolving` event |

By default both methods attempt to load the missing assembly from the add-in's own directory. Override them to implement custom resolution strategies.

## Add-in Path

Use `GetAddInPath()` to retrieve the directory of the current add-in assembly. This is safe to use even when multiple add-ins share the same `Scotec.Revit` assembly:

```csharp
var path = GetAddInPath();  // returns the directory of the derived RevitApp assembly
```

## Summary

| Feature                              | How to use                                                                         |
|-------------------------------------|------------------------------------------------------------------------------------|
| Startup with DI injection            | Mark a `bool`-returning method with `[RevitStartup]`                              |
| Shutdown with DI injection           | Mark a `bool`-returning method with `[RevitShutdown]`                             |
| Startup with `UIControlledApplication` (`RevitApp`) | Override `OnStartup(UIControlledApplication application)`                         |
| Shutdown with `UIControlledApplication` (`RevitApp`) | Override `OnShutdown(UIControlledApplication application)`                        |
| Startup with `ControlledApplication` (`RevitDbApp`)  | Override `OnStartup(ControlledApplication application)`                           |
| Shutdown with `ControlledApplication` (`RevitDbApp`) | Override `OnShutdown(ControlledApplication application)`                          |
| Simple startup (obsolete)            | Override `OnStartup()` *(obsolete — migrate to the overload above)*               |
| Simple shutdown (obsolete)           | Override `OnShutdown()` *(obsolete — migrate to the overload above)*              |
| Register additional services         | Override `OnConfigure(IHostBuilder builder)`                                       |
| Access DI from anywhere              | Use the `Services` property (`IServiceProvider`) inherited from `RevitAppBase`     |
| UI-level add-in                      | Derive from `RevitApp` (exposes `UIControlledApplication Application`)             |
| DB-level add-in                      | Derive from `RevitDbApp` (exposes `ControlledApplication Application`)             |
