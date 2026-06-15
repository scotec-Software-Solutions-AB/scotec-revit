// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit.EventHandler;

/// <summary>
///     Handles the Revit <see cref="UIControlledApplication.DialogBoxShowing" /> event.
/// </summary>
/// <remarks>
///     Only available when the application is registered as <c>IExternalApplication</c> (via <see cref="RevitApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="DialogBoxShowingEventArgs" />.
///         Use the event args to suppress or override specific Revit dialog boxes.
///     </para>
/// </remarks>
public abstract class RevitDialogBoxShowingHandler : RevitPreEventHandler<UIApplication, DialogBoxShowingEventArgs>
{
    private readonly UIControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="UIControlledApplication.DialogBoxShowing" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application.</param>
    protected RevitDialogBoxShowingHandler(UIControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
        Subscribe();
    }

    /// <inheritdoc />
    protected sealed override void Subscribe()
    {
        _application.DialogBoxShowing += HandleEvent;
    }

    /// <inheritdoc />
    protected sealed override void Unsubscribe()
    {
        _application.DialogBoxShowing -= HandleEvent;
    }
}
