// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Runtime.Loader;

namespace Scotec.Revit;

/// <summary>
/// Represents an abstract base class for a Revit Database Application.
/// </summary>
/// <remarks>
/// This class extends <see cref="RevitAppBase"/> and implements <see cref="IExternalDBApplication"/>.
/// It provides a foundation for creating Revit applications that leverage services such as logging, 
/// dependency injection, and hosted services.
/// </remarks>
/// <example>
/// To use this class, derive from it and override the necessary methods to implement your application's functionality.
/// </example>
public abstract class RevitDbApp : RevitAppBase, IExternalDBApplication
{
    /// <summary>
    /// Gets the <see cref="ControlledApplication"/> instance associated with the Revit application.
    /// </summary>
    /// <remarks>
    /// This property provides access to the Revit application's controlled environment, 
    /// allowing interaction with its settings, events, and other application-level features.
    /// </remarks>
    /// <value>
    /// The <see cref="ControlledApplication"/> instance representing the Revit application.
    /// </value>
    /// <example>
    /// Use this property to access application-level functionality, such as subscribing to events:
    /// <code>
    /// Application.DocumentOpened += (sender, args) =>
    /// {
    ///     // Handle the document opened event
    /// };
    /// </code>
    /// </example>
    public ControlledApplication? Application { get; private set; }

    /// <inheritdoc />
    ExternalDBApplicationResult IExternalDBApplication.OnStartup(ControlledApplication application)
    {
        Application = application;

        return OnStartup(application.ActiveAddInId) 
            ? ExternalDBApplicationResult.Succeeded 
            : ExternalDBApplicationResult.Failed;
    }

    /// <summary>
    /// Handles the shutdown process for the Revit Database Application.
    /// </summary>
    /// <param name="application">
    /// The <see cref="Autodesk.Revit.ApplicationServices.ControlledApplication"/> instance representing the Revit application.
    /// </param>
    /// <returns>
    /// An <see cref="Autodesk.Revit.DB.ExternalDBApplicationResult"/> indicating the result of the shutdown process.
    /// Returns <see cref="Autodesk.Revit.DB.ExternalDBApplicationResult.Succeeded"/> if the shutdown process completes successfully,
    /// otherwise returns <see cref="Autodesk.Revit.DB.ExternalDBApplicationResult.Failed"/>.
    /// </returns>
    /// <remarks>
    /// This method is invoked by the Revit API when the application is being shut down. It delegates the shutdown logic
    /// to the <see cref="RevitAppBase.OnShutdown()"/> method and converts its result into the appropriate Revit API result.
    /// </remarks>
    /// <example>
    /// Override the <see cref="RevitAppBase.OnShutdown()"/> method in a derived class to implement custom shutdown logic.
    /// </example>
    ExternalDBApplicationResult IExternalDBApplication.OnShutdown(ControlledApplication application)
    {
        return OnShutdown(application) ? ExternalDBApplicationResult.Succeeded : ExternalDBApplicationResult.Failed;
    }

    /// <summary>
    /// Configures the application by customizing the provided <see cref="IHostBuilder"/> instance.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="IHostBuilder"/> instance used to configure the application's services and dependencies.
    /// </param>
    /// <remarks>
    /// This method extends the base configuration by registering the <see cref="Application"/> instance 
    /// and its associated <see cref="Autodesk.Revit.DB.AddInId"/> as singleton services.
    /// </remarks>
    /// <seealso cref="RevitAppBase.OnConfigure(IHostBuilder)"/>
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
        });
    }

}
