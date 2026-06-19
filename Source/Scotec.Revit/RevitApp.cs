// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Scotec.Revit;

/// <summary>
///     Represents an abstract base class for creating Revit external applications.
///     This class provides integration with Revit's <see cref="Autodesk.Revit.UI.UIControlledApplication" /> and supports
///     advanced features such as dependency injection and service configuration.
/// </summary>
/// <remarks>
///     Derive from this class to implement custom Revit applications. It simplifies the integration
///     of hosted services, logging, and dependency injection into the Revit environment.
/// </remarks>
public abstract class RevitApp : RevitAppBase, IExternalApplication
{
    private static readonly Type[] UiControlledApplicationSignature = [typeof(UIControlledApplication)];

    /// <summary>
    ///     Gets the <see cref="Autodesk.Revit.UI.UIControlledApplication" /> instance associated with the Revit application.
    /// </summary>
    /// <value>
    ///     The <see cref="Autodesk.Revit.UI.UIControlledApplication" /> instance that provides access to Revit's controlled application
    ///     features.
    /// </value>
    /// <remarks>
    ///     This property is initialized during the startup process of the Revit application and provides access to
    ///     Revit's API for managing add-ins, events, and other application-level functionalities.
    /// </remarks>
    protected UIControlledApplication? Application { get; private set; }

    /// <inheritdoc />
    protected override Type[] StandardLifecycleApplicationSignature => UiControlledApplicationSignature;

    /// <inheritdoc />
    protected override Type LifecycleStopType => typeof(RevitApp);

    /// <summary>
    ///     Invoked by Revit during the startup of the external application.
    /// </summary>
    /// <param name="application">
    ///     The <see cref="Autodesk.Revit.UI.UIControlledApplication" /> instance provided by Revit.
    /// </param>
    /// <returns>
    ///     A <see cref="Autodesk.Revit.UI.Result" /> indicating the success or failure of the startup process.
    /// </returns>
    /// <remarks>
    ///     This method initializes the <see cref="RevitApp.Application" /> property and delegates the startup logic
    ///     to the <see cref="RevitAppBase.StartupCore(Autodesk.Revit.DB.AddInId)" /> method. If the base class startup process
    ///     completes successfully, it returns <see cref="Autodesk.Revit.UI.Result.Succeeded" />; otherwise, it returns
    ///     <see cref="Autodesk.Revit.UI.Result.Failed" />.
    /// </remarks>
    /// <exception cref="Exception">
    ///     Thrown if an error occurs during the startup process.
    /// </exception>
    Result IExternalApplication.OnStartup(UIControlledApplication application)
    {
        Application = application;

        return StartupCore(application.ActiveAddInId)
            ? Result.Succeeded
            : Result.Failed;
    }

    /// <summary>
    ///     Handles the shutdown process for the Revit external application.
    /// </summary>
    /// <param name="application">
    ///     The <see cref="Autodesk.Revit.UI.UIControlledApplication" /> instance representing the Revit application.
    /// </param>
    /// <returns>
    ///     A <see cref="Autodesk.Revit.UI.Result" /> value indicating whether the shutdown process was successful.
    ///     Returns <see cref="Autodesk.Revit.UI.Result.Succeeded" /> if the shutdown was successful; otherwise,
    ///     <see cref="Autodesk.Revit.UI.Result.Failed" />.
    /// </returns>
    /// <remarks>
    ///     This method delegates the shutdown process to the <see cref="RevitApp.OnShutdown(Autodesk.Revit.UI.UIControlledApplication)" />
    ///     method
    ///     and performs additional cleanup tasks specific to the Revit external application.
    /// </remarks>
    /// <exception cref="System.Exception">
    ///     An exception may be thrown if an error occurs during the shutdown process.
    /// </exception>
    Result IExternalApplication.OnShutdown(UIControlledApplication application)
    {
        return ShutdownCore(application.ControlledApplication) ? Result.Succeeded : Result.Failed;
    }

    /// <summary>
    ///     Executes tasks when Revit starts.
    /// </summary>
    /// <param name="application">
    ///     The <see cref="Autodesk.Revit.UI.UIControlledApplication" /> instance provided by Revit.
    /// </param>
    /// <returns>
    ///     A boolean value indicating whether the startup process was successful.
    /// </returns>
    /// <remarks>
    ///     Override this method in a derived class to implement custom startup logic with access to the
    ///     Revit <see cref="Autodesk.Revit.UI.UIControlledApplication" />, or declare a custom <c>OnStartup</c> method marked with
    ///     <see cref="RevitApplicationStartupAttribute" /> with additional DI-resolved parameters.
    /// </remarks>
    [UsedImplicitly]
    protected virtual bool OnStartup(UIControlledApplication application)
    {
        return true;
    }

    /// <summary>
    ///     Executes tasks during the shutdown process of the Revit application.
    /// </summary>
    /// <param name="application">
    ///     The <see cref="Autodesk.Revit.UI.UIControlledApplication" /> instance provided by Revit.
    /// </param>
    /// <returns>
    ///     A boolean value indicating the success or failure of the shutdown process.
    /// </returns>
    /// <remarks>
    ///     Override this method in a derived class to define custom shutdown behavior with access to the
    ///     Revit <see cref="Autodesk.Revit.UI.UIControlledApplication" />, or declare a custom <c>OnShutdown</c> method marked with
    ///     <see cref="RevitApplicationShutdownAttribute" /> with additional DI-resolved parameters.
    /// </remarks>
    [UsedImplicitly]
    protected virtual bool OnShutdown(UIControlledApplication application)
    {
        return true;
    }

    /// <summary>
    ///     Configures the host builder for the Revit application.
    /// </summary>
    /// <param name="builder">
    ///     The <see cref="IHostBuilder" /> instance used to configure services and application settings.
    /// </param>
    /// <remarks>
    ///     This method is invoked during the initialization of the Revit application to configure
    ///     dependency injection and service registration. It adds essential Revit-specific services,
    ///     such as the <see cref="Autodesk.Revit.UI.UIControlledApplication" />, the active add-in ID, and the controlled application,
    ///     to the service collection.
    /// </remarks>
    protected override void OnConfigure(IHostBuilder builder)
    {
        base.OnConfigure(builder);

        if (Application == null)
        {
            throw new InvalidOperationException("The Revit application instance is not available.");
        }

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(Application);
            services.AddSingleton(Application.ActiveAddInId);
            services.AddSingleton(Application.ControlledApplication);
            services.AddGlobalRevitContext(Application);
        });
    }
}
