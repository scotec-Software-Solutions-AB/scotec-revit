// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;

namespace Scotec.Revit.EventHandler;

/// <summary>
///     Base class for pre-document-event handlers with a strongly-typed sender, event args, and context.
/// </summary>
/// <typeparam name="TSender">The type of the event sender.</typeparam>
/// <typeparam name="TEventArgs">The Revit pre-document-event-args type.</typeparam>
/// <typeparam name="TContext">The Revit context type produced for each invocation.</typeparam>
public abstract class RevitPreDocumentEventHandler<TSender, TEventArgs, TContext> : RevitPreEventHandler<TSender, TEventArgs, TContext>
    where TSender : class
    where TEventArgs : RevitAPIPreDocEventArgs
    where TContext : class, IRevitContext
{
    /// <summary>
    ///     Initializes a new instance of <see cref="RevitPreDocumentEventHandler{TSender, TEventArgs, TContext}" />.
    /// </summary>
    /// <param name="addInId">The add-in GUID used to resolve the root DI container.</param>
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
    /// <summary>
    ///     Initializes a new instance of <see cref="RevitAppPreDocumentEventHandler{TEventArgs}" />.
    /// </summary>
    /// <param name="application">The Revit controlled application to register the event on.</param>
    protected RevitAppPreDocumentEventHandler(ControlledApplication application) : base(application.ActiveAddInId.GetGUID())
    {
        Application = application;
    }

    /// <summary>Gets the controlled application this handler is registered on.</summary>
    protected ControlledApplication Application { get; }

    /// <inheritdoc />
    protected override IRevitContext? CreateContext(Application? sender, TEventArgs args)
    {
        // Revit API: args.Document is preferred when available (e.g. DocumentOpening before it is set as ActiveDocument).
        // Fall back to Application when args.Document is null (e.g. DocumentCreating before the document exists).
        if (args.Document is { } document)
            return new RevitContext(document);
        return sender is not null ? new RevitContext(sender) : null;
    }
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
    /// <summary>
    ///     Initializes a new instance of <see cref="RevitUiPreDocumentEventHandler{TEventArgs}" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application to register the event on.</param>
    protected RevitUiPreDocumentEventHandler(UIControlledApplication application) : base(application.ActiveAddInId.GetGUID())
    {
        Application = application;
    }

    /// <summary>Gets the UI controlled application this handler is registered on.</summary>
    protected UIControlledApplication Application { get; }

    /// <inheritdoc />
    protected override IRevitUiContext? CreateContext(UIApplication? sender, TEventArgs args)
        => sender is not null ? new RevitUiContext(sender) : null;
}
