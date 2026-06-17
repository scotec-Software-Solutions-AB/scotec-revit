// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using JetBrains.Annotations;

namespace Scotec.Revit.EventHandler;

/// <summary>
///     Handles the Revit <see cref="UIControlledApplication.ViewActivated" /> event.
/// </summary>
/// <remarks>
///     Only available when the application is registered as <c>IExternalApplication</c> (via <see cref="RevitApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="ViewActivatedEventArgs" />, the
///         <see cref="UIApplication" />, the active <see cref="UIDocument" />, the active <see cref="Autodesk.Revit.DB.Document" />,
///         and the newly activated <see cref="Autodesk.Revit.DB.View" />.
///     </para>
/// </remarks>
[PublicAPI]
public abstract class RevitViewActivatedHandler : RevitUiPostDocumentEventHandler<ViewActivatedEventArgs>
{

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="UIControlledApplication.ViewActivated" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application.</param>
    protected RevitViewActivatedHandler(UIControlledApplication application)
        : base(application)
    {
        Subscribe();
    }

    /// <inheritdoc />
    protected sealed override void Subscribe()
    {
        Application.ViewActivated += HandleEvent;
    }

    /// <inheritdoc />
    protected sealed override void Unsubscribe()
    {
        Application.ViewActivated -= HandleEvent;
    }
}
