// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using System.Runtime.Loader;
using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Scotec.Revit;

/// <summary>
///     Basic implementation for a Revit app. Derive from this class to use services like logging, dependency injection, or
///     hosted services.
/// </summary>
public abstract class RevitApp : RevitAppBase, IExternalApplication
{
    /// <summary>
    /// </summary>
    public UIControlledApplication Application { get; private set; }

    /// <inheritdoc />
    Result IExternalApplication.OnStartup(UIControlledApplication application)
    {
        Application = application;

        return OnStartup(application.ActiveAddInId) 
            ? Result.Succeeded 
            : Result.Failed;
    }

    /// <inheritdoc />
    Result IExternalApplication.OnShutdown(UIControlledApplication application)
    {
        return OnShutdown(application.ControlledApplication) ? Result.Succeeded : Result.Failed;
    }

    /// <inheritdoc />
    protected override void OnConfigure(IHostBuilder builder)
    {
        base.OnConfigure(builder);

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(Application);
            services.AddSingleton(Application.ActiveAddInId);
            services.AddSingleton(Application.ControlledApplication);

        });
    }
}
