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
///     (<see cref="IGlobalRevitContext" /> / <see cref="IGlobalRevitUiContext" />) into an
///     <see cref="IServiceCollection" />.
/// </summary>
public static class RevitGlobalContextExtensions
{
    /// <summary>
    ///     Registers <see cref="GlobalRevitContext" /> as <see cref="IGlobalRevitContext" /> in the
    ///     service collection. Use this overload for DB-level add-ins (<see cref="RevitDbApp" />)
    ///     that do not have a <see cref="UIApplication" />.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="application">
    ///     The <see cref="ControlledApplication" /> provided by Revit at add-in startup.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection" /> for chaining.</returns>
    /// <exception cref="ArgumentNullException">
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
    ///     Registers <see cref="GlobalRevitUiContext" /> as both <see cref="IGlobalRevitUiContext" />
    ///     and <see cref="IGlobalRevitContext" /> in the service collection, sharing a single instance.
    ///     Use this overload for UI add-ins (<see cref="RevitApp" />) that have a
    ///     <see cref="UIApplication" />.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="application">
    ///     The <see cref="UIControlledApplication" /> provided by Revit at add-in startup.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection" /> for chaining.</returns>
    /// <exception cref="ArgumentNullException">
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
