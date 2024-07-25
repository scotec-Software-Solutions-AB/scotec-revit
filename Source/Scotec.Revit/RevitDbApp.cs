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
///     Basic implementation for a Revit app. Derive from this class to use services like logging, dependency injection, or
///     hosted services.
/// </summary>
public abstract class RevitDbApp : RevitAppBase, IExternalDBApplication
{
    /// <summary>
    /// </summary>
    public ControlledApplication Application { get; private set; }

    /// <inheritdoc />
    ExternalDBApplicationResult IExternalDBApplication.OnStartup(ControlledApplication application)
    {
        Application = application;
        
        return OnStartup(application.ActiveAddInId) 
            ? ExternalDBApplicationResult.Succeeded 
            : ExternalDBApplicationResult.Failed;
    }

    /// <inheritdoc />
    ExternalDBApplicationResult IExternalDBApplication.OnShutdown(ControlledApplication application)
    {
        return OnShutdown(application) ? ExternalDBApplicationResult.Succeeded : ExternalDBApplicationResult.Failed;
    }

    /// <inheritdoc />
    protected override void OnConfigure(IHostBuilder builder)
    {
        base.OnConfigure(builder);

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(Application);
            services.AddSingleton(Application.ActiveAddInId);
        });
    }

}
