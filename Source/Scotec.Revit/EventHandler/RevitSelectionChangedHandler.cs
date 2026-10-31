// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using JetBrains.Annotations;

namespace Scotec.Revit.EventHandler;

/// <summary>
///     Handles the Revit <see cref="UIControlledApplication.SelectionChanged" /> event.
/// </summary>
/// <remarks>
///     Only available when the application is registered as <c>IExternalApplication</c> (via <see cref="RevitApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="SelectionChangedEventArgs" /> and the
///         <see cref="UIApplication" /> sender as <see cref="IRevitUiContext" />.
///     </para>
///     <para>
///         <strong>Performance note:</strong> this event fires on every selection change in the active document.
///         Avoid expensive or blocking work inside this handler.
///     </para>
/// </remarks>
[PublicAPI]
public class RevitSelectionChangedHandler : RevitUiSingleEventHandler<SelectionChangedEventArgs>
{
    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="UIControlledApplication.SelectionChanged" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application.</param>
    public RevitSelectionChangedHandler(UIControlledApplication application)
        : base(application)
    {
        Subscribe();
    }

    /// <inheritdoc />
    protected sealed override void Subscribe()
    {
        Application.SelectionChanged += HandleEvent;
    }

    /// <inheritdoc />
    protected sealed override void Unsubscribe()
    {
        Application.SelectionChanged -= HandleEvent;
    }
}
