// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Scotec.Revit;

/// <summary>
///     Represents an abstract base class for creating Revit external applications.
///     This class provides integration with Revit's <see cref="UIControlledApplication" /> and supports
///     advanced features such as dependency injection and service configuration.
/// </summary>
/// <remarks>
///     Derive from this class to implement custom Revit applications. It simplifies the integration
///     of hosted services, logging, and dependency injection into the Revit environment.
/// </remarks>
public abstract class RevitApp : RevitAppBase, IExternalApplication
{
    /// <summary>
    ///     Gets the <see cref="UIControlledApplication" /> instance associated with the Revit application.
    /// </summary>
    /// <value>
    ///     The <see cref="UIControlledApplication" /> instance that provides access to Revit's controlled application
    ///     features.
    /// </value>
    /// <remarks>
    ///     This property is initialized during the startup process of the Revit application and provides access to
    ///     Revit's API for managing add-ins, events, and other application-level functionalities.
    /// </remarks>
    public UIControlledApplication? Application { get; private set; }

    /// <summary>
    ///     Invoked by Revit during the startup of the external application.
    /// </summary>
    /// <param name="application">
    ///     The <see cref="UIControlledApplication" /> instance provided by Revit.
    /// </param>
    /// <returns>
    ///     A <see cref="Result" /> indicating the success or failure of the startup process.
    /// </returns>
    /// <remarks>
    ///     This method initializes the <see cref="Application" /> property and delegates the startup logic
    ///     to the <see cref="RevitAppBase.OnStartup(Autodesk.Revit.DB.AddInId)" /> method. If the base class startup process
    ///     completes successfully, it returns <see cref="Result.Succeeded" />; otherwise, it returns
    ///     <see cref="Result.Failed" />.
    /// </remarks>
    /// <exception cref="Exception">
    ///     Thrown if an error occurs during the startup process.
    /// </exception>
    Result IExternalApplication.OnStartup(UIControlledApplication application)
    {
        Application = application;

        return OnStartup(application.ActiveAddInId)
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
    ///     This method delegates the shutdown process to the <see cref="RevitAppBase.OnShutdown(ControlledApplication)" />
    ///     method
    ///     and performs additional cleanup tasks specific to the Revit external application.
    /// </remarks>
    /// <exception cref="System.Exception">
    ///     An exception may be thrown if an error occurs during the shutdown process.
    /// </exception>
    Result IExternalApplication.OnShutdown(UIControlledApplication application)
    {
        return OnShutdown(application.ControlledApplication) ? Result.Succeeded : Result.Failed;
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
    ///     such as the <see cref="UIControlledApplication" />, the active add-in ID, and the controlled application,
    ///     to the service collection.
    /// </remarks>
    /// <example>
    ///     Example usage:
    ///     <code>
    /// protected override void OnConfigure(IHostBuilder builder)
    /// {
    ///     base.OnConfigure(builder);
    ///     builder.ConfigureServices(services =>
    ///     {
    ///         services.AddSingleton(Application);
    ///         services.AddSingleton(Application.ActiveAddInId);
    ///         services.AddSingleton(Application.ControlledApplication);
    ///     });
    /// }
    /// </code>
    /// </example>
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
        });
    }
}
