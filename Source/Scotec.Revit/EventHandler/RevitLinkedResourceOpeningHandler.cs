// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit.EventHandler;

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.LinkedResourceOpening" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="LinkedResourceOpeningEventArgs" />.
///     </para>
/// </remarks>
[PublicAPI]
public abstract class RevitLinkedResourceOpeningHandler : RevitAppPreEventHandler<LinkedResourceOpeningEventArgs>
{

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.LinkedResourceOpening" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitLinkedResourceOpeningHandler(ControlledApplication application)
        : base(application)
    {
        Subscribe();
    }

    /// <inheritdoc />
    protected sealed override void Subscribe()
    {
        Application.LinkedResourceOpening += HandleEvent;
    }

    /// <inheritdoc />
    protected sealed override void Unsubscribe()
    {
        Application.LinkedResourceOpening -= HandleEvent;
    }
}
