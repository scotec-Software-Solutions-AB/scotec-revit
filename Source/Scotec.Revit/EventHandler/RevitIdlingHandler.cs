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
///     Handles the Revit <see cref="UIControlledApplication.Idling" /> event.
/// </summary>
/// <remarks>
///     Only available when the application is registered as <c>IExternalApplication</c> (via <see cref="RevitApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="IdlingEventArgs" /> and the
///         <see cref="UIApplication" /> sender.
///     </para>
///     <para>
///         <strong>Performance note:</strong> this event fires repeatedly during idle periods.
///         Avoid blocking work. Use <see cref="IdlingEventArgs.SetRaiseWithoutDelay" /> only when continuous
///         polling is truly required, and revert to the default behavior as soon as possible.
///     </para>
/// </remarks>
public abstract class RevitIdlingHandler : RevitPreEventHandler<UIApplication, IdlingEventArgs>
{
    private readonly UIControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="UIControlledApplication.Idling" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application.</param>
    protected RevitIdlingHandler(UIControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
        Subscribe();
    }

    /// <inheritdoc />
    protected sealed override void Subscribe()
    {
        _application.Idling += HandleEvent;
    }

    /// <inheritdoc />
    protected sealed override void Unsubscribe()
    {
        _application.Idling -= HandleEvent;
    }

    /// <inheritdoc />
    protected override void RegisterEventContext(IServiceCollection services, UIApplication? sender, IdlingEventArgs args)
    {
        if (sender is not null)
        {
            var context = new RevitUiContext(sender);
            services.AddScoped<IRevitUiContext>(_ => context);
            services.AddScoped<IRevitContext>(_ => context);
        }
    }
}
