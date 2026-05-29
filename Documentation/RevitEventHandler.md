# RevitEventHandler

`RevitEventHandler<TEventArgs>` is the generic base class for handling Revit application events with automatic dependency injection (DI) scope per invocation. Each concrete handler wraps one specific Revit event, self-subscribes in the constructor, and self-unsubscribes on disposal.

Each time the subscribed event fires, a new DI scope is created by default. Revit context objects available from the event -- such as `Document`, `View`, `UIApplication`, and `UIDocument` -- are registered automatically. Services from the application's DI container are resolved and injected into the handler method. Scope creation can be disabled per handler via `UseNewScope`.

## Overview

The framework provides two sets of pre-built concrete handler base classes:

| Set | Source application | Events |
|-----|--------------------|--------|
| Document handlers | `ControlledApplication` | Open, create, close, save, sync, print, export, import, change |
| UI handlers | `UIControlledApplication` | View activation, idling, dialogs, application closing |

Document handlers are available for both `RevitApp` (`IExternalApplication`) and `RevitDbApp` (`IExternalDBApplication`). UI handlers require `RevitApp`.

## Implementing a Handler

### The Recommended Approach: `[RevitEventHandlerExecute]` Attribute

The preferred way to implement handler logic is to declare a method with the `[RevitEventHandlerExecute]` attribute. The method must return `void`. It may have any name and any combination of DI-resolvable parameters. The framework discovers the method at runtime, resolves all parameters from the per-invocation DI scope, and calls it.

Event-args and known Revit context objects (for example `Document`, `View`) are passed through directly without going through DI registration.

**Example: react to a document being opened**

```csharp
public class MyDocumentOpenedHandler : RevitDocumentOpenedHandler
{
    public MyDocumentOpenedHandler(ControlledApplication application)
        : base(application) { }

    [RevitEventHandlerExecute]
    private void OnDocumentOpened(Document document, IMyService myService)
    {
        myService.TrackDocument(document);
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
    private void OnViewActivated(View view, Document document, IMyViewService viewService)
    {
        viewService.SetActiveView(view, document);
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
    Document document,              // passed through directly
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
    private void OnViewActivated(View view, Document document)
    {
        _viewModel.UpdateActiveView(view, document);
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

> **Note:** When `UseNewScope` is `false`, event args, `Document`, `View`, and other context objects listed in _What Is Registered by Default_ are **not** available for injection.

### What Is Registered by Default

The event-args type and the Revit context objects specific to each event are registered automatically. See the tables above for what each handler registers. In addition:

| Type | Registered when |
|------|-----------------|
| `TEventArgs` | Always |
| `Document` | Available from the event args or sender |
| `View` | Available from the event args (`RevitViewActivatedHandler`) |
| `UIApplication` | Available from the sender (`RevitViewActivatedHandler`, `RevitIdlingHandler`) |
| `UIDocument` | Available from the sender's active document (`RevitViewActivatedHandler`) |

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
    private void OnDocumentOpened(Document document, IMyService myService, IMyValidator validator)
    {
        if (validator.IsValid(document))
        {
            myService.TrackDocument(document);
        }
    }
}
```

### DI Scope Creation Flow

1. The event fires and Revit calls the handler's internal callback.
2. If `UseNewScope` is `true`, a new Autofac lifetime scope is opened; otherwise the root container is used directly and steps 3–5 are skipped.
3. The event-args instance is registered in the scope.
4. `RegisterEventContext` is called — the concrete handler registers known Revit context objects.
5. `ConfigureServices` is called to register additional user-provided services.
6. The method marked with `[RevitEventHandlerExecute]` is discovered and invoked with parameters resolved from the scope or root container.
7. If no attributed method exists, `OnExecute(TEventArgs)` is called instead.
8. If a scope was created, it is disposed.

## Performance Considerations

The following events fire very frequently. Keep handler logic minimal and avoid allocations or long-running work inside them:

| Handler class | Reason |
|---|---|
| `RevitDocumentChangedHandler` | Fires after every committed transaction |
| `RevitProgressChangedHandler` | Fires repeatedly during long operations |
| `RevitIdlingHandler` | Fires during every Revit idle period |

For `RevitIdlingHandler`, avoid calling `IdlingEventArgs.SetRaiseWithoutDelay()` unless continuous polling is truly required, and revert to the default behavior as soon as possible.

## Implementing a Custom Handler Base

For events not covered by the pre-built handlers, or to share common behaviour across multiple handlers, derive directly from `RevitEventHandler<TEventArgs>`:

```csharp
public abstract class MyCustomEventHandler : RevitEventHandler<DocumentClosingEventArgs>
{
    private readonly ControlledApplication _application;

    protected MyCustomEventHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    protected override void Subscribe()
    {
        _application.DocumentClosing += HandleEvent;
    }

    protected override void Unsubscribe()
    {
        _application.DocumentClosing -= HandleEvent;
    }

    protected override void RegisterEventContext(ContainerBuilder builder, object sender, DocumentClosingEventArgs args)
    {
        if (args.Document is not null)
        {
            builder.RegisterInstance(args.Document).ExternallyOwned();
        }
    }
}
```

Pass `application.ActiveAddInId.GetGUID()` to the base constructor -- this is used to look up the root Autofac container for the current add-in.

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
| Register additional services | Override `ConfigureServices(IServiceCollection services)` |
| Disable per-invocation scope | Override `UseNewScope` and return `false` |
| Document events (`RevitApp` and `RevitDbApp`) | Derive from a `Revit*Handler` class that takes `ControlledApplication` |
| UI events (`RevitApp` only) | Derive from a `Revit*Handler` class that takes `UIControlledApplication` |
| Custom event not in the pre-built set | Derive directly from `RevitEventHandler<TEventArgs>` |
| Subscribe to the event | Automatic -- handled in the constructor |
| Unsubscribe from the event | Dispose the handler instance when its owner is done |