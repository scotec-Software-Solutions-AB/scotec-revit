// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Scotec.Revit;

/// <summary>
///     Represents a specialized host builder for Revit applications, extending the functionality of
///     <see cref="HostBuilder" />.
/// </summary>
/// <remarks>
///     This class is designed to configure and initialize the hosting environment for Revit applications.
///     It integrates with Autofac for dependency injection and allows for custom application configuration.
/// </remarks>
/// <seealso cref="RevitAppBase" />
internal sealed class RevitHostBuilder : HostBuilder
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RevitHostBuilder" /> class with the specified Revit application.
    /// </summary>
    /// <param name="app">
    ///     An instance of <see cref="RevitAppBase" /> representing the Revit application for which the host is being built.
    /// </param>
    /// <remarks>
    ///     This constructor configures the hosting environment for a Revit application by:
    ///     <list type="bullet">
    ///         <item>Setting up Autofac as the service provider factory.</item>
    ///         <item>Configuring application settings using the add-in path provided by the <paramref name="app" /> instance.</item>
    ///     </list>
    /// </remarks>
    /// <seealso cref="RevitAppBase" />
    public RevitHostBuilder(RevitAppBase app)
    {
        // Captured by the ConfigureContainer closure below.
        // ConfigureServices callbacks all receive the same IServiceCollection instance and
        // run before ConfigureContainer during Build(), so by the time ConfigureContainer
        // is invoked this reference points to the fully populated service collection —
        // including any AddRevitScopeFactory call made by the add-in in OnConfigure.
        IServiceCollection? capturedServices = null;

        // Add required services.
        UseServiceProviderFactory(new AutofacServiceProviderFactory())

            // Add AppSettings.
            .ConfigureAppConfiguration((context, builder) => { ConfigureApp(app.GetAddInPath(), builder); })

            // Add required services.
            .ConfigureServices((context, services) =>
            {
                capturedServices = services;
                services.AddLogging(loggingBuilder => { loggingBuilder.AddConfiguration(context.Configuration.GetSection("Logging")); });
            })

            // Install the IServiceScopeFactory → IRevitScopeFactory forwarding rule in the
            // root container when the add-in has opted in to IRevitScopeFactory. The rule is
            // registered here (root) and in PopulateRevit (child scopes) so that resolving
            // IServiceScopeFactory from any scope — root or child — always returns the
            // Revit-aware factory rather than Autofac's AutofacServiceScopeFactory.
            .ConfigureContainer<ContainerBuilder>((_, builder) =>
            {
                if (capturedServices?.Any(d => d.ServiceType == typeof(IRevitScopeFactory)) == true)
                {
                    builder.RegisterRevitScopeFactoryForwarding();
                }
            });
    }

    /// <summary>
    ///     Configures the application settings for the Revit host using the specified add-in path and configuration builder.
    /// </summary>
    /// <param name="addInPath">
    ///     The directory path of the current add-in, used as the base path for configuration files.
    /// </param>
    /// <param name="builder">
    ///     An instance of <see cref="IConfigurationBuilder" /> used to build the application's configuration.
    /// </param>
    /// <remarks>
    ///     This method performs the following actions:
    ///     <list type="bullet">
    ///         <item>Sets the base path for configuration files to the specified <paramref name="addInPath" />.</item>
    ///         <item>Adds the default configuration file <c>appsettings.json</c> if it exists.</item>
    ///         <item>
    ///             Adds an environment-specific configuration file (e.g., <c>appsettings.{environment}.json</c>)
    ///             if the <c>REVIT_ENVIRONMENT</c> environment variable is set and not empty.
    ///         </item>
    ///     </list>
    /// </remarks>
    private static void ConfigureApp(string addInPath, IConfigurationBuilder builder)
    {
        builder.SetBasePath(addInPath);
        builder.AddJsonFile("appsettings.json", true, false);
        var environment = Environment.GetEnvironmentVariable("REVIT_ENVIRONMENT");
        if (!string.IsNullOrEmpty(environment))
        {
            builder.AddJsonFile($"appsettings.{environment}.json", true, false);
        }
    }
}
