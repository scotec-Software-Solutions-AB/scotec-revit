# RevitEventHandler

`RevitEventHandler<TEventArgs>` is the generic base class for handling Revit application events with automatic dependency injection (DI) scope per invocation. Each concrete handler wraps one specific Revit event, self-subscribes in the constructor, and self-unsubscribes on disposal.

Each time the subscribed event fires, a new DI scope is created by default. An `IRevitContext` is registered automatically in every invocation scope. For UI-specific events, `IRevitUiContext` is registered as well. Services from the application's DI container are resolved and injected into the handler method. Scope creation can be disabled per handler via `UseNewScope`.

## Overview

The framework provides two sets of pre-built concrete handler base classes:

| Set | Source application | Events |
|-----|--------------------|--------|
| Document handlers | `ControlledApplication` | Open, create, close, save, sync, print, export, import, change |
| UI handlers | `UIControlledApplication` | View activation, idling, dialogs, application closing |

Document handlers are available for both `RevitApp` (`IExternalApplication`) and `RevitDbApp` (`IExternalDBApplication`). UI handlers require `RevitApp`.

## Implementing a Handler

### The Recommended Approach: `[RevitEventHandlerExecute]` Attribute

The framework discovers the method at runtime, resolves all parameters from the per-invocation DI scope, and calls it.

**Example: react to a document being opened**

```csharp
public class MyDocumentOpenedHandler : RevitDocumentOpenedHandler
{
    public MyDocumentOpenedHandler(ControlledApplication application)
        : base(application) { }

    [RevitEventHandlerExecute]
    private void OnDocumentOpened(IRevitContext context, IMyService myService)
    {
        myService.TrackDocument(context.Document);
    }
}
```

**Example: react to a view being activated**

```csharp
public class MyViewActivatedHandler : RevitViewActivatedHandler
{
    public MyViewActivatedHandler(UIControlledApplication application)
        : base(application) { }

    [RevitEventHandlerExecute]
    private void OnViewActivated(IRevitUiContext context, IMyViewService viewService)
    {
        viewService.SetActiveView(context.ActiveView, context.Document);
    }
}
```

The attributed method must return `void`. It may be `private`, `protected`, or `public`. Only one method per type hierarchy may carry `[RevitEventHandlerExecute]` -- `InvalidOperationException` is thrown at runtime if more than one is found.

#### Optional Parameters

The framework distinguishes between required and optional parameters:

| Convention | Example | Behaviour |
|---|---|---|
| Nullable annotation | `IMyService? service` | Optional -- receives `null` if not registered |
| Default value of `null` | `IMyService service = null` | Optional -- receives `null` if not registered |
| No annotation, no default | `IMyService service` | Required -- throws if not registered |

```csharp
[RevitEventHandlerExecute]
private void OnDocumentOpened(
    IRevitContext context,          // required -- always registered
    IMyService myService,           // required -- throws if not registered
    IOptionalFeature? feature,      // optional -- null if not registered
    ILogging logging = null)        // optional -- null if not registered
{
    // ...
}
```

### The Standard Approach: Override `OnExecute`

Override `OnExecute(TEventArgs args)` when DI parameter injection is not needed:

```csharp
public class MyDocumentSavedHandler : RevitDocumentSavedHandler
{
    public MyDocumentSavedHandler(ControlledApplication application)
        : base(application) { }

    protected override void OnExecute(DocumentSavedEventArgs args)
    {
        var document = args.Document;
        // React to the document being saved.
    }
}
```

> **Note:** `OnExecute(TEventArgs)` is only called when no method marked with `[RevitEventHandlerExecute]` is found in the type hierarchy.

## Available Handler Classes

### Document Handlers -- `ControlledApplication`

These handlers are available for both `RevitApp` and `RevitDbApp`.

| Handler class | Event | Context registered in scope |
|---|---|---|
| `RevitDocumentOpeningHandler` | `DocumentOpening` | *(no document yet -- use `args.PathName`)* |
| `RevitDocumentOpenedHandler` | `DocumentOpened` | `Document` |
| `RevitDocumentCreatingHandler` | `DocumentCreating` | *(no document yet)* |
| `RevitDocumentCreatedHandler` | `DocumentCreated` | `Document` |
| `RevitDocumentClosingHandler` | `DocumentClosing` | `Document` |
| `RevitDocumentClosedHandler` | `DocumentClosed` | *(document destroyed -- use `args.Status`)* |
| `RevitDocumentSavingHandler` | `DocumentSaving` | `Document` |
| `RevitDocumentSavedHandler` | `DocumentSaved` | `Document` |
| `RevitDocumentSavingAsHandler` | `DocumentSavingAs` | `Document` |
| `RevitDocumentSavedAsHandler` | `DocumentSavedAs` | `Document` |
| `RevitDocumentChangedHandler` | `DocumentChanged` | `Document` |
| `RevitDocumentSynchronizingWithCentralHandler` | `DocumentSynchronizingWithCentral` | `Document` |
| `RevitDocumentSynchronizedWithCentralHandler` | `DocumentSynchronizedWithCentral` | `Document` |
| `RevitDocumentPrintingHandler` | `DocumentPrinting` | `Document` |
| `RevitDocumentPrintedHandler` | `DocumentPrinted` | `Document` |

### Application-Level Handlers -- `ControlledApplication`

These handlers are also available for both `RevitApp` and `RevitDbApp`.

| Handler class | Event | Context registered in scope |
|---|---|---|
| `RevitFailuresProcessingHandler` | `FailuresProcessing` | *(use `args.GetFailuresAccessor()`)* |
| `RevitFileExportingHandler` | `FileExporting` | `Document` |
| `RevitFileExportedHandler` | `FileExported` | `Document` |
| `RevitFileImportingHandler` | `FileImporting` | `Document` |
| `RevitFileImportedHandler` | `FileImported` | `Document` |
| `RevitLinkedResourceOpeningHandler` | `LinkedResourceOpening` | -- |
| `RevitLinkedResourceOpenedHandler` | `LinkedResourceOpened` | -- |
| `RevitProgressChangedHandler` | `ProgressChanged` | -- |

### UI Handlers -- `UIControlledApplication`

These handlers require `RevitApp` (`IExternalApplication`). They are not available in `RevitDbApp`.

| Handler class | Event | Context registered in scope |
|---|---|---|
| `RevitViewActivatedHandler` | `ViewActivated` | `UIApplication`, `UIDocument`, `Document`, `View` |
| `RevitIdlingHandler` | `Idling` | `UIApplication` |
| `RevitApplicationClosingHandler` | `ApplicationClosing` | -- |
| `RevitDialogBoxShowingHandler` | `DialogBoxShowing` | -- |
| `RevitDisplayingOptionsDialogHandler` | `DisplayingOptionsDialog` | -- |
| `RevitFabricationPartBrowserChangedHandler` | `FabricationPartBrowserChanged` | -- |
| `RevitFormulaEditingHandler` | `FormulaEditing` | -- |
| `RevitTransferredProjectStandardsHandler` | `TransferredProjectStandards` | -- |
| `RevitTransferringProjectStandardsHandler` | `TransferringProjectStandards` | -- |

## Handler Lifetime and Scope

A handler is active from the moment it is constructed until it is disposed. The appropriate lifetime depends on where and why the handler is needed.

### Application Lifetime

Handlers that must be active for the entire add-in session are created in startup and disposed in shutdown. This is the most common case for cross-cutting concerns such as document tracking, audit logging, or enforcing project standards.

**With `RevitApp`**

```csharp
public class MyApp : RevitApp
{
    private RevitDocumentOpenedHandler? _documentOpenedHandler;
    private RevitViewActivatedHandler?  _viewActivatedHandler;

    protected override void OnConfigure(IHostBuilder builder)
    {
        base.OnConfigure(builder);
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IMyService, MyService>();
        });
    }

    protected override bool OnStartup(UIControlledApplication application)
    {
        _documentOpenedHandler = new MyDocumentOpenedHandler(application.ControlledApplication);
        _viewActivatedHandler  = new MyViewActivatedHandler(application);
        return true;
    }

    protected override bool OnShutdown(UIControlledApplication application)
    {
        _documentOpenedHandler?.Dispose();
        _viewActivatedHandler?.Dispose();
        return true;
    }
}
```

**With `RevitDbApp`**

```csharp
public class MyDbApp : RevitDbApp
{
    private RevitDocumentOpenedHandler? _documentOpenedHandler;

    protected override bool OnStartup(ControlledApplication application)
    {
        _documentOpenedHandler = new MyDocumentOpenedHandler(application);
        return true;
    }

    protected override bool OnShutdown(ControlledApplication application)
    {
        _documentOpenedHandler?.Dispose();
        return true;
    }
}
```

**Using `[RevitStartup]` and `[RevitShutdown]`**

```csharp
public class MyApp : RevitApp
{
    private RevitDocumentSavedHandler? _savedHandler;

    protected override void OnConfigure(IHostBuilder builder)
    {
        base.OnConfigure(builder);
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IMyService, MyService>();
        });
    }

    [RevitStartup]
    private bool Initialize(UIControlledApplication application, IMyService myService)
    {
        _savedHandler = new MyDocumentSavedHandler(application.ControlledApplication);
        return true;
    }

    [RevitShutdown]
    private bool Teardown()
    {
        _savedHandler?.Dispose();
        return true;
    }
}
```

### Scoped Lifetime -- Modeless Windows and Dockable Panes

Handlers do not have to live for the entire session. A modeless window or dockable pane may need to react to Revit events only while it is open. In that case, create the handler when the window opens and dispose it when the window closes.

The handler holds a reference to the window's view model (or any other context) through a constructor parameter, so the `[RevitEventHandlerExecute]` method has everything it needs without additional DI registrations.

```csharp
// Handler scoped to a modeless window.
public sealed class SelectionSyncHandler : RevitViewActivatedHandler
{
    private readonly MyPanelViewModel _viewModel;

    public SelectionSyncHandler(UIControlledApplication application, MyPanelViewModel viewModel)
        : base(application)
    {
        _viewModel = viewModel;
    }

    [RevitEventHandlerExecute]
    private void OnViewActivated(IRevitUiContext context)
    {
        _viewModel.UpdateActiveView(context.ActiveView, context.Document);
    }
}
```

The window (or its view model) owns the handler and disposes it on close:

```csharp
public sealed class MyPanel : IDisposable
{
    private readonly SelectionSyncHandler _handler;

    public MyPanel(UIControlledApplication application)
    {
        ViewModel = new MyPanelViewModel();
        _handler  = new SelectionSyncHandler(application, ViewModel);
    }

    public MyPanelViewModel ViewModel { get; }

    public void Dispose()
    {
        _handler.Dispose();  // unsubscribes from ViewActivated
    }
}
```

> **Important:** Always dispose scoped handlers when the window closes -- not when the add-in shuts down. A handler whose owner has been closed but that is never disposed stays subscribed and continues to fire, keeping the view model and the window alive in memory.

### Choosing the Right Lifetime

| Scenario | Lifetime | Who creates | Who disposes |
|---|---|---|---|
| Document tracking, audit, standards enforcement | Application | `OnStartup` / `[RevitStartup]` | `OnShutdown` / `[RevitShutdown]` |
| Modeless window reacting to Revit events | Scoped to window | Window constructor or `Show` | Window `Close` / `Dispose` |
| Dockable pane reacting to Revit events | Scoped to pane | Pane constructor | Pane `Dispose` (or add-in shutdown) |
| Temporary batch operation | Scoped to operation | Before operation | After operation (`using`) |

## Dependency Injection (DI) Scope

By default, each event invocation creates a new child DI lifetime scope using Autofac and `Microsoft.Extensions.DependencyInjection`. The scope is disposed automatically after the handler method returns.

### Controlling Scope Creation: `UseNewScope`

The `UseNewScope` property controls whether a new child scope is created for each invocation:

| Value | Behaviour |
|-------|-----------|
| `true` *(default)* | A new child lifetime scope is created. Event args, context objects, and any services registered via `ConfigureServices` are available for injection. |
| `false` | No scope is created. Services are resolved directly from the root container. `ConfigureServices` and `RegisterEventContext` are not called, and no event context objects are registered. |

Override this property when scope creation is not desired — for example, when the handler resolves only long-lived singleton services already registered in the root container:

```csharp
public class MyDocumentOpenedHandler : RevitDocumentOpenedHandler
{
    public MyDocumentOpenedHandler(ControlledApplication application)
        : base(application) { }

    protected override bool UseNewScope => false;

    [RevitEventHandlerExecute]
    private void OnDocumentOpened(IMyRootService service)
    {
        service.Notify();
    }
}
```

The property is `virtual`, so the override may contain arbitrary logic:

```csharp
protected override bool UseNewScope => _settings.UseScopedHandlers;
```

> **Note:** When `UseNewScope` is `false`, event args, `IRevitContext`, `IRevitUiContext`, and any other objects registered via `ConfigureServices` or `RegisterEventContext` are **not** available for injection.

### What Is Registered by Default

Every invocation scope always contains:

| Type | Notes |
|------|-------|
| `TEventArgs` | The event args instance for the current invocation |
| `IRevitContext` | Always registered — provides `Application` and `Document` |
| `IRevitUiContext` | Additionally registered for UI-handler events — extends `IRevitContext` with `UiApplication`, `UiDocument`, and `ActiveView` |

The `Document` property on `IRevitContext` may be `null` when the document is not yet available (pre-open and pre-create events) or has already been destroyed (`DocumentClosed`).

### Revit Context Interfaces: `IRevitContext` and `IRevitUiContext`

Every invocation scope provides a context object that exposes the Revit state for the current event. The type depends on the event source:

| Interface | Handler base classes | Properties |
|---|---|---|
| `IRevitContext` | All `ControlledApplication`-sender handlers | `Application`, `Document` |
| `IRevitUiContext` | All `UIControlledApplication`-sender handlers | `Application`, `Document`, `UiApplication`, `UiDocument`, `ActiveView` |

`IRevitUiContext` extends `IRevitContext`, so UI handlers expose both interfaces.

The context is created once per invocation before any delegates or the `[RevitEventHandlerExecute]` method run. All delegates in the same invocation share the same instance.

The `Document` property may be `null` when the document is not yet available:

| Scenario | `Document` |
|---|---|
| Pre-open and pre-create events | `null` — document does not exist yet |
| `DocumentClosed` | `null` — document has already been destroyed |
| All other document events | Non-`null` |

```csharp
[RevitEventHandlerExecute]
private void OnDocumentOpened(IRevitContext context)
{
    Log.Info($"Opened: {context.Document.Title}");
}
```

```csharp
[RevitEventHandlerExecute]
private void OnViewActivated(IRevitUiContext context)
{
    _panelViewModel.UpdateView(context.ActiveView, context.Document);
}
```

### Registering Additional Services

Override `ConfigureServices(IServiceCollection services)` to add custom services to the per-invocation scope:

```csharp
public class MyDocumentOpenedHandler : RevitDocumentOpenedHandler
{
    public MyDocumentOpenedHandler(ControlledApplication application)
        : base(application) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IMyValidator, MyValidator>();
    }

    [RevitEventHandlerExecute]
    private void OnDocumentOpened(IRevitContext context, IMyService myService, IMyValidator validator)
    {
        if (validator.IsValid(context.Document))
        {
            myService.TrackDocument(context.Document);
        }
    }
}
```

### Registering Delegates with `AddHandler`

As an alternative to the `[RevitEventHandlerExecute]` attribute, delegates can be registered programmatically via `AddHandler`. This is useful when subscriptions need to be managed at runtime or when multiple consumers share one handler instance.

```csharp
var handle = myDocumentOpenedHandler.AddHandler(
    (IRevitContext context, IMyService service) =>
    {
        service.TrackDocument(context.Document);
    });

// Later: remove this specific subscription
handle.Dispose();
```

The strongly-typed overload accepts `Action<TContext>` and receives the context directly:

```csharp
var handle = myDocumentOpenedHandler.AddHandler(
    (IRevitContext context) => service.TrackDocument(context.Document));
```

All delegates registered on one handler instance share the same invocation scope and the same `IRevitContext` instance. A delegate may receive its own child scope by providing a `configureServices` callback:

```csharp
var handle = myDocumentOpenedHandler.AddHandler(
    (ISpecialService svc) => svc.DoWork(),
    services => services.AddTransient<ISpecialService, SpecialService>());
```

The child scope inherits all invocation scope registrations — including `IRevitContext`, `TEventArgs`, and `ConfigureServices` registrations — and adds the delegate-specific services on top.

`AddHandler` returns an `IDisposable` handle. Disposing it removes that specific registration. Disposing the handler itself removes all registrations and unsubscribes from the event.

> **Important:** When at least one delegate is registered via `AddHandler`, the `[RevitEventHandlerExecute]` method and the `OnExecute` override are **not** called. The `AddHandler` path is exclusive.

### DI Scope Creation Flow

1. The event fires and Revit calls the handler's internal callback.
2. If `UseNewScope` is `true`, a new Autofac lifetime scope is opened; otherwise the root container is used directly and steps 3–6 are skipped.
3. The event-args instance is registered in the scope.
4. `CreateContext` is called — the handler shim creates an `IRevitContext` (or `IRevitUiContext` for UI handlers) instance from the event sender and args. `IRevitContext` is always registered in the scope. For UI-specific handlers, `IRevitUiContext` is registered as well.
5. `RegisterEventContext` is called — additional Revit context objects can be registered via `IServiceCollection`.
6. `ConfigureServices` is called to register additional user-provided services.
7. If delegates are registered via `AddHandler`, they run sequentially. Each delegate resolves parameters from the invocation scope. A delegate that provides a `configureServices` callback runs in a child scope that inherits all invocation scope registrations.
8. If no `AddHandler` delegates are registered, the method marked with `[RevitEventHandlerExecute]` is discovered and invoked.
9. If no attributed method exists, `OnExecute` is called instead.
10. The invocation scope and any per-delegate child scopes are disposed.

The resulting scope tree looks like this:

```
autofacRoot
  └── invocation scope          (one per Revit event fire)
        ├── TEventArgs
        ├── IRevitContext         ← always registered
        ├── IRevitUiContext       ← additionally registered for UI-handler events
        ├── RegisterEventContext registrations
        ├── ConfigureServices registrations
        │
        ├── delegate A            ← resolves directly from invocation scope
        ├── delegate B            ← resolves directly from invocation scope
        └── child scope           ← only when delegate provides configureServices
              ├── inherits everything above
              └── delegate-specific extras
```

## Performance Considerations

The following events fire very frequently. Keep handler logic minimal and avoid allocations or long-running work inside them:

| Handler class | Reason |
|---|---|
| `RevitDocumentChangedHandler` | Fires after every committed transaction |
| `RevitProgressChangedHandler` | Fires repeatedly during long operations |
| `RevitIdlingHandler` | Fires during every Revit idle period |

For `RevitIdlingHandler`, avoid calling `IdlingEventArgs.SetRaiseWithoutDelay()` unless continuous polling is truly required, and revert to the default behavior as soon as possible.

## Multiple Handler Instances

Multiple instances of the same handler class — or of different classes subscribed to the same event — operate independently. Each instance has its own invocation scope, its own `IRevitContext`, and its own delegate list:

```
Revit fires DocumentOpened
  ├── Handler instance A  →  invocation scope A  →  IRevitContext A
  └── Handler instance B  →  invocation scope B  →  IRevitContext B
```

The order in which instances are called reflects the order Revit delivers the event to each subscriber, which is not guaranteed.

| Goal | Approach |
|---|---|
| Independent reactions from different parts of the add-in | Multiple handler instances |
| Coordinated steps within one logical reaction | Multiple delegates on one handler via `AddHandler` |

## Revit API Constraints Inside Event Handlers

The Revit API enforces restrictions on what operations can be performed inside an event callback. Violating these constraints results in an `Autodesk.Revit.Exceptions.InvalidOperationException` at the API boundary.

| Action | Allowed |
|---|---|
| Read document data | ✅ |
| Modify elements (in a transaction, in writable events) | ✅ |
| Cancel the event (in pre-events, where supported) | ✅ |
| Queue work via `IExternalEventHandler` | ✅ |
| Close, open, or create documents | ❌ |
| Start a modal dialog synchronously | ❌ |
| Call back into the event source recursively | ❌ |

These restrictions are imposed by Revit regardless of the handler framework used. An `IRevitContext` obtained during a `DocumentOpened` callback, for example, holds a valid `Document` reference — but that document cannot be closed from within the same callback.

## Implementing a Custom Handler Base

To share common behaviour across multiple handlers, derive from the appropriate typed shim (`RevitApp*EventHandler` or `RevitUi*EventHandler`) rather than from the raw base class. The shims handle context creation and GUID extraction automatically.

```csharp
// Abstract base that adds shared validation logic for any pre-document event.
public abstract class ValidatedPreDocumentHandler<TEventArgs>
    : RevitAppPreDocumentEventHandler<TEventArgs>
    where TEventArgs : RevitAPIPreDocEventArgs
{
    protected ValidatedPreDocumentHandler(ControlledApplication application)
        : base(application) { }   // no Subscribe() here — class is abstract

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IDocumentValidator, DefaultDocumentValidator>();
    }
}

// Concrete handler — implements Subscribe/Unsubscribe and the execute method.
public sealed class MyDocumentSavingHandler : ValidatedPreDocumentHandler<DocumentSavingEventArgs>
{
    public MyDocumentSavingHandler(ControlledApplication application)
        : base(application)
    {
        Subscribe();   // called in the most-derived class, after all fields are set
    }

    protected sealed override void Subscribe()   => Application.DocumentSaving += HandleEvent;
    protected sealed override void Unsubscribe() => Application.DocumentSaving -= HandleEvent;

    [RevitEventHandlerExecute]
    private void OnDocumentSaving(IRevitContext context, IDocumentValidator validator)
    {
        if (!validator.IsValid(context.Document))
        {
            Cancel();
        }
    }
}
```

The `Application` property is provided by the shim base class and holds the `ControlledApplication` (or `UIControlledApplication` for UI handlers) passed to the constructor. No `_application` field is needed in derived classes.

> **Note:** `Subscribe()` must be called from the most-derived non-abstract class. Abstract intermediates must not call it in their constructors, because virtual methods may reference fields that are not yet initialized in a derived class.

## Disposal and Cleanup

All handler classes implement `IDisposable`. Disposing a handler unsubscribes it from the Revit event and prevents memory leaks from dangling event subscriptions. Dispose each handler as soon as its owner is done with it -- not necessarily at add-in shutdown.

```csharp
protected override bool OnShutdown(UIControlledApplication application)
{
    _myHandler?.Dispose();
    return true;
}
```

## Summary

| Feature | How to use |
|---|---|
| Custom handler logic (DI) | Mark a `void` method with `[RevitEventHandlerExecute]` |
| Standard handler logic | Override `OnExecute(TEventArgs args)` |
| Register Revit context objects | Override `RegisterEventContext(IServiceCollection services, ...)` |
| Register additional services | Override `ConfigureServices(IServiceCollection services)` |
| Disable per-invocation scope | Override `UseNewScope` and return `false` |
| Document events (`RevitApp` and `RevitDbApp`) | Derive from a `Revit*Handler` class that takes `ControlledApplication` |
| UI events (`RevitApp` only) | Derive from a `Revit*Handler` class that takes `UIControlledApplication` |
| Custom event not in the pre-built set | Derive from the appropriate typed shim (e.g. `RevitAppPostDocumentEventHandler<TEventArgs>`) |
| Subscribe to the event | Automatic -- handled in the constructor |
| Unsubscribe from the event | Dispose the handler instance when its owner is done |
| Register a delegate at runtime | Call `AddHandler(delegate)` on the handler instance |
| Access the current Revit context | Inject `IRevitContext` (document handlers) or `IRevitUiContext` (UI handlers) |
| Multiple independent reactions to one event | Create multiple handler instances |