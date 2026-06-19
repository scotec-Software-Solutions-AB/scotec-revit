// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;

namespace Scotec.Revit.EventHandler;

public abstract class RevitPostEventHandler<TSender, TEventArgs, TContext> : RevitEventHandler<TSender, TEventArgs, TContext>
    where TSender : class
    where TEventArgs : RevitAPIPostEventArgs
    where TContext : class, IRevitContext
{
    protected RevitPostEventHandler(Guid addInId) : base(addInId)
    {
    }
}

/// <summary>
///     Convenience base class for post-event handlers whose sender is
///     <see cref="Application" /> and whose context is <see cref="IRevitContext" />.
/// </summary>
/// <typeparam name="TEventArgs">The Revit event-args type.</typeparam>
public abstract class RevitAppPostEventHandler<TEventArgs>
    : RevitPostEventHandler<Application, TEventArgs, IRevitContext>
    where TEventArgs : RevitAPIPostEventArgs
{
    protected RevitAppPostEventHandler(ControlledApplication application) : base(application.ActiveAddInId.GetGUID())
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
///     Convenience base class for post-event handlers whose sender is
///     <see cref="UIApplication" /> and whose context is <see cref="IRevitUiContext" />.
/// </summary>
/// <typeparam name="TEventArgs">The Revit event-args type.</typeparam>
public abstract class RevitUiPostEventHandler<TEventArgs>
    : RevitPostEventHandler<UIApplication, TEventArgs, IRevitUiContext>
    where TEventArgs : RevitAPIPostEventArgs
{
    protected RevitUiPostEventHandler(UIControlledApplication application) : base(application.ActiveAddInId.GetGUID())
    {
        Application = application;
    }

    /// <summary>Gets the UI controlled application this handler is registered on.</summary>
    protected UIControlledApplication Application { get; }

    /// <inheritdoc />
    protected override IRevitUiContext? CreateContext(UIApplication? sender, TEventArgs args)
        => sender is not null ? new RevitUiContext(sender) : null;
}
