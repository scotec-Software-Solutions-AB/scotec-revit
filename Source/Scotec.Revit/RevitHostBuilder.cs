﻿// Copyright © 2023 Olaf Meyer
// Copyright © 2023 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Scotec.Revit;

/// <summary>
/// The program initialization.
/// </summary>
public class RevitHostBuilder : IHostBuilder
{
    private readonly IHostBuilder _hostBuilder;

    /// <summary>
    /// 
    /// </summary>
    public RevitHostBuilder(RevitApp app)
    {
        _hostBuilder = new HostBuilder()
                       .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                       .ConfigureServices(services =>
                       {
                           services.AddSingleton(app.Application);
                           services.AddSingleton(app.Application.ActiveAddInId);
                           services.AddSingleton(app.Application.ControlledApplication);
                       })
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

    /// <inheritdoc/>
    public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
    {
        return _hostBuilder.ConfigureHostConfiguration(configureDelegate);
    }

    /// <inheritdoc/>
    public IHostBuilder ConfigureAppConfiguration(
        Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
    {
        return _hostBuilder.ConfigureAppConfiguration(configureDelegate);
    }

    /// <inheritdoc/>
    public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
    {
        return _hostBuilder.ConfigureServices(configureDelegate);
    }

    /// <inheritdoc/>
    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(
        IServiceProviderFactory<TContainerBuilder> factory) where TContainerBuilder : notnull
    {
        return _hostBuilder.UseServiceProviderFactory(factory);
    }

    /// <inheritdoc/>
    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(
        Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        where TContainerBuilder : notnull
    {
        throw new InvalidOperationException("Replacing the service provider factory is not supported.");
    }

    /// <inheritdoc/>
    public IHostBuilder ConfigureContainer<TContainerBuilder>(
        Action<HostBuilderContext, TContainerBuilder> configureDelegate)
    {
        return _hostBuilder.ConfigureContainer(configureDelegate);
    }

    /// <inheritdoc/>
    public IHost Build()
    {
        var host = _hostBuilder.Build();

        return host;
    }

    /// <inheritdoc/>
    public IDictionary<object, object> Properties => _hostBuilder.Properties;
}
