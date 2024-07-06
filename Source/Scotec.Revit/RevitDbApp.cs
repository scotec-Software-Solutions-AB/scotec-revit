// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Microsoft.Extensions.DependencyInjection;
using System;

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

        var configureServices = new Action<IServiceCollection>(services =>
        {
            services.AddSingleton(application);
            services.AddSingleton(application.ActiveAddInId);

        });

        return OnStartup(application.ActiveAddInId, configureServices) 
            ? ExternalDBApplicationResult.Succeeded 
            : ExternalDBApplicationResult.Failed;
    }

    /// <inheritdoc />
    ExternalDBApplicationResult IExternalDBApplication.OnShutdown(ControlledApplication application)
    {
        return OnShutdown(application) ? ExternalDBApplicationResult.Succeeded : ExternalDBApplicationResult.Failed;
    }
}
