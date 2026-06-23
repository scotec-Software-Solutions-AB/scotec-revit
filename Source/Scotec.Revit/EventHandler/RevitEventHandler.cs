// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.DB.Events;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scotec.Revit.EventHandler;

/// <summary>
///     Abstract generic base class for handling a Revit application event with a per-invocation DI lifetime scope.
/// </summary>
/// <typeparam name="TSender">
///     The typed Revit application sender for the event — either
///     <c>Autodesk.Revit.ApplicationServices.Application</c> (for <c>ControlledApplication</c>-source events) or
///     <c>Autodesk.Revit.UI.UIApplication</c> (for <c>UIControlledApplication</c>-source events).
/// </typeparam>
/// <typeparam name="TEventArgs">The Revit event-args type for the event being handled.</typeparam>
/// <typeparam name="TContext">
///     The context interface registered in the per-invocation DI scope.
///     Must be <see cref="IRevitContext" /> or a type derived from it (such as <see cref="IRevitUiContext" />).
///     The context is created by <see cref="CreateContext" /> before any handler logic runs and is shared by
///     all delegates and the execute method within the same invocation.
/// </typeparam>
/// <remarks>
///     <para>
///         Derived classes subscribe to a specific Revit event by implementing <see cref="Subscribe" /> and
///         <see cref="Unsubscribe" />, wiring <see cref="HandleEvent" /> as the event callback.
///         When the event fires, the framework:
///         <list type="number">
///             <item>Creates a child Autofac lifetime scope (unless <see cref="UseNewScope" /> is <c>false</c>).</item>
///             <item>Calls <see cref="CreateContext" /> to produce a <typeparamref name="TContext" /> and registers it
///                   as <see cref="IRevitContext" /> in the scope. For UI-specific events, the context is additionally
///                   registered as <see cref="IRevitUiContext" />.</item>
///             <item>Calls <see cref="RegisterEventContext" /> to register any additional event-specific
///                   objects that cannot be obtained through <typeparamref name="TContext" />.</item>
///             <item>Calls <see cref="ConfigureServices" /> for user-provided service registrations.</item>
///             <item>Invokes registered <see cref="AddHandler(Delegate,Action{IServiceCollection}?)">delegates</see>,
///                   or falls back to a method marked with <see cref="RevitEventHandlerExecuteAttribute" />,
///                   or finally to <see cref="OnExecute" />.</item>
///         </list>
///     </para>
///     <para>
///         Use <see cref="AddHandler(Delegate,Action{IServiceCollection}?)">AddHandler</see> to register delegates
///         at runtime. All delegates on the same handler instance share the same invocation scope and the same
///         <typeparamref name="TContext" /> instance. When <see cref="AddHandler(Delegate,Action{IServiceCollection}?)">AddHandler</see>
///         registrations are present, the <see cref="RevitEventHandlerExecuteAttribute" /> method and
///         <see cref="OnExecute" /> are not called.
///     </para>
///     <para>
///         The handler self-unsubscribes in <see cref="Dispose()" />. Always dispose handler instances
///         when their owner is done — for application-lifetime handlers this is typically during add-in shutdown.
///     </para>
/// </remarks>
public abstract class RevitEventHandler<TSender, TEventArgs, TContext> : IDisposable
    where TSender : class
    where TEventArgs : RevitAPIEventArgs
    where TContext : class, IRevitContext
{
    private bool _disposed;

    // Delegate registration infrastructure -------------------------------------------------------

    /// <summary>
    ///     Stores a delegate together with its optional scope-configuration callback.
    /// </summary>
    private sealed record DelegateRegistration(
        Delegate Action,
        Action<IServiceCollection>? ConfigureServices);

    /// <summary>
    ///     Returned from <see cref="AddHandler(Delegate, Action{IServiceCollection}?)" /> and
    ///     <see cref="AddHandler{TContext}(Action{TContext})" />.
    ///     Disposing the handle removes the corresponding registration.
    /// </summary>
    private sealed class DelegateHandle : IDisposable
    {
        private readonly List<DelegateRegistration> _list;
        private readonly DelegateRegistration _registration;

        internal DelegateHandle(List<DelegateRegistration> list, DelegateRegistration registration)
        {
            _list = list;
            _registration = registration;
        }

        public void Dispose() => _list.Remove(_registration);
    }

    private readonly List<DelegateRegistration> _delegateRegistrations = [];

    // --------------------------------------------------------------------------------------------

    /// <summary>
    ///     Initializes a new instance of <see cref="RevitEventHandler{TSender, TEventArgs, TContext}" />.
    /// </summary>
    /// <param name="addInId">
    ///     The add-in GUID used to look up the root Autofac container via
    ///     <see cref="RevitAppBase.GetServiceProvider(System.Guid)" />.
    ///     Pass <c>application.ActiveAddInId.GetGUID()</c> from the concrete constructor.
    /// </param>
    /// <remarks>
    ///     Derived classes must call <see cref="Subscribe" /> at the end of their own constructor,
    ///     after all fields are initialized, to avoid virtual-member-call issues during construction.
    /// </remarks>
    protected RevitEventHandler(Guid addInId)
    {
        AddInId = addInId;
    }

    /// <summary>
    ///     Gets the add-in GUID used to resolve the root DI container.
    /// </summary>
    protected Guid AddInId { get; }

    /// <summary>
    ///     Registers a delegate to be called when the Revit event fires, with all parameters resolved
    ///     from the per-invocation DI scope. The delegate runs <em>instead of</em> any method marked
    ///     with <see cref="RevitEventHandlerExecuteAttribute" /> or the <see cref="OnExecute" /> override
    ///     as long as at least one registration is present.
    /// </summary>
    /// <param name="action">
    ///     A delegate with any number and types of parameters. All parameters are resolved from the
    ///     per-invocation DI scope. The scope automatically contains the current
    ///     <typeparamref name="TEventArgs" /> instance as well as any objects registered via
    ///     <see cref="RegisterEventContext" /> and <see cref="ConfigureServices" />.
    /// </param>
    /// <param name="configureServices">
    ///     An optional callback to register additional services for this specific invocation.
    ///     A child scope is created from the invocation scope when this parameter is non-<c>null</c>.
    /// </param>
    /// <returns>
    ///     An <see cref="IDisposable" /> handle. Disposing the handle removes this specific registration.
    ///     Disposing the handler itself clears all registrations.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action" /> is <c>null</c>.</exception>
    [UsedImplicitly]
    public IDisposable AddHandler(Delegate action, Action<IServiceCollection>? configureServices = null)
    {
        ArgumentNullException.ThrowIfNull(action);

        var registration = new DelegateRegistration(action, configureServices);
        _delegateRegistrations.Add(registration);
        return new DelegateHandle(_delegateRegistrations, registration);
    }

    /// <summary>
    ///     Registers a strongly-typed delegate to be called when the Revit event fires.
    ///     The <typeparamref name="TContext" /> parameter is the context type declared on the handler class
    ///     (either <see cref="IRevitContext" /> for <c>Application</c>-sender handlers or
    ///     <see cref="IRevitUiContext" /> for <c>UIApplication</c>-sender handlers) and is resolved from
    ///     the per-invocation DI scope. The context object is created automatically by
    ///     <see cref="CreateContext" /> before the delegate is invoked.
    /// </summary>
    /// <param name="action">The strongly-typed delegate to register.</param>
    /// <returns>
    ///     An <see cref="IDisposable" /> handle. Disposing the handle removes this specific registration.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action" /> is <c>null</c>.</exception>
    [UsedImplicitly]
    public IDisposable AddHandler(Action<TContext> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var registration = new DelegateRegistration(action, ConfigureServices: null);
        _delegateRegistrations.Add(registration);
        return new DelegateHandle(_delegateRegistrations, registration);
    }

    /// <summary>
    ///     Gets the event args for the currently executing invocation, or <c>null</c> when no invocation is active.
    /// </summary>
    protected TEventArgs? EventArgs { get; private set; }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Gets a value indicating whether the current event supports cancellation.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when accessed outside of an active event invocation.
    /// </exception>
    public bool IsCancellable => EventArgs?.Cancellable ?? throw new InvalidOperationException("EventArgs not set. This property can only be accessed during event handling.");

    /// <summary>
    ///     Gets a value indicating whether the current event has already been cancelled.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when accessed outside of an active event invocation.
    /// </exception>
    public bool IsCancelled => EventArgs?.IsCancelled() ?? throw new InvalidOperationException("EventArgs not set. This property can only be accessed during event handling.");

    /// <summary>
    ///     Subscribes to the specific Revit event.
    ///     Wire <see cref="HandleEvent" /> to the event here.
    ///     Must be called explicitly at the end of the derived class constructor.
    /// </summary>
    protected abstract void Subscribe();

    /// <summary>
    ///     Unsubscribes from the specific Revit event. Called from <see cref="Dispose(bool)" />.
    ///     Remove <see cref="HandleEvent" /> from the event here.
    /// </summary>
    protected abstract void Unsubscribe();

    /// <summary>
    ///     Allows derived classes to register additional services into the per-invocation DI scope.
    ///     Called once per event invocation when <see cref="UseNewScope" /> is <c>true</c>.
    /// </summary>
    /// <param name="services">
    ///     The <see cref="IServiceCollection" /> for the current invocation scope.
    ///     Registrations made here are available to all delegates and the execute method
    ///     within the same invocation.
    /// </param>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Derived classes can override to add services.
    }

    /// <summary>
    ///     Gets a value indicating whether a new DI lifetime scope should be created for each event invocation.
    /// </summary>
    /// <value>
    ///     <c>true</c> to create a new child lifetime scope for each invocation (default);
    ///     <c>false</c> to resolve services directly from the root container without creating a scope.
    ///     When set to <c>false</c>, no additional objects are registered in the DI container for the invocation.
    /// </value>
    /// <remarks>
    ///     Override this property in derived classes and return <c>false</c> when scope creation is not desired,
    ///     for example to avoid registering short-lived instances that conflict with singleton registrations.
    ///     The property is virtual so the override may contain arbitrary logic.
    /// </remarks>
    protected virtual bool UseNewScope { get; } = true;

    /// <summary>
    ///     Called when the event fires and no delegates have been registered via
    ///     <see cref="AddHandler(Delegate, Action{IServiceCollection}?)" /> and no method marked with
    ///     <see cref="RevitEventHandlerExecuteAttribute" /> is found in the type hierarchy.
    ///     Override this method for a simple, non-DI handler implementation.
    /// </summary>
    /// <param name="sender">The typed event sender for the current invocation.</param>
    /// <param name="args">The event args for the current invocation.</param>
    protected virtual void OnExecute(TSender? sender, TEventArgs args)
    {
    }

    /// <summary>
    ///     Registers additional event-specific objects into the per-invocation DI scope.
    ///     Called once per invocation after <see cref="CreateContext" /> when <see cref="UseNewScope" /> is <c>true</c>.
    ///     Override only when objects that are not reachable through <see cref="IRevitContext" /> or
    ///     <see cref="IRevitUiContext" /> need to be available for injection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> for the current invocation scope.</param>
    /// <param name="sender">The typed event sender.</param>
    /// <param name="args">The event args instance.</param>
    protected virtual void RegisterEventContext(IServiceCollection services, TSender? sender, TEventArgs args)
    {
    }

    /// <summary>
    ///     Creates the <typeparamref name="TContext" /> instance for the current event invocation.
    ///     The returned context is registered in the per-invocation DI scope as
    ///     <typeparamref name="TContext" /> and, when <typeparamref name="TContext" /> is a more-derived
    ///     interface (such as <see cref="IRevitUiContext" />), also as <see cref="IRevitContext" />,
    ///     so that both keys are resolvable from the scope.
    ///     The context is created once per invocation and shared by all delegates and the execute method.
    /// </summary>
    /// <param name="sender">The typed event sender.</param>
    /// <param name="args">The event args instance.</param>
    /// <returns>
    ///     A <typeparamref name="TContext" /> instance, or <c>null</c> when no context is available
    ///     for the current invocation — for example, before a document has been opened or after it
    ///     has been destroyed (such as in <c>DocumentClosed</c>).
    /// </returns>
    protected abstract TContext? CreateContext(TSender? sender, TEventArgs args);

    /// <summary>
    ///     The Revit event callback. Opens a DI lifetime scope (unless <see cref="UseNewScope" /> is <c>false</c>),
    ///     registers context, and dispatches to the execute entry point.
    ///     Wire this method to the Revit event in <see cref="Subscribe" />.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="args">The event args.</param>
    protected void HandleEvent(object? sender, TEventArgs args)
    {
        EventArgs = args;
        var typedSender = sender as TSender;

        ILifetimeScope? scope = null;
        IServiceProvider serviceProvider;

        var autofacRoot = RevitAppBase.GetServiceProvider().GetAutofacRoot();

        if (UseNewScope)
        {
            var context = CreateContext(typedSender, args);
            scope = autofacRoot.BeginLifetimeScope(builder =>
            {
                IServiceCollection services = new ServiceCollection();
                services.AddScoped<TEventArgs>(_ => args);
                RegisterEventContext(services, typedSender, args);
                ConfigureServices(services);
                // IRevitContext is always registered.
                // IRevitUiContext is additionally registered when the context is a UI context.
                builder.RegisterInstance(context!).As<IRevitContext>().OwnedByLifetimeScope();
                if (context is IRevitUiContext uiContext)
                {
                    // Same instance. Use ExternallyOwned here to avoid multiple calls to Dispose.
                    builder.RegisterInstance(uiContext).As<IRevitUiContext>().ExternallyOwned();
                }
                builder.Populate(services);
            });
            serviceProvider = scope.Resolve<IServiceProvider>();
        }
        else
        {
            serviceProvider = autofacRoot.Resolve<IServiceProvider>();
        }

        try
        {
            InvokeOnExecute(typedSender, args, serviceProvider);
        }
        finally
        {
            scope?.Dispose();
            EventArgs = null;
        }
    }

    /// <summary>
    ///     Releases resources and unsubscribes from the event.
    /// </summary>
    /// <param name="disposing"><c>true</c> when called from <see cref="Dispose()" />.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _delegateRegistrations.Clear();
            Unsubscribe();
        }
    }

    private void InvokeOnExecute(TSender? sender, TEventArgs args, IServiceProvider serviceProvider)
    {
        // Delegate-subscription path: runs instead of the attribute/OnExecute path when registrations exist.
        if (_delegateRegistrations.Count > 0)
        {
            foreach (var registration in _delegateRegistrations.ToList())
            {
                IServiceProvider resolvedProvider;
                ILifetimeScope? childScope = null;

                if (registration.ConfigureServices is not null)
                {
                    // Create a child scope from the current invocation scope so that all per-invocation
                    // registrations (TContext, TEventArgs, etc.) are inherited by the child scope.
                    var parentScope = serviceProvider.GetRequiredService<ILifetimeScope>();
                    childScope = parentScope.BeginLifetimeScope(builder =>
                    {
                        var extra = new ServiceCollection();
                        registration.ConfigureServices(extra);
                        builder.Populate(extra);
                    });
                    resolvedProvider = childScope.Resolve<IServiceProvider>();
                }
                else
                {
                    resolvedProvider = serviceProvider;
                }

                try
                {
                    var delegateArgs = RevitReflectionHelper.ResolveParameters(
                        registration.Action.Method, resolvedProvider);
                    registration.Action.DynamicInvoke(delegateArgs);
                }
                finally
                {
                    childScope?.Dispose();
                }
            }

            return;
        }

        // Legacy attribute/OnExecute path.
        var method = RevitReflectionHelper.FindSingleAttributedMethod<RevitEventHandlerExecuteAttribute>(
            GetType(), typeof(RevitEventHandler<TSender, TEventArgs, TContext>), typeof(void));

        if (method is not null)
        {
            RevitReflectionHelper.Invoke(this, method, serviceProvider,
                new Dictionary<Type, object>
                {
                    [typeof(TEventArgs)] = args,
                    [typeof(IServiceProvider)] = serviceProvider
                });

            return;
        }

        OnExecute(sender, args);
    }
}

