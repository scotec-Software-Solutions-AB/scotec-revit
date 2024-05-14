// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Scotec.Revit;

internal sealed class RevitHostBuilder : HostBuilder
{
    public RevitHostBuilder(RevitApp app)
    {
        // Add required services.
        UseServiceProviderFactory(new AutofacServiceProviderFactory())

            // Add required services.
            .ConfigureServices(services =>
            {
                services.AddSingleton(app.Application);
                services.AddSingleton(app.Application.ActiveAddInId);
                services.AddSingleton(app.Application.ControlledApplication);
            })

            // Add Appsettings.
            .ConfigureAppConfiguration(builder =>
            {
                builder.SetBasePath(app.GetAddinPath());
                builder.AddJsonFile("appsettings.json", true, false);
                var environment = Environment.GetEnvironmentVariable("REVIT_ENVIRONMENT");
                if (!string.IsNullOrEmpty(environment))
                {
                    builder.AddJsonFile($"appsettings.{environment}.json", true, false);
                }
            });
    }
}
