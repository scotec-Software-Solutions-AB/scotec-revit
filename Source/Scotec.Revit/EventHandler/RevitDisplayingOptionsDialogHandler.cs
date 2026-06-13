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
///     Handles the Revit <see cref="UIControlledApplication.DisplayingOptionsDialog" /> event.
/// </summary>
/// <remarks>
///     Only available when the application is registered as <c>IExternalApplication</c> (via <see cref="RevitApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="DisplayingOptionsDialogEventArgs" />.
///         Use the event args to add custom tabs to the Revit Options dialog.
///     </para>
/// </remarks>
public abstract class RevitDisplayingOptionsDialogHandler : RevitPreEventHandler<DisplayingOptionsDialogEventArgs>
{
    private readonly UIControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="UIControlledApplication.DisplayingOptionsDialog" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application.</param>
    protected RevitDisplayingOptionsDialogHandler(UIControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.DisplayingOptionsDialog += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.DisplayingOptionsDialog -= HandleEvent;
    }
}
