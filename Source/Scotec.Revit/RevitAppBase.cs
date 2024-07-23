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
    [Obsolete("This property has been marked as deprecated and will be removed in a future version. Use the 'AddInId' property instead. ")]
    protected Guid AddinId => AddInId;

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
    [Obsolete("This method has been marked as deprecated and will be removed in a future version. Use the 'GetAddInPath' method instead. ")]
    public string GetAddinPath()
    {
        return GetAddInPath();
    }

    /// <summary>
    ///     Returns the location of the add-in.
    /// </summary>
    public string GetAddInPath()
    {
        // Do not use Assembly.GetExecutingAssembly().Location. This assembly might be used in multiple add-ins but will be loaded into the
        // process only once. Therefore, do not use Assembly.GetExecutingAssembly().Location because this might not return the path of
        // the current add-in. Use GetAssembly().Location instead. This will return the path to the assembly that contains the derived RevitApp.
        return Path.GetDirectoryName(GetAssembly().Location);
    }

    /// <summary>
    ///     Can be overridden to add or configure services such as logging or hosted services.
    /// </summary>
    protected virtual void OnConfigure(IHostBuilder builder)
    {
    }

    protected Assembly GetAssembly()
    {
        return GetType().Assembly;
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
    protected virtual Assembly OnAssemblyResolve(ResolveEventArgs args)
    {
        var currentPath = GetAddInPath();
        var assemblyName = new AssemblyName(args.Name);

        var assemblyPath = Path.Combine(currentPath!, assemblyName.Name + ".dll");
        return File.Exists(assemblyPath)
            ? Assembly.LoadFrom(assemblyPath)
            : null;
    }

    /// <summary>
    ///     Can be overridden to implement a custom assembly resolver.
    /// </summary>
    /// <returns>Returns the loaded assembly or null if the assembly could not be loaded.</returns>
    /// <remarks>Loads the assembly into the current load context.</remarks>
    protected virtual Assembly OnAssemblyResolve(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        var currentPath = GetAddInPath();
        var assemblyPath = Path.Combine(currentPath!, assemblyName.Name + ".dll");
        var currentContext = AssemblyLoadContext.GetLoadContext(GetAssembly());

        return currentContext != null && File.Exists(assemblyPath)
            ? currentContext.LoadFromAssemblyPath(assemblyPath)
            : null;
    }

    /// <summary>
    /// </summary>
    protected bool OnStartup(AddInId addInId)
    {
        var loadContext = AssemblyLoadContext.GetLoadContext(GetAssembly());
        if (loadContext != null)
        {
            loadContext.Resolving += LoadContextOnResolving;
        }

        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;

        AddInId = addInId.GetGUID();

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

            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;

            var loadContext = AssemblyLoadContext.GetLoadContext(GetAssembly());
            if (loadContext != null)
            {
                loadContext.Resolving -= LoadContextOnResolving;
            }

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

    private Assembly LoadContextOnResolving(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        return OnAssemblyResolve(context, assemblyName);
    }

    private void AddServiceProvider(IServiceProvider services)
    {
        ServiceProviders.Add(AddInId, services);
    }

    private IHostBuilder CreateRevitHostBuilder()
    {
        return new RevitHostBuilder(this);
    }

    private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        return OnAssemblyResolve(args);
    }
}
