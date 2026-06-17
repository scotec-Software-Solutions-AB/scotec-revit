// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Scotec.Revit.EventHandler;

public abstract class RevitPreEventHandler<TSender, TEventArgs, TContext> : RevitEventHandler<TSender, TEventArgs, TContext>
    where TSender : class
    where TEventArgs : RevitAPIPreEventArgs
    where TContext : class, IRevitContext
{
    protected RevitPreEventHandler(Guid addInId) : base(addInId)
    {
    }

    /// <summary>
    ///     Registers <see cref="RevitEventCancellation" /> into the per-invocation DI scope so that delegates
    ///     registered via <see cref="RevitEventHandler{TSender,TEventArgs}.AddHandler" /> can cancel the event
    ///     by resolving and calling <see cref="RevitEventCancellation.Cancel" />.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> for the current invocation scope.</param>
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.AddScoped<RevitEventCancellation>(_ => new RevitEventCancellation(Cancel));
    }

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
    protected RevitAppPreEventHandler(ControlledApplication application) : base(application.ActiveAddInId.GetGUID())
    {
        Application = application;
    }

    /// <summary>Gets the controlled application this handler is registered on.</summary>
    protected ControlledApplication Application { get; }

    /// <inheritdoc />
    /// <remarks>Returns <c>null</c> — a bare <see cref="Application" /> sender carries no context at this level.</remarks>
    protected override IRevitContext? CreateContext(Application? sender, TEventArgs args) => null;
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
    protected RevitUiPreEventHandler(UIControlledApplication application) : base(application.ActiveAddInId.GetGUID())
    {
        Application = application;
    }

    /// <summary>Gets the UI controlled application this handler is registered on.</summary>
    protected UIControlledApplication Application { get; }

    /// <inheritdoc />
    protected override IRevitUiContext? CreateContext(UIApplication? sender, TEventArgs args)
        => sender?.ActiveUIDocument is not null ? new RevitUiContext(sender) : null;
}
