// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit;

/// <summary>
///     Extension methods for registering the application-lifetime Revit context singletons
///     (<see cref="Scotec.Revit.IGlobalRevitContext" /> / <see cref="Scotec.Revit.IGlobalRevitUiContext" />) into an
///     <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
/// </summary>
public static class RevitGlobalContextServiceExtensions
{
    /// <summary>
    ///     Registers <see cref="Scotec.Revit.GlobalRevitContext" /> as <see cref="Scotec.Revit.IGlobalRevitContext" /> in the
    ///     service collection. Use this overload for DB-level add-ins (<see cref="Scotec.Revit.RevitDbApp" />)
    ///     that do not have a <see cref="Autodesk.Revit.UI.UIApplication" />.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="application">
    ///     The <see cref="Autodesk.Revit.ApplicationServices.ControlledApplication" /> provided by Revit at add-in startup.
    /// </param>
    /// <returns>The same <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" /> for chaining.</returns>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown when <paramref name="services" /> or <paramref name="application" /> is <c>null</c>.
    /// </exception>
    public static IServiceCollection AddGlobalRevitContext(
        this IServiceCollection services,
        ControlledApplication application)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(application);

        var context = new GlobalRevitContext(application);
        services.AddSingleton<IGlobalRevitContext>(context);

        return services;
    }

    /// <summary>
    ///     Registers <see cref="Scotec.Revit.GlobalRevitUiContext" /> as both <see cref="Scotec.Revit.IGlobalRevitUiContext" />
    ///     and <see cref="Scotec.Revit.IGlobalRevitContext" /> in the service collection, sharing a single instance.
    ///     Use this overload for UI add-ins (<see cref="Scotec.Revit.RevitApp" />) that have a
    ///     <see cref="Autodesk.Revit.UI.UIApplication" />.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="application">
    ///     The <see cref="Autodesk.Revit.UI.UIControlledApplication" /> provided by Revit at add-in startup.
    /// </param>
    /// <returns>The same <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" /> for chaining.</returns>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown when <paramref name="services" /> or <paramref name="application" /> is <c>null</c>.
    /// </exception>
    public static IServiceCollection AddGlobalRevitContext(
        this IServiceCollection services,
        UIControlledApplication application)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(application);

        var context = new GlobalRevitUiContext(application);
        services.AddSingleton<IGlobalRevitUiContext>(context);
        services.AddSingleton<IGlobalRevitContext>(context);

        return services;
    }
}

/// <summary>
///     Extension methods for registering <see cref="Scotec.Revit.RevitTask" /> into an
///     <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" />.
/// </summary>
public static class RevitTaskServiceExtensions
{
    /// <summary>
    ///     Registers a new <see cref="Scotec.Revit.RevitTask" /> instance as a singleton in the service collection.
    /// </summary>
    /// <remarks>
    ///     <see cref="Scotec.Revit.RevitTask" /> wraps a Revit <c>ExternalEvent</c> and marshals work onto the Revit
    ///     API thread. Because <c>ExternalEvent</c> must be created while Revit is idle and is tied to
    ///     the add-in lifetime, the instance is constructed eagerly and registered as a singleton.
    ///     <para>
    ///         Dispose the registered <see cref="Scotec.Revit.RevitTask" /> during add-in shutdown to release the
    ///         underlying <c>ExternalEvent</c>.
    ///     </para>
    /// </remarks>
    /// <param name="services">The service collection to register into.</param>
    /// <returns>The same <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection" /> for chaining.</returns>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown when <paramref name="services" /> is <c>null</c>.
    /// </exception>
    public static IServiceCollection AddRevitTask(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddSingleton(new RevitTask());
    }
}