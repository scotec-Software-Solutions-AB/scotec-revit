// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;

namespace Scotec.Revit.EventHandler;

public abstract class RevitPreDocumentEventHandler<TSender, TEventArgs, TContext> : RevitPreEventHandler<TSender, TEventArgs, TContext>
    where TSender : class
    where TEventArgs : RevitAPIPreDocEventArgs
    where TContext : class, IRevitContext
{
    protected RevitPreDocumentEventHandler(Guid addInId) : base(addInId)
    {
    }
}

/// <summary>
///     Convenience base class for pre-document-event handlers whose sender is
///     <see cref="Application" /> and whose context is <see cref="IRevitContext" />.
/// </summary>
/// <typeparam name="TEventArgs">The Revit event-args type.</typeparam>
public abstract class RevitAppPreDocumentEventHandler<TEventArgs>
    : RevitPreDocumentEventHandler<Application, TEventArgs, IRevitContext>
    where TEventArgs : RevitAPIPreDocEventArgs
{
    protected RevitAppPreDocumentEventHandler(ControlledApplication application) : base(application.ActiveAddInId.GetGUID())
    {
        Application = application;
    }

    /// <summary>Gets the controlled application this handler is registered on.</summary>
    protected ControlledApplication Application { get; }

    /// <inheritdoc />
    protected override IRevitContext? CreateContext(Application? sender, TEventArgs args)
        => args.Document is not null ? new RevitContext(args.Document) : null;
}

/// <summary>
///     Convenience base class for pre-document-event handlers whose sender is
///     <see cref="UIApplication" /> and whose context is <see cref="IRevitUiContext" />.
/// </summary>
/// <typeparam name="TEventArgs">The Revit event-args type.</typeparam>
public abstract class RevitUiPreDocumentEventHandler<TEventArgs>
    : RevitPreDocumentEventHandler<UIApplication, TEventArgs, IRevitUiContext>
    where TEventArgs : RevitAPIPreDocEventArgs
{
    protected RevitUiPreDocumentEventHandler(UIControlledApplication application) : base(application.ActiveAddInId.GetGUID())
    {
        Application = application;
    }

    /// <summary>Gets the UI controlled application this handler is registered on.</summary>
    protected UIControlledApplication Application { get; }

    /// <inheritdoc />
    protected override IRevitUiContext? CreateContext(UIApplication? sender, TEventArgs args)
        => sender?.ActiveUIDocument is not null ? new RevitUiContext(sender) : null;
}
