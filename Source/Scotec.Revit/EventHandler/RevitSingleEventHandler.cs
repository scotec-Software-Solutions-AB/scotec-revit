// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System;

namespace Scotec.Revit.EventHandler;

/// <summary>
///     Base class for single-event handlers with a strongly-typed sender, event args, and context.
/// </summary>
/// <typeparam name="TSender">The type of the event sender.</typeparam>
/// <typeparam name="TEventArgs">The Revit single-event-args type.</typeparam>
/// <typeparam name="TContext">The Revit context type produced for each invocation.</typeparam>
public abstract class RevitSingleEventHandler<TSender, TEventArgs, TContext> : RevitEventHandler<TSender, TEventArgs, TContext>
    where TSender : class
    where TEventArgs : RevitAPISingleEventArgs
    where TContext : class, IRevitContext
{
    /// <summary>
    ///     Initializes a new instance of <see cref="RevitSingleEventHandler{TSender, TEventArgs, TContext}" />.
    /// </summary>
    /// <param name="addInId">The add-in GUID used to resolve the root DI container.</param>
    protected RevitSingleEventHandler(Guid addInId) : base(addInId)
    {
    }
}

/// <summary>
///     Convenience base class for single-event handlers whose sender is
///     <see cref="Application" /> and whose context is <see cref="IRevitContext" />.
/// </summary>
/// <typeparam name="TEventArgs">The Revit event-args type.</typeparam>
public abstract class RevitAppSingleEventHandler<TEventArgs>
    : RevitSingleEventHandler<Application, TEventArgs, IRevitContext>
    where TEventArgs : RevitAPISingleEventArgs
{
    /// <summary>
    ///     Initializes a new instance of <see cref="RevitAppSingleEventHandler{TEventArgs}" />.
    /// </summary>
    /// <param name="application">The Revit controlled application to register the event on.</param>
    protected RevitAppSingleEventHandler(ControlledApplication application) : base(application.ActiveAddInId.GetGUID())
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
///     Convenience base class for single-event handlers whose sender is
///     <see cref="UIApplication" /> and whose context is <see cref="IRevitUiContext" />.
/// </summary>
/// <typeparam name="TEventArgs">The Revit event-args type.</typeparam>
public abstract class RevitUiSingleEventHandler<TEventArgs>
    : RevitSingleEventHandler<UIApplication, TEventArgs, IRevitUiContext>
    where TEventArgs : RevitAPISingleEventArgs
{
    /// <summary>
    ///     Initializes a new instance of <see cref="RevitUiSingleEventHandler{TEventArgs}" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application to register the event on.</param>
    protected RevitUiSingleEventHandler(UIControlledApplication application) : base(application.ActiveAddInId.GetGUID())
    {
        Application = application;
    }

    /// <summary>Gets the UI controlled application this handler is registered on.</summary>
    protected UIControlledApplication Application { get; }

    /// <inheritdoc />
    protected override IRevitUiContext? CreateContext(UIApplication? sender, TEventArgs args)
        => sender is not null ? new RevitUiContext(sender) : null;
}
