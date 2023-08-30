// Copyright © 2023 Olaf Meyer
// Copyright © 2023 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autodesk.Revit.UI;
using Microsoft.Extensions.Hosting;

namespace Scotec.Revit;

/// <summary>
///     Basic implementation for a Revit app. Derive from this class to use services like logging, dependency injection, or
///     hosted services.
/// </summary>
public abstract class RevitApp : IExternalApplication
{
    /// <summary>
    ///     Due to Revit's assembly resolver implementation, assemblies used in multiple addins are only loaded once. Static
    ///     members are therefore shared between multiple addins. Since each add-in should use its own service provider, the
    ///     individual service providers are kept in a static dictionary. The addin ID serves as the key here. The
    ///     addin-specific service providers are called when executing Revit commands.
    /// </summary>
    private static readonly Dictionary<Guid, IServiceProvider> ServiceProviders;

    static RevitApp()
    {
        ServiceProviders = new Dictionary<Guid, IServiceProvider>();
    }

    /// <summary>
    ///     Returns the Autodesk Revit user interface, providing access to  UI customization methods and events.
    /// </summary>
    protected internal UIControlledApplication Application { get; internal set; }

    protected internal IHost Host { get; internal set; }

    /// <summary>
    ///     Returns the ID of the addin.
    /// </summary>
    protected Guid AddinId { get; private set; }

    /// <summary>
    ///     Provides access to the service provider.
    /// </summary>
    protected IServiceProvider Services => GetServiceProvider(AddinId);

    /// <inheritdoc />
    Result IExternalApplication.OnStartup(UIControlledApplication application)
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;

        AddinId = application.ActiveAddInId.GetGUID();
        Application = application;

        try
        {
            var builder = CreateRevitHostBuilder();
            OnConfigure(builder);

            Host = builder.Build();
            Host.Start();

            AddServiceProvider(Host.Services);
            return OnStartup();
        }
        catch (Exception)
        {
            return Result.Failed;
        }
    }

    /// <inheritdoc />
    Result IExternalApplication.OnShutdown(UIControlledApplication application)
    {
        try
        {
            var result = OnShutdown();

            ServiceProviders.Remove(application.ActiveAddInId.GetGUID());
            Host.StopAsync().GetAwaiter().GetResult();
            Host?.Dispose();
            Host = null;

            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;

            return result;
        }
        catch (Exception)
        {
            return Result.Failed;
        }
    }

    /// <summary>
    ///     Can be overridden to add or configure services such as logging or hosted services.
    /// </summary>
    protected virtual void OnConfigure(IHostBuilder builder)
    {
    }

    /// <summary>
    ///     Implement this method to execute some tasks when Revit shuts down.
    /// </summary>
    protected abstract Result OnShutdown();

    /// <summary>
    ///     Implement this method to execute some tasks when Revit starts
    /// </summary>
    protected abstract Result OnStartup();

    /// <summary>
    ///     Can be overridden to implement a custom assembly resolver.
    /// </summary>
    /// <returns>Returns the loaded assembly or null if the assembly could not be loaded.</returns>
    protected virtual Assembly OnAssemblyResolve(ResolveEventArgs args)
    {
        return null;
    }

    internal static IServiceProvider GetServiceProvider(Guid addinId)
    {
        return ServiceProviders[addinId];
    }

    private void AddServiceProvider(IServiceProvider services)
    {
        ServiceProviders.Add(AddinId, services);
    }

    private IHostBuilder CreateRevitHostBuilder()
    {
        return new RevitHostBuilder(this);
    }

    private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        return OnAssemblyResolve(args)
               ?? OnAssemblyResolveBase(args);
    }

    private static Assembly OnAssemblyResolveBase(ResolveEventArgs args)
    {
        var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var assemblyName = new AssemblyName(args.Name);

        var assemblyPath = Path.Combine(currentPath!, assemblyName.Name + ".dll");
        return File.Exists(assemblyPath)
            ? Assembly.LoadFrom(assemblyPath)
            : null;
    }
}
