using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Autodesk.Revit.UI;
using Microsoft.Extensions.Hosting;

namespace Scotec.Revit;

public abstract class RevitApp : IExternalApplication
{
    private static readonly Dictionary<Guid, IServiceProvider> ServiceProviders = new();
    public UIControlledApplication Application { get; internal set; }
    internal IHost Host { get; set; }

    public Result OnStartup(UIControlledApplication application)
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;

        Application = application;

        try
        {
            var builder = CreateRevitHostBuilder();

            Configure(builder);
            Host = builder.Build();
            Host.Start();

            AddServiceProvider(Host.Services);
            OnStartup();
        }
        catch (Exception)
        {
            return Result.Failed;
        }

        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application)
    {
        try
        {
            OnShutdown();

            ServiceProviders.Remove(application.ActiveAddInId.GetGUID());
            Host.StopAsync().GetAwaiter().GetResult();
            Host?.Dispose();
            Host = null;

            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;
        }
        catch (Exception)
        {
            return Result.Failed;
        }

        return Result.Succeeded;
    }

    protected abstract IHostBuilder Configure(IHostBuilder builder);
    protected abstract Result OnShutdown();

    protected abstract Result OnStartup();

    internal void AddServiceProvider(IServiceProvider services)
    {
        ServiceProviders.Add(Application.ActiveAddInId.GetGUID(), services);
    }

    internal static IServiceProvider GetServiceProvider(Guid addinId)
    {
        return ServiceProviders[addinId];
    }

    private IHostBuilder CreateRevitHostBuilder()
    {
        return new RevitHostBuilder(this);
    }

    private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
    {
        // The configuration of AssemblyMappingFolder in PackageContents.xml does not work at all. Thus we use an assembly resolver here.
        var currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var assemblyName = new AssemblyName(args.Name);

        var assemblyPath = Path.Combine(currentPath!, assemblyName.Name + ".dll");
        return File.Exists(assemblyPath)
            ? Assembly.LoadFrom(assemblyPath)
            : null;
    }
}
