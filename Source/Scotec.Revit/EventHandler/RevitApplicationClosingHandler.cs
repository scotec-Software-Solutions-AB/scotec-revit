// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit.EventHandler;

/// <summary>
///     Handles the Revit <see cref="UIControlledApplication.ApplicationClosing" /> event.
/// </summary>
/// <remarks>
///     Only available when the application is registered as <c>IExternalApplication</c> (via <see cref="RevitApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="ApplicationClosingEventArgs" />.
///     </para>
/// </remarks>
[PublicAPI]     
public class RevitApplicationClosingHandler : RevitUiPreEventHandler<ApplicationClosingEventArgs>
{

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="UIControlledApplication.ApplicationClosing" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application.</param>
    public RevitApplicationClosingHandler(UIControlledApplication application)
        : base(application)
    {
        Subscribe();
    }

    /// <inheritdoc />
    protected sealed override void Subscribe()
    {
        Application.ApplicationClosing += HandleEvent;
    }

    /// <inheritdoc />
    protected sealed override void Unsubscribe()
    {
        Application.ApplicationClosing -= HandleEvent;
    }
}
