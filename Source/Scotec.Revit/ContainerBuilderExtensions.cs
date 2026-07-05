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
    ///     Populates the container builder from <paramref name="services" /> and, when
    ///     <see cref="IRevitScopeFactory" /> is registered in <paramref name="parentContext" />,
    ///     overrides the <see cref="IServiceScopeFactory" /> registration so that it resolves to
    ///     <see cref="IRevitScopeFactory" /> rather than Autofac's built-in <c>AutofacServiceScopeFactory</c>.
    /// </summary>
    /// <remarks>
    ///     Autofac's <c>Populate</c> extension unconditionally re-registers <c>AutofacServiceScopeFactory</c>
    ///     as <see cref="IServiceScopeFactory" /> inside every lifetime scope it is called in. Because Autofac
    ///     resolves the last registration for a given service type, registering the forwarding rule immediately
    ///     after <c>Populate</c> ensures that <see cref="IServiceScopeFactory" /> always resolves to
    ///     <see cref="RevitScopeFactory" /> in that scope — making the Revit-aware factory transparent to
    ///     callers that inject the standard interface.
    ///     When <paramref name="parentContext" /> is <see langword="null" /> or
    ///     <see cref="IRevitScopeFactory" /> is not registered in it, the method behaves identically to
    ///     a plain <c>Populate</c> call and no forwarding rule is installed.
    ///     The root container receives the same forwarding rule via
    ///     <see cref="RegisterRevitScopeFactoryForwarding" /> called from <c>RevitHostBuilder</c>
    ///     when the add-in has opted in to <see cref="IRevitScopeFactory" />.
    /// </remarks>
    /// <param name="builder">The <see cref="ContainerBuilder" /> to populate.</param>
    /// <param name="services">The <see cref="IServiceCollection" /> whose descriptors are added to the builder.</param>
    /// <param name="parentContext">
    ///     The Autofac component context of the parent scope, used to determine whether
    ///     <see cref="IRevitScopeFactory" /> is registered. Pass <see langword="null" /> to skip the
    ///     forwarding rule unconditionally.
    /// </param>
    internal static void PopulateRevit(this ContainerBuilder builder,
                                       IServiceCollection services,
                                       IComponentContext? parentContext = null)
    {
        builder.Populate(services);

        // Only install the forwarding rule when IRevitScopeFactory is registered in the
        // parent scope. If the add-in has not opted in to the scope factory, Populate's
        // default AutofacServiceScopeFactory registration is left in place.
        if (parentContext?.IsRegistered<IRevitScopeFactory>() == true)
        {
            builder.RegisterRevitScopeFactoryForwarding();
        }
    }

    /// <summary>
    ///     Registers the <see cref="IServiceScopeFactory" /> → <see cref="IRevitScopeFactory" /> forwarding
    ///     rule on <paramref name="builder" />.
    /// </summary>
    /// <remarks>
    ///     Autofac resolves the <em>last</em> registration for a given service type, so this rule — registered
    ///     after <c>Populate</c> has already added <c>AutofacServiceScopeFactory</c> — ensures that
    ///     <see cref="IServiceScopeFactory" /> always resolves to <see cref="IRevitScopeFactory" /> in the
    ///     target scope.
    ///     Called from <see cref="PopulateRevit" /> for child scopes and from <c>RevitHostBuilder</c> for
    ///     the root container.
    /// </remarks>
    /// <param name="builder">The <see cref="ContainerBuilder" /> on which to register the rule.</param>
    internal static void RegisterRevitScopeFactoryForwarding(this ContainerBuilder builder)
    {
        builder.Register(ctx => ctx.Resolve<IRevitScopeFactory>())
               .As<IServiceScopeFactory>()
               .InstancePerLifetimeScope();
    }
}
