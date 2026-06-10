// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit;

/// <summary>
///     Marks a method as the execute entry point for a <see cref="RevitEventHandler{TEventArgs}" />.
/// </summary>
/// <remarks>
///     Apply this attribute to a single method in a derived class. The framework discovers and invokes
///     it with parameters resolved from the per-invocation DI scope. If no method is marked, the framework
///     falls back to <see cref="RevitEventHandler{TEventArgs}.OnExecute" />.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class RevitEventHandlerExecuteAttribute : Attribute;

/// <summary>
///     Abstract generic base class for handling a Revit application event with a DI lifetime scope per invocation.
/// </summary>
/// <typeparam name="TEventArgs">The Revit event-args type for the event being handled.</typeparam>
/// <remarks>
///     <para>
///         Derived classes subscribe to a specific Revit event by overriding <see cref="Subscribe" /> and
///         <see cref="Unsubscribe" />, calling <see cref="HandleEvent" /> as the event callback. The framework
///         opens a new Autofac lifetime scope for each event invocation, registers the <typeparamref name="TEventArgs" />
///         instance, and calls <see cref="RegisterEventContext" /> so that concrete handlers can additionally
///         register well-known types such as <c>Document</c>, <c>View</c>, <c>UIApplication</c>, or
///         <c>UIDocument</c>.
///     </para>
///     <para>
///         Override <see cref="ConfigureServices" /> to register further services into the scope.
///         Declare a single method marked with <see cref="RevitEventHandlerExecuteAttribute" /> to receive
///         DI-resolved parameters, or override <see cref="OnExecute" /> for a simple non-DI fallback.
///     </para>
///     <para>
///         The handler self-subscribes in the constructor and self-unsubscribes in <see cref="Dispose()" />.
///         Always dispose handler instances during application shutdown.
///     </para>
/// </remarks>
public abstract class RevitEventHandler<TEventArgs> : IDisposable
    where TEventArgs : EventArgs
{
    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of <see cref="RevitEventHandler{TEventArgs}" /> and subscribes to the event.
    /// </summary>
    /// <param name="addInId">
    ///     The add-in GUID used to look up the root Autofac container via
    ///     <see cref="RevitAppBase.GetServiceProvider(System.Guid)" />.
    /// </param>
    protected RevitEventHandler(Guid addInId)
    {
        AddInId = addInId;
        Subscribe();
    }

    /// <summary>
    ///     Gets the add-in GUID used to resolve the root DI container.
    /// </summary>
    protected Guid AddInId { get; }

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
    ///     Subscribes to the specific Revit event. Called once from the constructor.
    ///     Wire <see cref="HandleEvent" /> to the event here.
    /// </summary>
    protected abstract void Subscribe();

    /// <summary>
    ///     Unsubscribes from the specific Revit event. Called from <see cref="Dispose(bool)" />.
    ///     Remove <see cref="HandleEvent" /> from the event here.
    /// </summary>
    protected abstract void Unsubscribe();

    /// <summary>
    ///     Allows derived classes to add additional services into the per-invocation DI scope.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> for the current invocation scope.</param>
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
    ///     Called when the event fires and no method marked with <see cref="RevitEventHandlerExecuteAttribute" /> is
    ///     found in the type hierarchy. Override this method for a simple, non-DI handler implementation.
    /// </summary>
    /// <param name="args">The event args for the current invocation.</param>
    protected virtual void OnExecute(TEventArgs args)
    {
    }

    /// <summary>
    ///     Registers well-known Revit context objects (such as <c>Document</c>, <c>View</c>, <c>UIApplication</c>)
    ///     into the per-invocation scope. Override in concrete handlers to expose the types that are available
    ///     from the specific event args or sender.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> for the current invocation scope.</param>
    /// <param name="sender">The event sender object.</param>
    /// <param name="args">The event args instance.</param>
    protected virtual void RegisterEventContext(IServiceCollection services, object sender, TEventArgs args)
    {
    }

    /// <summary>
    ///     The Revit event callback. Opens a DI lifetime scope (unless <see cref="UseNewScope" /> is <c>false</c>),
    ///     registers context, and dispatches to the execute entry point.
    ///     Wire this method to the Revit event in <see cref="Subscribe" />.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="args">The event args.</param>
    protected void HandleEvent(object sender, TEventArgs args)
    {
        ILifetimeScope? scope = null;
        IServiceProvider serviceProvider;

        var autofacRoot = RevitAppBase.GetServiceProvider().GetAutofacRoot();

        if (UseNewScope)
        {
            scope = autofacRoot.BeginLifetimeScope(builder =>
            {
                var services = new ServiceCollection();
                services.AddSingleton(typeof(TEventArgs), args);
                RegisterEventContext(services, sender, args);
                ConfigureServices(services);
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
            InvokeOnExecute(args, serviceProvider);
        }
        finally
        {
            scope?.Dispose();
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
            Unsubscribe();
        }
    }

    private void InvokeOnExecute(TEventArgs args, IServiceProvider serviceProvider)
    {
        var method = RevitReflectionHelper.FindSingleAttributedMethod<RevitEventHandlerExecuteAttribute>(
            GetType(), typeof(RevitEventHandler<TEventArgs>), typeof(void));

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

        OnExecute(args);
    }
}
