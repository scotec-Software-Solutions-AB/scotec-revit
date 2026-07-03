// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit;

/// <summary>
///     Extension methods for <see cref="ContainerBuilder" /> used internally by the Scotec.Revit framework.
/// </summary>
internal static class ContainerBuilderExtensions
{
    /// <summary>
    ///     Populates the container builder from <paramref name="services" /> and immediately overrides the
    ///     <see cref="IServiceScopeFactory" /> registration so that it resolves to
    ///     <see cref="IRevitScopeFactory" /> rather than Autofac's built-in <c>AutofacServiceScopeFactory</c>.
    /// </summary>
    /// <remarks>
    ///     Autofac's <c>Populate</c> extension unconditionally re-registers <c>AutofacServiceScopeFactory</c>
    ///     as <see cref="IServiceScopeFactory" /> inside every lifetime scope it is called in. Because Autofac
    ///     resolves the last registration for a given service type, registering the forwarding rule immediately
    ///     after <c>Populate</c> ensures that <see cref="IServiceScopeFactory" /> always resolves to
    ///     <see cref="RevitScopeFactory" /> in that scope — making the Revit-aware factory transparent to
    ///     callers that inject the standard interface.
    /// </remarks>
    /// <param name="builder">The <see cref="ContainerBuilder" /> to populate.</param>
    /// <param name="services">The <see cref="IServiceCollection" /> whose descriptors are added to the builder.</param>
    internal static void PopulateRevit(this ContainerBuilder builder, IServiceCollection services)
    {
        builder.Populate(services);

        // Override the IServiceScopeFactory that Populate installs with a forwarding rule to
        // IRevitScopeFactory. Autofac uses last-registration-wins for default service resolution,
        // so this registration takes precedence over AutofacServiceScopeFactory in this scope.
        builder.Register(ctx => ctx.Resolve<IRevitScopeFactory>())
               .As<IServiceScopeFactory>()
               .InstancePerLifetimeScope();
    }
}
