// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using JetBrains.Annotations;

namespace Scotec.Revit.EventHandler;

/// <summary>
///     Handles the Revit <see cref="UIControlledApplication.Idling" /> event.
/// </summary>
/// <remarks>
///     Only available when the application is registered as <c>IExternalApplication</c> (via <see cref="RevitApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="IdlingEventArgs" /> and the
///         <see cref="UIApplication" /> sender as <see cref="IRevitUiContext" />.
///     </para>
///     <para>
///         <strong>Performance note:</strong> this event fires repeatedly during idle periods.
///         Avoid blocking work. Use <see cref="IdlingEventArgs.SetRaiseWithoutDelay" /> only when continuous
///         polling is truly required, and revert to the default behavior as soon as possible.
///     </para>
/// </remarks>
[PublicAPI]
public class RevitIdlingHandler : RevitUiPreEventHandler<IdlingEventArgs>
{

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="UIControlledApplication.Idling" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application.</param>
    public RevitIdlingHandler(UIControlledApplication application)
        : base(application)
    {
        Subscribe();
    }

    /// <inheritdoc />
    protected sealed override void Subscribe()
    {
        Application.Idling += HandleEvent;
    }

    /// <inheritdoc />
    protected sealed override void Unsubscribe()
    {
        Application.Idling -= HandleEvent;
    }
}
