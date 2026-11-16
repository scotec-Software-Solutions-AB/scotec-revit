// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace Scotec.Revit;

/// <summary>
///     Marks a singleton service that must be instantiated during add-in startup instead of on first resolution.
/// </summary>
/// <remarks>
///     Some services (for example <see cref="RevitTaskDispatcher" />, which creates an <c>ExternalEvent</c>) can only be created
///     inside a valid Revit API context. Registering them with a factory delegate lets them receive their dependencies from the
///     container, and this marker makes <see cref="RevitAppBase" /> resolve them right after the host has been built, while
///     the add-in is still starting up.
/// </remarks>
internal sealed record EagerSingletonRegistration(Type ServiceType)
{
    /// <summary>
    ///     Registers <typeparamref name="TService" /> as a singleton created by <paramref name="factory" />, and marks it to be
    ///     instantiated during add-in startup. Does nothing if a service of the same type is already registered.
    /// </summary>
    internal static IServiceCollection TryAddEagerSingleton<TService>(
        IServiceCollection services,
        Func<IServiceProvider, TService> factory)
        where TService : class
    {
        if (services.Any(descriptor => descriptor.ServiceType == typeof(TService)))
        {
            return services;
        }

        services.AddSingleton(factory);
        services.AddSingleton(new EagerSingletonRegistration(typeof(TService)));

        return services;
    }

    /// <summary>
    ///     Resolves all services marked as eager singletons so that they are created in the current (startup) context.
    /// </summary>
    internal static void Initialize(IServiceProvider serviceProvider)
    {
        foreach (var registration in serviceProvider.GetServices<EagerSingletonRegistration>())
        {
            serviceProvider.GetRequiredService(registration.ServiceType);
        }
    }
}
