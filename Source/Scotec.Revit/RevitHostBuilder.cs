// Copyright © 2023 Olaf Meyer
// Copyright © 2023 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using System.Collections.Generic;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Scotec.Revit;

internal class RevitHostBuilder : IHostBuilder
{
    private readonly IHostBuilder _hostBuilder;

    public RevitHostBuilder(RevitApp app)
    {
        _hostBuilder = new HostBuilder()
                       .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                       .ConfigureServices(services =>
                       {
                           services.AddSingleton(app.Application);
                           services.AddSingleton(app.Application.ActiveAddInId);
                           services.AddSingleton(app.Application.ControlledApplication);
                       });
    }

    public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
    {
        return _hostBuilder.ConfigureHostConfiguration(configureDelegate);
    }

    public IHostBuilder ConfigureAppConfiguration(
        Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
    {
        return _hostBuilder.ConfigureAppConfiguration(configureDelegate);
    }

    public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
    {
        return _hostBuilder.ConfigureServices(configureDelegate);
    }

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(
        IServiceProviderFactory<TContainerBuilder> factory) where TContainerBuilder : notnull
    {
        return _hostBuilder.UseServiceProviderFactory(factory);
    }

    public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(
        Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        where TContainerBuilder : notnull
    {
        throw new InvalidOperationException("Replacing the service provider factory is not supported.");
    }

    public IHostBuilder ConfigureContainer<TContainerBuilder>(
        Action<HostBuilderContext, TContainerBuilder> configureDelegate)
    {
        return _hostBuilder.ConfigureContainer(configureDelegate);
    }

    public IHost Build()
    {
        var host = _hostBuilder.Build();

        return host;
    }

    public IDictionary<object, object> Properties => _hostBuilder.Properties;
}
