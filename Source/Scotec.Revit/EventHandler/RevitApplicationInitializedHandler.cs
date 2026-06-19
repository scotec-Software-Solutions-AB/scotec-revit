// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Microsoft.Extensions.DependencyInjection;
using System;
using Autodesk.Revit.ApplicationServices;

namespace Scotec.Revit.EventHandler;

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.ApplicationInitialized" /> event.
/// </summary>
/// <remarks>
///     Only available when the application is registered as <c>IExternalApplication</c> (via <see cref="RevitApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="Autodesk.Revit.DB.Events.ApplicationInitializedEventArgs" />.
///     </para>
/// </remarks>
public class RevitApplicationInitializedHandler : RevitAppSingleEventHandler<ApplicationInitializedEventArgs>
{

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="Autodesk.Revit.DB.Events.ApplicationInitializedEventArgs" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application.</param>
    public RevitApplicationInitializedHandler(ControlledApplication application)
        : base(application)
    {
        Subscribe();
    }

    /// <inheritdoc />
    protected sealed override void Subscribe()
    {
        Application.ApplicationInitialized += HandleEvent;
    }

    /// <inheritdoc />
    protected sealed override void Unsubscribe()
    {
        Application.ApplicationInitialized -= HandleEvent;
    }
}
