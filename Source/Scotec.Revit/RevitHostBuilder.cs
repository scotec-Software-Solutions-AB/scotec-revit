// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Scotec.Revit;

internal sealed class RevitHostBuilder : HostBuilder
{
    public RevitHostBuilder(RevitAppBase app)
    {
        // Add required services.
        UseServiceProviderFactory(new AutofacServiceProviderFactory())

            // Add required services.
            //.ConfigureServices(services => { })

            // Add AppSettings.
            .ConfigureAppConfiguration(builder => { ConfigureApp(app.GetAddInPath(), builder); });
    }

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
