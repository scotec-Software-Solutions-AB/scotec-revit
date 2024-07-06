// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Scotec.Revit;

/// <summary>
///     Basic implementation for a Revit app. Derive from this class to use services like logging, dependency injection, or
///     hosted services.
/// </summary>
public abstract class RevitAppBase
{
    /// <summary>
    ///     Due to Revit's assembly resolver implementation, assemblies used in multiple add-ins are only loaded once. Static
    ///     members are therefore shared between multiple add-ins. Since each add-in should use its own service provider, the
    ///     individual service providers are kept in a static dictionary. The add-in ID serves as the key here. The
    ///     add-in-specific service providers are used when executing Revit commands.
    /// </summary>
    private static readonly Dictionary<Guid, IServiceProvider> ServiceProviders;

    static RevitAppBase()
    {
        ServiceProviders = new Dictionary<Guid, IServiceProvider>();
    }

    /// <summary>
    /// </summary>
    protected internal IHost Host { get; internal set; }

    /// <summary>
    ///     Returns the ID of the add-in.
    /// </summary>
    protected Guid AddInId { get; private set; }

    /// <summary>
    ///     Provides access to the service provider.
    /// </summary>
    protected IServiceProvider Services => GetServiceProvider(AddInId);

    /// <summary>
    ///     Returns the location of the add-in.
    /// </summary>
    public string GetAddInPath()
    {
        // Do not use Assembly.GetExecutingAssembly().Location. This assembly might be used in multiple addins but will be loaded into the
        // process only once. Therefore, do not use Assembly.GetExecutingAssembly().Location because this might not return the path of
        // the current add-in. Use GetType().Assembly-Location instead. This will return the path to the assembly that contains the derived RevitApp.
        //var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        return Path.GetDirectoryName(GetType().Assembly.Location);
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
    protected abstract bool OnShutdown();

    /// <summary>
    ///     Implement this method to execute some tasks when Revit starts
    /// </summary>
    protected abstract bool OnStartup();

    /// <summary>
    ///     Can be overridden to implement a custom assembly resolver.
    /// </summary>
    /// <returns>Returns the path assembly or null if the assembly could not be found.</returns>
    protected virtual string OnAssemblyResolve(AssemblyName assemblyName)
    {
        var currentPath = GetAddInPath();

        var assemblyPath = Path.Combine(currentPath!, assemblyName.Name + ".dll");
        return File.Exists(assemblyPath) ? assemblyPath : null;
    }

    /// <summary>
    /// </summary>
    protected bool OnStartup(AddInId addInId, Action<IServiceCollection> configureServices)
    {
        GetAssemblyLoadContext().Resolving += OnResolving;
        AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

        AddInId = addInId.GetGUID();

        try
        {
            var builder = CreateRevitHostBuilder(configureServices);
            OnConfigure(builder);

            Host = builder.Build();
            Host.Start();

            AddServiceProvider(Host.Services);
            return OnStartup();
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// </summary>
    protected bool OnShutdown(ControlledApplication application)
    {
        try
        {
            var result = OnShutdown();

            ServiceProviders.Remove(application.ActiveAddInId.GetGUID());
            Host?.StopAsync().GetAwaiter().GetResult();
            Host?.Dispose();
            Host = null;

            AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;

            return result;
        }
        catch (Exception)
        {
            return false;
        }
    }

    internal static IServiceProvider GetServiceProvider(Guid addInId)
    {
        return ServiceProviders[addInId];
    }

    private AssemblyLoadContext GetAssemblyLoadContext()
    {
        return AssemblyLoadContext.GetLoadContext(GetType().Assembly);
    }

    private Assembly OnResolving(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        var assemblyPath = OnAssemblyResolve(assemblyName);
        return assemblyPath != null && File.Exists(assemblyPath)
            ? context.LoadFromAssemblyPath(assemblyPath)
            : null;
    }

    private void AddServiceProvider(IServiceProvider services)
    {
        ServiceProviders.Add(AddInId, services);
    }

    private IHostBuilder CreateRevitHostBuilder(Action<IServiceCollection> configureServices)
    {
        return new RevitHostBuilder(this, configureServices);
    }

    private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        var assemblyPath = OnAssemblyResolve(new AssemblyName(args.Name));
        return assemblyPath != null && File.Exists(assemblyPath)
            ? Assembly.LoadFrom(assemblyPath)
            : null;
    }
}
