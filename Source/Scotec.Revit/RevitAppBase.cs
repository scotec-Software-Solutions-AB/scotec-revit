// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Microsoft.Extensions.Hosting;

namespace Scotec.Revit;

/// <summary>
///     Marks a method as the startup entry point for a Revit add-in.
/// </summary>
/// <remarks>
///     Apply this attribute to a <c>bool</c>-returning method named <c>OnStartup</c> in a class that derives from
///     <see cref="RevitApp" /> or <see cref="RevitDbApp" /> to indicate that the framework should invoke it during
///     add-in startup. The method's parameters are resolved from the DI container, allowing services registered in
///     <see cref="RevitAppBase.OnConfigure" /> to be injected directly.
/// </remarks>
/// <example>
///     <code>
/// [RevitStartup]
/// protected bool OnStartup(IMyService myService, ILogger&lt;MyApp&gt; logger)
/// {
///     logger.LogInformation("Starting add-in.");
///     myService.Initialize();
///     return true;
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class RevitStartupAttribute : Attribute;

/// <summary>
///     Marks a method as the shutdown entry point for a Revit add-in.
/// </summary>
/// <remarks>
///     Apply this attribute to a <c>bool</c>-returning method named <c>OnShutdown</c> in a class that derives from
///     <see cref="RevitApp" /> or <see cref="RevitDbApp" /> to indicate that the framework should invoke it during
///     add-in shutdown. The method's parameters are resolved from the DI container, allowing services registered in
///     <see cref="RevitAppBase.OnConfigure" /> to be injected directly.
/// </remarks>
/// <example>
///     <code>
/// [RevitShutdown]
/// protected bool OnShutdown(IMyService myService, ILogger&lt;MyApp&gt; logger)
/// {
///     logger.LogInformation("Shutting down add-in.");
///     myService.Cleanup();
///     return true;
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class RevitShutdownAttribute : Attribute;

/// <summary>
///     Basic implementation for a Revit app. Derive from this class to use services like logging, dependency injection, or
///     hosted services.
/// </summary>
public abstract class RevitAppBase
{
    private static readonly Type[] StandardOnControlledApplicationSignature = [typeof(ControlledApplication)];

    /// <summary>
    ///     Revit's assembly resolver loads assemblies used by multiple add-ins only once
    ///     shared across add-ins.
    ///     To ensure that each add-in uses its own instance of a service provider, service providers are stored in a
    ///     static dictionary, keyed by the add-in ID.
    ///     These add-in-specific service providers are utilized during the execution of Revit commands.
    ///     However, this behavior does not apply if each plugin runs — as recommended — in its own isolation context.
    ///     In such cases, the Scotec.Revit assembly is loaded separately in each context.
    ///     Nonetheless, it may be a valid use case to run a set of add-ins within the same context, which is well supported by
    ///     this library.
    /// </summary>
    private static readonly Dictionary<Guid, IServiceProvider> ServiceProviders;

    /// <summary>
    ///     Initializes static members of the <see cref="RevitAppBase" /> class.
    /// </summary>
    /// <remarks>
    ///     This static constructor initializes the <see cref="ServiceProviders" /> dictionary,
    ///     ensuring that each Revit add-in has its own dedicated service provider.
    /// </remarks>
    static RevitAppBase()
    {
        ServiceProviders = new Dictionary<Guid, IServiceProvider>();
    }

    /// <summary>
    ///     Gets or sets the host instance used to manage the application's lifetime and services.
    /// </summary>
    /// <remarks>
    ///     This property is intended for internal use and provides access to the host instance
    ///     that facilitates dependency injection, logging, and hosted services.
    /// </remarks>
    protected internal IHost? Host { get; internal set; }

    /// <summary>
    ///     Returns the standard single-parameter signature for <c>OnStartup</c> / <c>OnShutdown</c> overloads
    ///     specific to the concrete app class. Overridden by <see cref="RevitApp" /> to provide
    ///     <c>[typeof(UIControlledApplication)]</c>. <see cref="RevitDbApp" /> uses the base
    ///     <see cref="StandardOnControlledApplicationSignature" /> directly.
    /// </summary>
    protected abstract Type[] StandardLifecycleApplicationSignature { get; }

    /// <summary>
    ///     Returns the stop type used when searching for a standard lifecycle method override
    ///     specific to the concrete app class. Overridden by <see cref="RevitApp" /> and
    ///     <see cref="RevitDbApp" /> to return their own type, ensuring that only methods
    ///     overridden in user-derived classes are discovered.
    /// </summary>
    protected abstract Type StandardLifecycleStopType { get; }

    /// <summary>
    ///     Returns the ID of the add-in.
    /// </summary>
    [Obsolete("This property is deprecated and will be removed in a future version. Please use the 'AddInId' property instead.")]
    protected Guid AddinId => AddInId;

    /// <summary>
    ///     Gets the unique identifier for the Revit add-in.
    /// </summary>
    /// <remarks>
    ///     This property is used to identify the add-in within the Revit environment.
    ///     It is set during the startup process and cannot be modified afterward.
    /// </remarks>
    protected Guid AddInId { get; private set; }

    /// <summary>
    ///     Provides access to the service provider associated with the current add-in.
    /// </summary>
    /// <remarks>
    ///     This property is deprecated and will be removed in a future version.
    ///     Use constructor or method injection via the DI container instead, or resolve services through
    ///     a method marked with <see cref="RevitStartupAttribute" /> or <see cref="RevitShutdownAttribute" />.
    /// </remarks>
    [Obsolete("This property is deprecated and will be removed in a future version. Use DI injection via [RevitStartup] / [RevitShutdown] methods instead.")]
    protected IServiceProvider Services => GetServiceProvider(AddInId);

    /// <summary>
    ///     Returns the location of the add-in.
    /// </summary>
    /// <remarks>
    ///     This method is deprecated and will be removed in a future version.
    ///     Please use the <see cref="GetAddInPath" /> method instead.
    /// </remarks>
    /// <returns>
    ///     A string representing the directory path of the add-in.
    /// </returns>
    /// <seealso cref="GetAddInPath" />
    [Obsolete("This method is deprecated and will be removed in a future version. Please use the 'GetAddInPath' method instead.")]
    public string GetAddinPath()
    {
        return GetAddInPath();
    }

    /// <summary>
    ///     Retrieves the directory path of the current add-in.
    /// </summary>
    /// <remarks>
    ///     This method ensures the correct path is returned by using the assembly
    ///     containing the derived RevitApp instead of relying on
    ///     <see cref="Assembly.GetExecutingAssembly" />.
    /// </remarks>
    /// <returns>
    ///     A string representing the directory path of the current add-in.
    /// </returns>
    public string GetAddInPath()
    {
        // Do not use Assembly.GetExecutingAssembly().Location. This assembly might be used in multiple add-ins but will be loaded into the
        // process only once. Therefore, do not use Assembly.GetExecutingAssembly().Location because this might not return the path of
        // the current add-in. Use GetAssembly().Location instead. This will return the path to the assembly that contains the derived RevitApp.
        return Path.GetDirectoryName(GetAssembly().Location)!;
    }

    /// <summary>
    ///     Allows derived classes to configure services such as logging, dependency injection, or hosted services.
    /// </summary>
    /// <param name="builder">
    ///     An <see cref="IHostBuilder" /> instance used to configure the application's services and behavior.
    /// </param>
    /// <remarks>
    ///     Override this method in a derived class to customize the application's service configuration.
    /// </remarks>
    protected virtual void OnConfigure(IHostBuilder builder)
    {
    }

    /// <summary>
    ///     Gets the assembly from the current type.
    /// </summary>
    /// <returns></returns>
    protected Assembly GetAssembly()
    {
        return GetType().Assembly;
    }

    /// <summary>
    ///     Executes tasks during the shutdown process of the Revit application.
    /// </summary>
    /// <returns>
    ///     A boolean value indicating the success or failure of the shutdown process.
    /// </returns>
    /// <remarks>
    ///     This overload is obsolete. Override <see cref="OnShutdown(ControlledApplication)" /> instead,
    ///     or declare a custom <c>OnShutdown</c> method marked with <see cref="RevitShutdownAttribute" /> with
    ///     DI-resolved parameters, which the framework will discover and invoke automatically.
    /// </remarks>
    [Obsolete("Override OnShutdown(ControlledApplication application) instead, or declare a method marked with [RevitShutdown] with DI-resolved parameters.")]
    protected virtual bool OnShutdown()
    {
        return true;
    }

    /// <summary>
    ///     Executes tasks during the shutdown process of the Revit application.
    /// </summary>
    /// <param name="application">
    ///     The <see cref="ControlledApplication" /> instance provided by Revit.
    /// </param>
    /// <returns>
    ///     A boolean value indicating the success or failure of the shutdown process.
    /// </returns>
    /// <remarks>
    ///     Override this method in a derived class to define custom shutdown behavior with access to the
    ///     Revit application, or declare a custom <c>OnShutdown</c> method marked with
    ///     <see cref="RevitShutdownAttribute" /> with additional DI-resolved parameters.
    /// </remarks>
    protected virtual bool OnShutdown(ControlledApplication application)
    {
        return true;
    }

    /// <summary>
    ///     Executes tasks when Revit starts.
    /// </summary>
    /// <returns>
    ///     A boolean value indicating whether the startup process was successful.
    /// </returns>
    /// <remarks>
    ///     This overload is obsolete. Override <see cref="OnStartup(ControlledApplication)" /> instead,
    ///     or declare a custom <c>OnStartup</c> method marked with <see cref="RevitStartupAttribute" /> with
    ///     DI-resolved parameters, which the framework will discover and invoke automatically.
    /// </remarks>
    [Obsolete("Override OnStartup(ControlledApplication application) instead, or declare a method marked with [RevitStartup] with DI-resolved parameters.")]
    protected virtual bool OnStartup()
    {
        return true;
    }

    /// <summary>
    ///     Executes tasks when Revit starts.
    /// </summary>
    /// <param name="application">
    ///     The <see cref="ControlledApplication" /> instance provided by Revit.
    /// </param>
    /// <returns>
    ///     A boolean value indicating whether the startup process was successful.
    /// </returns>
    /// <remarks>
    ///     Override this method in a derived class to implement custom startup logic with access to the
    ///     Revit application, or declare a custom <c>OnStartup</c> method marked with
    ///     <see cref="RevitStartupAttribute" /> with additional DI-resolved parameters.
    /// </remarks>
    protected virtual bool OnStartup(ControlledApplication application)
    {
        return true;
    }

    /// <summary>
    ///     Resolves an assembly when the runtime fails to locate it.
    /// </summary>
    /// <param name="args">
    ///     The <see cref="ResolveEventArgs" /> containing information about the assembly that failed to load.
    /// </param>
    /// <returns>
    ///     An <see cref="Assembly" /> instance representing the resolved assembly, or <c>null</c> if the assembly could not be
    ///     found.
    /// </returns>
    /// <remarks>
    ///     This method can be overridden in derived classes to implement a custom assembly resolution strategy.
    ///     By default, it attempts to locate the assembly in the same directory as the current add-in.
    /// </remarks>
    protected virtual Assembly? OnAssemblyResolve(ResolveEventArgs args)
    {
        var currentPath = GetAddInPath();
        var assemblyName = new AssemblyName(args.Name);

        var assemblyPath = Path.Combine(currentPath!, assemblyName.Name + ".dll");
        return File.Exists(assemblyPath)
            ? Assembly.LoadFrom(assemblyPath)
            : null;
    }

    /// <summary>
    ///     Resolves and loads an assembly based on the provided assembly name and context.
    /// </summary>
    /// <param name="context">
    ///     The <see cref="AssemblyLoadContext" /> in which the assembly is being resolved.
    /// </param>
    /// <param name="assemblyName">
    ///     The <see cref="AssemblyName" /> of the assembly to resolve.
    /// </param>
    /// <returns>
    ///     The loaded <see cref="Assembly" /> if found; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    ///     This method attempts to locate the assembly in the directory of the current add-in
    ///     and loads it into the specified load context. If the assembly file does not exist
    ///     or cannot be loaded, <c>null</c> is returned.
    /// </remarks>
    protected virtual Assembly? OnAssemblyResolve(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        var currentPath = GetAddInPath();
        var assemblyPath = Path.Combine(currentPath!, assemblyName.Name + ".dll");
        var currentContext = AssemblyLoadContext.GetLoadContext(GetAssembly());

        return currentContext != null && File.Exists(assemblyPath)
            ? currentContext.LoadFromAssemblyPath(assemblyPath)
            : null;
    }

    /// <summary>
    ///     Handles the startup process for the Revit application.
    /// </summary>
    /// <param name="addInId">
    ///     The <see cref="AddInId" /> representing the unique identifier of the add-in.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the startup process completes successfully; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This method initializes the necessary services and configurations for the Revit application.
    ///     It also sets up assembly resolution and starts the host environment.
    /// </remarks>
    /// <exception cref="Exception">
    ///     Thrown if an error occurs during the startup process.
    /// </exception>
    internal bool StartupCore(AddInId addInId)
    {
        var loadContext = AssemblyLoadContext.GetLoadContext(GetAssembly());
        if (loadContext != null)
        {
            loadContext.Resolving += LoadContextOnResolving;
        }

        //AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;

        AddInId = addInId.GetGUID();

        try
        {
            var builder = CreateRevitHostBuilder();
            OnConfigure(builder);

            Host = builder.Build();
            Host.Start();

            AddServiceProvider(Host.Services);
            return InvokeOnStartup(Host.Services);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    ///     Handles the shutdown process for the Revit application.
    /// </summary>
    /// <param name="application">
    ///     The <see cref="Autodesk.Revit.ApplicationServices.ControlledApplication" /> instance representing the Revit
    ///     application.
    /// </param>
    /// <returns>
    ///     A boolean value indicating whether the shutdown process was successful.
    /// </returns>
    /// <remarks>
    ///     This method performs cleanup tasks such as stopping and disposing of the host,
    ///     removing service providers, and detaching event handlers.
    /// </remarks>
    /// <exception cref="System.Exception">
    ///     An exception may be thrown if an error occurs during the shutdown process.
    /// </exception>
    internal bool ShutdownCore(ControlledApplication application)
    {
        try
        {
            var result = InvokeOnShutdown(Host?.Services);

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

    /// <summary>
    ///     Retrieves the <see cref="IServiceProvider" /> associated with the specified Add-In ID.
    /// </summary>
    /// <param name="addInId">The unique identifier of the Add-In for which the service provider is requested.</param>
    /// <returns>The <see cref="IServiceProvider" /> instance associated with the specified Add-In ID.</returns>
    /// <exception cref="KeyNotFoundException">
    ///     Thrown if no service provider is found for the given <paramref name="addInId" />.
    /// </exception>
    internal static IServiceProvider GetServiceProvider(Guid addInId)
    {
        return ServiceProviders[addInId];
    }

    /// <summary>
    ///     Handles the resolution of assemblies within the specified <see cref="AssemblyLoadContext" />.
    /// </summary>
    /// <param name="context">
    ///     The <see cref="AssemblyLoadContext" /> in which the assembly is being resolved.
    /// </param>
    /// <param name="assemblyName">
    ///     The <see cref="AssemblyName" /> of the assembly to resolve.
    /// </param>
    /// <returns>
    ///     The resolved <see cref="Assembly" /> if found; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    ///     This method delegates the resolution logic to the
    ///     <see cref="OnAssemblyResolve(AssemblyLoadContext, AssemblyName)" /> method.
    /// </remarks>
    private Assembly? LoadContextOnResolving(AssemblyLoadContext context, AssemblyName assemblyName)
    {
        return OnAssemblyResolve(context, assemblyName);
    }

    /// <summary>
    ///     Dispatches the startup call. Resolution priority:
    ///     <list type="number">
    ///         <item>A method marked with <see cref="RevitStartupAttribute" /> — all parameters resolved from DI.</item>
    ///         <item><see cref="OnStartup(ControlledApplication)" /> — if overridden in the derived class.</item>
    ///         <item><see cref="OnStartup()" /> — obsolete parameter-less fallback.</item>
    ///     </list>
    /// </summary>
    private bool InvokeOnStartup(IServiceProvider services)
    {
        return InvokeLifecycleMethod("OnStartup", services);
    }

    /// <summary>
    ///     Dispatches the shutdown call. Resolution priority:
    ///     <list type="number">
    ///         <item>A method marked with <see cref="RevitShutdownAttribute" /> — all parameters resolved from DI.</item>
    ///         <item><see cref="OnShutdown(ControlledApplication)" /> — if overridden in the derived class.</item>
    ///         <item><see cref="OnShutdown()" /> — obsolete parameter-less fallback.</item>
    ///     </list>
    /// </summary>
    private bool InvokeOnShutdown(IServiceProvider? services)
    {
#pragma warning disable CS0618
        if (services is null)
        {
            return OnShutdown();
        }
#pragma warning restore CS0618

        return InvokeLifecycleMethod("OnShutdown", services);
    }

    /// <summary>
    ///     Dispatches a lifecycle call (startup or shutdown) using the following priority:
    ///     <list type="number">
    ///         <item>
    ///             A method decorated with <see cref="RevitStartupAttribute" /> or <see cref="RevitShutdownAttribute" />.
    ///             All parameters are resolved from <paramref name="services" />.
    ///         </item>
    ///         <item>
    ///             <c>OnStartup(ControlledApplication)</c> or <c>OnShutdown(ControlledApplication)</c> —
    ///             resolved from <paramref name="services" /> if overridden in the derived class.
    ///         </item>
    ///         <item>The obsolete parameter-less <c>OnStartup()</c> or <c>OnShutdown()</c> as a last resort.</item>
    ///     </list>
    /// </summary>
    private bool InvokeLifecycleMethod(string methodName, IServiceProvider services)
    {
        var entryPointAttribute = methodName == "OnStartup"
            ? typeof(RevitStartupAttribute)
            : typeof(RevitShutdownAttribute);

        // Priority 1: method explicitly marked with [RevitStartup] / [RevitShutdown].
        var attributedMethod = RevitReflectionHelper.FindMethod(
            GetType(), typeof(RevitAppBase), methodName, typeof(bool),
            m => m.IsDefined(entryPointAttribute, false));

        if (attributedMethod is not null)
        {
            return (bool)RevitReflectionHelper.Invoke(this, attributedMethod, services)!;
        }

        // Priority 2a: class-specific standard overload (e.g. OnStartup(UIControlledApplication) in RevitApp).
        // Only invoked when the method is overridden below RevitApp / RevitDbApp, not when it is still
        // the default implementation provided by those framework base classes.
        if (StandardLifecycleApplicationSignature is not null && StandardLifecycleStopType is not null)
        {
            var appSpecificMethod = RevitReflectionHelper.FindMethod(
                GetType(), StandardLifecycleStopType, methodName, typeof(bool),
                m => m.GetParameters()
                      .Select(p => p.ParameterType)
                      .SequenceEqual(StandardLifecycleApplicationSignature));

            if (appSpecificMethod is not null)
            {
                return (bool)RevitReflectionHelper.Invoke(this, appSpecificMethod, services)!;
            }
        }

        // Priority 2b: OnStartup(ControlledApplication) / OnShutdown(ControlledApplication) overridden in the derived class.
        var controlledAppMethod = RevitReflectionHelper.FindMethod(
            GetType(), typeof(RevitAppBase), methodName, typeof(bool),
            m => m.GetParameters()
                  .Select(p => p.ParameterType)
                  .SequenceEqual(StandardOnControlledApplicationSignature));

        if (controlledAppMethod is not null)
        {
            return (bool)RevitReflectionHelper.Invoke(this, controlledAppMethod, services)!;
        }

        // Priority 3: obsolete parameter-less fallback.
#pragma warning disable CS0618
        return methodName == "OnStartup" ? OnStartup() : OnShutdown();
#pragma warning restore CS0618
    }

    /// <summary>
    ///     Adds a service provider to the static dictionary of service providers, associating it with the current add-in's ID.
    /// </summary>
    /// <param name="services">
    ///     The <see cref="IServiceProvider" /> instance to associate with the current add-in.
    /// </param>
    /// <remarks>
    ///     This method ensures that each add-in has its own dedicated service provider, avoiding conflicts caused by shared
    ///     static members when multiple add-ins are loaded in the same Revit process.
    ///     When running each plugin in its own isolation context, the Scotec.Revit assembly is loaded separately for each
    ///     context,
    ///     and this method will not be invoked multiple times for the same assembly. However, it may still be a valid use case
    ///     to
    ///     run a set of add-ins within the same context, which is well supported by this library.
    /// </remarks>
    private void AddServiceProvider(IServiceProvider services)
    {
        ServiceProviders.Add(AddInId, services);
    }

    /// <summary>
    ///     Creates and configures a new instance of <see cref="IHostBuilder" /> for the Revit application.
    /// </summary>
    /// <remarks>
    ///     This method initializes a custom host builder tailored for Revit applications, allowing for the addition
    ///     of services, configuration, and dependency injection.
    /// </remarks>
    /// <returns>
    ///     An instance of <see cref="IHostBuilder" /> configured for the Revit application.
    /// </returns>
    /// <seealso cref="RevitHostBuilder" />
    private IHostBuilder CreateRevitHostBuilder()
    {
        return new RevitHostBuilder(this);
    }

    /// <summary>
    ///     Handles the <see cref="AppDomain.AssemblyResolve" /> event to resolve assemblies that cannot be located by the
    ///     runtime.
    /// </summary>
    /// <param name="sender">
    ///     The source of the event, typically the current <see cref="AppDomain" />.
    /// </param>
    /// <param name="args">
    ///     The <see cref="ResolveEventArgs" /> containing information about the assembly that failed to load.
    /// </param>
    /// <returns>
    ///     An <see cref="Assembly" /> instance representing the resolved assembly, or <c>null</c> if the assembly could not be
    ///     found.
    /// </returns>
    /// <remarks>
    ///     This method delegates the resolution logic to the <see cref="OnAssemblyResolve(ResolveEventArgs)" /> method.
    ///     It is automatically attached to the <see cref="AppDomain.AssemblyResolve" /> event during application startup.
    /// </remarks>
    private Assembly? CurrentDomainOnAssemblyResolve(object? sender, ResolveEventArgs args)
    {
        return OnAssemblyResolve(args);
    }
}
