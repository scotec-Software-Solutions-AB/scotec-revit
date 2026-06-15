// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit.EventHandler;

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.FailuresProcessing" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="FailuresProcessingEventArgs" />.
///         Use <see cref="FailuresProcessingEventArgs.GetFailuresAccessor" /> to inspect and resolve failures.
///     </para>
/// </remarks>
public abstract class RevitFailuresProcessingHandler : RevitSingleEventHandler<Application, FailuresProcessingEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.FailuresProcessing" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitFailuresProcessingHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
        Subscribe();
    }

    /// <inheritdoc />
    protected sealed override void Subscribe()
    {
        _application.FailuresProcessing += HandleEvent;
    }

    /// <inheritdoc />
    protected sealed override void Unsubscribe()
    {
        _application.FailuresProcessing -= HandleEvent;
    }
}
