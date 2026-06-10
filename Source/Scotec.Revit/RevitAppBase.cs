// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
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
[JetBrains.Annotations.MeansImplicitUse]
public sealed class RevitApplicationStartupAttribute : Attribute;

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
[JetBrains.Annotations.MeansImplicitUse]
public sealed class RevitApplicationShutdownAttribute : Attribute;

/// <summary>
///     Basic implementation for a Revit app. Derive from this class to use services like logging, dependency injection, or
///     hosted services.
/// </summary>
public abstract class RevitAppBase
{
    /// <summary>
    ///     Holds the root <see cref="IServiceProvider" /> for the current add-in instance.
    /// </summary>
    /// <remarks>
    ///     This property must be <c>static</c> because Revit instantiates types such as
    ///     <see cref="RevitCommand" />, <see cref="RevitCommandAvailability" />, and
    ///     <see cref="RevitEventHandler{TEventArgs}" /> directly, without any knowledge of DI.
    ///     A static entry point is therefore the only way those types can obtain the service provider
    ///     at runtime without requiring Revit to participate in object construction.
    /// </remarks>
    private static IServiceProvider? ServiceProvider
    {
        get => field;
        set
        {
            if (field is not null && value is not null)
            {
                throw new InvalidOperationException("ServiceProvider is already set. Only one instance of a Revit application is allowed per Assembly Load Context.");
            }
            field = value;
        }
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
    ///     <see cref="StandardLifecycleApplicationSignature" /> directly.
    /// </summary>
    protected abstract Type[] StandardLifecycleApplicationSignature { get; }

    /// <summary>
    ///     Returns the stop type used when searching for a standard lifecycle method override
    ///     specific to the concrete app class. Overridden by <see cref="RevitApp" /> and
    ///     <see cref="RevitDbApp" /> to return their own type, ensuring that only methods
    ///     overridden in user-derived classes are discovered.
    /// </summary>
    protected abstract Type LifecycleStopType { get; }


    /// <summary>
    ///     Gets the unique identifier for the Revit add-in.
    /// </summary>
    /// <remarks>
    ///     This property is used to identify the add-in within the Revit environment.
    ///     It is set during the startup process and cannot be modified afterward.
    /// </remarks>
    protected Guid AddInId { get; private set; }

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
    ///     or declare a custom <c>OnShutdown</c> method marked with <see cref="RevitApplicationShutdownAttribute" /> with
    ///     DI-resolved parameters, which the framework will discover and invoke automatically.
    /// </remarks>
    [Obsolete("Override OnShutdown(ControlledApplication application) instead, or declare a method marked with [RevitShutdown] with DI-resolved parameters.")]
    protected virtual bool OnShutdown()
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
    ///     or declare a custom <c>OnStartup</c> method marked with <see cref="RevitApplicationStartupAttribute" /> with
    ///     DI-resolved parameters, which the framework will discover and invoke automatically.
    /// </remarks>
    [Obsolete("Override OnStartup(ControlledApplication application) instead, or declare a method marked with [RevitStartup] with DI-resolved parameters.")]
    protected virtual bool OnStartup()
    {
        return true;
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
        AddInId = addInId.GetGUID();

        try
        {
            var builder = CreateRevitHostBuilder();
            OnConfigure(builder);

            Host = builder.Build();
            Host.Start();

            ServiceProvider = Host.Services;
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

            ServiceProvider = null;
            Host?.StopAsync().GetAwaiter().GetResult();
            Host?.Dispose();
            Host = null;

            return result;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    ///     Returns the root <see cref="IServiceProvider" /> for the current add-in.
    /// </summary>
    /// <returns>
    ///     The <see cref="IServiceProvider" /> set during <see cref="StartupCore" />, or <c>null</c> if the
    ///     add-in has not started or has already shut down.
    /// </returns>
    internal static IServiceProvider GetServiceProvider()
    {
        return ServiceProvider ?? throw new InvalidOperationException("The service provider is not available. Ensure the add-in has started successfully.");
    }

    /// <summary>
    ///     Dispatches the startup call. Resolution priority:
    ///     <list type="number">
    ///         <item>A method marked with <see cref="RevitApplicationStartupAttribute" /> — all parameters resolved from DI.</item>
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
    ///         <item>A method marked with <see cref="RevitApplicationShutdownAttribute" /> — all parameters resolved from DI.</item>
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
    ///             A method decorated with <see cref="RevitApplicationStartupAttribute" /> or <see cref="RevitApplicationShutdownAttribute" />.
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
            ? typeof(RevitApplicationStartupAttribute)
            : typeof(RevitApplicationShutdownAttribute);

        // Priority 1: method explicitly marked with [RevitStartup] / [RevitShutdown].
        var attributedMethod = RevitReflectionHelper.FindMethod(
            GetType(), typeof(RevitAppBase), methodName, typeof(bool),
            m => m.IsDefined(entryPointAttribute, false));

        if (attributedMethod is not null)
        {
            return (bool)RevitReflectionHelper.Invoke(this, attributedMethod, services)!;
        }

        // Priority 2a: class-specific standard overload (e.g. OnStartup(UIControlledApplication in RevitApp or
        // OnStartup(ControlledApplication) in RevitDbApp).
        // Only invoked when the method is overridden below RevitApp / RevitDbApp, not when it is still
        // the default implementation provided by those framework base classes.
        var appSpecificMethod = RevitReflectionHelper.FindMethod(
            GetType(), LifecycleStopType, methodName, typeof(bool),
            m => m.GetParameters()
                  .Select(p => p.ParameterType)
                  .SequenceEqual(StandardLifecycleApplicationSignature));

        if (appSpecificMethod is not null)
        {
            return (bool)RevitReflectionHelper.Invoke(this, appSpecificMethod, services)!;
        }

        // Priority 3: obsolete parameter-less fallback.
#pragma warning disable CS0618
        return methodName == "OnStartup" ? OnStartup() : OnShutdown();
#pragma warning restore CS0618
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
}
