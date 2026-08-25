// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Scotec.Revit.EventHandler;

/// <summary>
///     Base class for pre-event handlers with a strongly-typed sender, event args, and context.
///     Extends <see cref="RevitEventHandler{TSender,TEventArgs,TContext}" /> with support for cancelling the Revit event.
/// </summary>
/// <typeparam name="TSender">The type of the event sender.</typeparam>
/// <typeparam name="TEventArgs">The Revit pre-event-args type.</typeparam>
/// <typeparam name="TContext">The Revit context type produced for each invocation.</typeparam>
public abstract class RevitPreEventHandler<TSender, TEventArgs, TContext> : RevitEventHandler<TSender, TEventArgs, TContext>
    where TSender : class
    where TEventArgs : RevitAPIPreEventArgs
    where TContext : class, IRevitContext
{
    /// <summary>
    ///     Initializes a new instance of <see cref="RevitPreEventHandler{TSender, TEventArgs, TContext}" />.
    /// </summary>
    /// <param name="addInId">The add-in GUID used to resolve the root DI container.</param>
    protected RevitPreEventHandler(Guid addInId) : base(addInId)
    {
    }

    /// <summary>
    ///     Registers <see cref="RevitEventCancellation" /> into the per-invocation DI scope so that delegates
    ///     registered via <see cref="RevitEventHandler{TSender,TEventArgs,TContext}.AddHandler(System.Delegate,System.Action{Microsoft.Extensions.DependencyInjection.IServiceCollection})" /> can cancel the event
    ///     by resolving and calling <see cref="RevitEventCancellation.Cancel" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> for the current invocation scope.</param>
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddScoped<RevitEventCancellation>(_ => new RevitEventCancellation(Cancel));
    }

    /// <summary>
    ///     Cancels the current Revit pre-event. Must only be called from within an active event-handler invocation.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when called outside of an active event-handler invocation.
    /// </exception>
    public void Cancel()
    {
        if (EventArgs is null)
        {
            throw new InvalidOperationException("EventArgs not set. This method can only be called during event handling.");
        }

        EventArgs.Cancel();
    }
}

/// <summary>
///     Convenience base class for pre-event handlers whose sender is
///     <see cref="Application" /> and whose context is <see cref="IRevitContext" />.
/// </summary>
/// <typeparam name="TEventArgs">The Revit event-args type.</typeparam>
public abstract class RevitAppPreEventHandler<TEventArgs>
    : RevitPreEventHandler<Application, TEventArgs, IRevitContext>
    where TEventArgs : RevitAPIPreEventArgs
{
    /// <summary>
    ///     Initializes a new instance of <see cref="RevitAppPreEventHandler{TEventArgs}" />.
    /// </summary>
    /// <param name="application">The Revit controlled application to register the event on.</param>
    protected RevitAppPreEventHandler(ControlledApplication application) : base(application.ActiveAddInId.GetGUID())
    {
        Application = application;
    }

    /// <summary>Gets the controlled application this handler is registered on.</summary>
    protected ControlledApplication Application { get; }

    /// <inheritdoc />
    protected override IRevitContext? CreateContext(Application? sender, TEventArgs args)
        => sender is not null ? new RevitContext(sender) : null;
}

/// <summary>
///     Convenience base class for pre-event handlers whose sender is
///     <see cref="UIApplication" /> and whose context is <see cref="IRevitUiContext" />.
/// </summary>
/// <typeparam name="TEventArgs">The Revit event-args type.</typeparam>
public abstract class RevitUiPreEventHandler<TEventArgs>
    : RevitPreEventHandler<UIApplication, TEventArgs, IRevitUiContext>
    where TEventArgs : RevitAPIPreEventArgs
{
    /// <summary>
    ///     Initializes a new instance of <see cref="RevitUiPreEventHandler{TEventArgs}" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application to register the event on.</param>
    protected RevitUiPreEventHandler(UIControlledApplication application) : base(application.ActiveAddInId.GetGUID())
    {
        Application = application;
    }

    /// <summary>Gets the UI controlled application this handler is registered on.</summary>
    protected UIControlledApplication Application { get; }

    /// <inheritdoc />
    protected override IRevitUiContext? CreateContext(UIApplication? sender, TEventArgs args)
        => sender is not null ? new RevitUiContext(sender) : null;
}
