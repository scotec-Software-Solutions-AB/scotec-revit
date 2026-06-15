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
///     Handles the Revit <see cref="UIControlledApplication.ViewActivated" /> event.
/// </summary>
/// <remarks>
///     Only available when the application is registered as <c>IExternalApplication</c> (via <see cref="RevitApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="ViewActivatedEventArgs" />, the
///         <see cref="UIApplication" />, the active <see cref="UIDocument" />, the active <see cref="Document" />,
///         and the newly activated <see cref="View" />.
///     </para>
/// </remarks>
public abstract class RevitViewActivatedHandler : RevitPostDocumentEventHandler<ViewActivatedEventArgs>
{
    private readonly UIControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="UIControlledApplication.ViewActivated" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application.</param>
    protected RevitViewActivatedHandler(UIControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
        Subscribe();
    }

    /// <inheritdoc />
    protected sealed override void Subscribe()
    {
        _application.ViewActivated += HandleEvent;
    }

    /// <inheritdoc />
    protected sealed override void Unsubscribe()
    {
        _application.ViewActivated -= HandleEvent;
    }

    /// <inheritdoc />
    protected override void RegisterEventContext(IServiceCollection services, object sender, ViewActivatedEventArgs args)
    {
        if (sender is UIApplication uiApplication)
        {
            var context = new RevitUiContext(uiApplication);
            services.AddScoped<IRevitUiContext>(_ => context);
        }
    }
}
