// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Scotec.Revit;

/// <summary>
///     Represents an abstract base class for a Revit Database Application.
/// </summary>
/// <remarks>
///     This class extends <see cref="RevitAppBase" /> and implements <see cref="IExternalDBApplication" />.
///     It provides a foundation for creating Revit applications that leverage services such as logging,
///     dependency injection, and hosted services.
/// </remarks>
/// <example>
///     To use this class, derive from it and override the necessary methods to implement your application's functionality.
/// </example>
public abstract class RevitDbApp : RevitAppBase, IExternalDBApplication
{
    private static readonly Type[] ControlledApplicationSignature = [typeof(ControlledApplication)];

    /// <summary>
    ///     Gets the <see cref="ControlledApplication" /> instance associated with the Revit application.
    /// </summary>
    /// <remarks>
    ///     This property provides access to the Revit application's controlled environment,
    ///     allowing interaction with its settings, events, and other application-level features.
    /// </remarks>
    /// <value>
    ///     The <see cref="ControlledApplication" /> instance representing the Revit application.
    /// </value>
    /// <example>
    ///     Use this property to access application-level functionality, such as subscribing to events:
    ///     <code>
    /// Application.DocumentOpened += (sender, args) =>
    /// {
    ///     // Handle the document opened event
    /// };
    /// </code>
    /// </example>
    public ControlledApplication? Application { get; private set; }

    /// <inheritdoc />
    protected override Type[] StandardLifecycleApplicationSignature => ControlledApplicationSignature;

    /// <inheritdoc />
    protected override Type LifecycleStopType => typeof(RevitDbApp);

    /// <inheritdoc />
    ExternalDBApplicationResult IExternalDBApplication.OnStartup(ControlledApplication application)
    {
        Application = application;

        return StartupCore(application.ActiveAddInId)
            ? ExternalDBApplicationResult.Succeeded
            : ExternalDBApplicationResult.Failed;
    }

    /// <summary>
    ///     Executes tasks when Revit starts.
    /// </summary>
    /// <param name="application">
    ///     The <see cref="ControlledApplication" /> instance provided by Revit.
    /// </param>
    /// <returns>
    ///     A boolean value indicating whether the startup process was successful.
    /// </returns>
    /// <remarks>
    ///     Override this method in a derived class to implement custom startup logic with access to the
    ///     Revit <see cref="ControlledApplication" />, or declare a custom <c>OnStartup</c> method marked with
    ///     <see cref="RevitApplicationStartupAttribute" /> with additional DI-resolved parameters.
    /// </remarks>
    protected virtual bool OnStartup(ControlledApplication application)
    {
        return true;
    }

    /// <summary>
    ///     Executes tasks during the shutdown process of the Revit application.
    /// </summary>
    /// <param name="application">
    ///     The <see cref="ControlledApplication" /> instance provided by Revit.
    /// </param>
    /// <returns>
    ///     A boolean value indicating the success or failure of the shutdown process.
    /// </returns>
    /// <remarks>
    ///     Override this method in a derived class to define custom shutdown behavior with access to the
    ///     Revit <see cref="ControlledApplication" />, or declare a custom <c>OnShutdown</c> method marked with
    ///     <see cref="RevitApplicationShutdownAttribute" /> with additional DI-resolved parameters.
    /// </remarks>
    protected virtual bool OnShutdown(ControlledApplication application)
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
    ///     such as the <see cref="ControlledApplication" />, the active add-in ID, and the controlled application,
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


    /// <summary>
    ///     Handles the shutdown process for the Revit Database Application.
    /// </summary>
    /// <param name="application">
    ///     The <see cref="Autodesk.Revit.ApplicationServices.ControlledApplication" /> instance representing the Revit
    ///     application.
    /// </param>
    /// <returns>
    ///     An <see cref="Autodesk.Revit.DB.ExternalDBApplicationResult" /> indicating the result of the shutdown process.
    ///     Returns <see cref="Autodesk.Revit.DB.ExternalDBApplicationResult.Succeeded" /> if the shutdown process completes
    ///     successfully,
    ///     otherwise returns <see cref="Autodesk.Revit.DB.ExternalDBApplicationResult.Failed" />.
    /// </returns>
    /// <remarks>
    ///     This method is invoked by the Revit API when the application is being shut down. It delegates the shutdown logic
    ///     to the <see cref="RevitAppBase.OnShutdown()" /> method and converts its result into the appropriate Revit API
    ///     result.
    /// </remarks>
    /// <example>
    ///     Override the <see cref="RevitAppBase.OnShutdown()" /> method in a derived class to implement custom shutdown logic.
    /// </example>
    ExternalDBApplicationResult IExternalDBApplication.OnShutdown(ControlledApplication application)
    {
        return ShutdownCore(application) ? ExternalDBApplicationResult.Succeeded : ExternalDBApplicationResult.Failed;
    }

    /// <summary>
    ///     Configures the application by customizing the provided <see cref="IHostBuilder" /> instance.
    /// </summary>
    /// <param name="builder">
    ///     The <see cref="IHostBuilder" /> instance used to configure the application's services and dependencies.
    /// </param>
    /// <remarks>
    ///     This method extends the base configuration by registering the <see cref="Application" /> instance
    ///     and its associated <see cref="Autodesk.Revit.DB.AddInId" /> as singleton services.
    ///     When overriding this method in a derived class, always call <c>base.OnConfigure(builder)</c> to ensure
    ///     that the Revit-specific services are registered correctly.
    /// </remarks>
    /// <seealso cref="RevitAppBase.OnConfigure(IHostBuilder)" />
    protected override void OnConfigure(IHostBuilder builder)
    {
        base.OnConfigure(builder);

        if (Application == null)
        {
            throw new InvalidOperationException("The Revit application instance is not available.");
        }

        builder.ConfigureServices(services =>
        {
            services.TryAddSingleton(Application);
            services.TryAddSingleton(Application.ActiveAddInId);
            services.AddGlobalRevitContext(Application);
        });
    }
}
