// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit;

/// <summary>
///     Default implementation of <see cref="IRevitScopeFactory" />.
/// </summary>
/// <remarks>
///     Registered as a scoped service so that Autofac injects the <see cref="ILifetimeScope" />
///     and <see cref="IServiceProvider" /> of the <em>current</em> scope at resolution time.
///     <see cref="CreateScope" /> applies the following decision tree on every call:
///     <list type="number">
///         <item>
///             <strong><see cref="RevitContextTracker.IsActive" /> is <see langword="false" /></strong>
///             (env var is <c>0</c> or absent) — no Revit entry point is executing anywhere in the
///             process. A plain child scope is created with no context objects registered.
///         </item>
///         <item>
///             <strong><see cref="RevitContextTracker.IsActive" /> is <see langword="true" /></strong>
///             and <see cref="IRevitContext" /> is resolvable from the current scope — this add-in's
///             own entry point owns the context and has already registered it. A plain child scope
///             inherits those registrations through the Autofac scope chain.
///         </item>
///         <item>
///             <strong><see cref="RevitContextTracker.IsActive" /> is <see langword="true" /></strong>,
///             <see cref="IRevitContext" /> is <em>not</em> yet resolvable, and
///             <see cref="RevitContextTracker.ThisAddInCommandIsActive" /> is
///             <see langword="true" /> — this add-in's own <see cref="RevitEntryPointKind.Command" />
///             entry point has activated the context tracker but has not yet registered
///             <see cref="IRevitContext" /> in a DI scope (the brief window between
///             <c>RevitContextTracker.Activate(Command)</c> and scope construction in
///             <see cref="RevitCommand" />, <see cref="RevitTask" />, or
///             <see cref="RevitCommandAvailability" />). A plain child scope is created; the
///             entry point will register the context in its own scope immediately after.
///             Handler-kind entry points (RevitEventHandler, RevitUpdater) do not set
///             <see cref="RevitContextTracker.ThisAddInCommandIsActive" />, so they fall
///             through to Branch 4b.
///         </item>
///         <item>
///             <strong>Branch 4a</strong> — <see cref="RevitContextTracker.IsActive" /> is
///             <see langword="true" />, <see cref="IRevitContext" /> is absent,
///             <see cref="RevitContextTracker.ThisAddInCommandIsActive" /> is
///             <see langword="false" />, and
///             <see cref="RevitContextTracker.IsCommandActive" /> is <see langword="true" />
///             — the active entry point is a document-stable
///             <see cref="RevitEntryPointKind.Command" /> belonging to a different add-in.
///             A fresh <see cref="IRevitContext" /> / <see cref="IRevitUiContext" /> is
///             constructed from the application-lifetime <see cref="IGlobalRevitUiContext" />
///             or <see cref="IGlobalRevitContext" /> and registered in the new child scope.
///         </item>
///         <item>
///             <strong>Branch 4b</strong> — <see cref="RevitContextTracker.IsActive" /> is
///             <see langword="true" />, <see cref="IRevitContext" /> is absent,
///             <see cref="RevitContextTracker.ThisAddInCommandIsActive" /> is
///             <see langword="false" />, and
///             <see cref="RevitContextTracker.IsCommandActive" /> is <see langword="false" />
///             — the active entry point is a <see cref="RevitEntryPointKind.Handler" />
///             (event handler or DMU updater) belonging to a different add-in. The active
///             document cannot be assumed to be <c>ActiveUIDocument</c>, so a plain child
///             scope is returned with no context objects.
///         </item>
///     </list>
/// </remarks>
internal sealed class RevitScopeFactory : IRevitScopeFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILifetimeScope _root;

    /// <summary>
    ///     Initializes a new instance of <see cref="RevitScopeFactory" />.
    /// </summary>
    /// <param name="serviceProvider">
    ///     The service provider of the current scope, used to probe for
    ///     <see cref="IRevitContext" /> and to resolve the global context singletons when a
    ///     fresh context is required.
    /// </param>
    /// <param name="root">
    ///     The Autofac lifetime scope of the current resolution context, from which child
    ///     scopes are created. Autofac injects this contextually for every scope.
    /// </param>
    public RevitScopeFactory(IServiceProvider serviceProvider, ILifetimeScope root)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(root);
        _serviceProvider = serviceProvider;
        _root = root;
    }

    /// <inheritdoc />
    public IServiceScope CreateScope(Action<IServiceCollection>? configure = null)
    {
        // ── Branch 1: no active Revit context anywhere in the process ─────────
        // env var == 0 (or absent). No Revit entry point is executing. Open a
        // plain child scope — do not register any IRevitContext / IRevitUiContext.
        if (!RevitContextTracker.IsActive)
        {
            return new RevitServiceScope(OpenScope(configure));
        }

        // ── Branch 2: active context already in this add-in's scope chain ─────
        // env var >= 1 and IRevitContext is resolvable from the current scope.
        // The enclosing entry point (RevitCommand, RevitEventHandler, etc.) owns the
        // context. Open a plain child scope — IRevitContext / IRevitUiContext are
        // inherited automatically through the Autofac scope chain.
        if (_serviceProvider.GetService<IRevitContext>() is not null)
        {
            return new RevitServiceScope(OpenScope(configure));
        }

        // ── Branch 3: this add-in's Command entry point is active but scope not yet built ─
        // ThisAddInCommandIsActive is true, meaning Activate(Command) was called by this
        // add-in's own entry point infrastructure (RevitCommand, RevitTask,
        // RevitCommandAvailability) but its DI scope — and therefore IRevitContext — does
        // not exist yet. This is the brief window between RevitContextTracker.Activate()
        // and CreateLifetimeScope() inside the entry point method. A plain child scope is
        // returned; the entry point will register IRevitContext in its own scope immediately
        // afterwards. Attempting to create a fresh context here would duplicate and conflict
        // with that registration.
        // Handler-kind entry points (RevitEventHandler, RevitUpdater) do not set
        // ThisAddInCommandIsActive, so they fall through to Branch 4b below where a plain
        // scope is returned because the document identity cannot be assumed.
        if (RevitContextTracker.ThisAddInCommandIsActive)
        {
            return new RevitServiceScope(OpenScope(configure));
        }

        // ── Branch 4a: Command-kind entry point active in another add-in ─────────────────
        // env var >= 1, IRevitContext is absent, this add-in has no active entry point, and
        // IsCommandActive is true. The active entry point is a document-stable command or
        // external event belonging to a different add-in. ActiveUIDocument is stable and
        // consistent with the Revit API threading model, so it is safe to construct a fresh
        // context from the application-lifetime global and register it in the new child scope.
        if (RevitContextTracker.IsCommandActive)
        {
            // Branch 4a — UI add-in path.
            var uiGlobalContext = _serviceProvider.GetService<IGlobalRevitUiContext>();
            if (uiGlobalContext is not null)
            {
                var context = new RevitUiContext(uiGlobalContext.UiApplication);
                var lifetimeScope = _root.BeginLifetimeScope(builder =>
                {
                    builder.RegisterInstance(context).As<IRevitContext>().OwnedByLifetimeScope();
                    // Same instance — ExternallyOwned prevents a second Dispose call.
                    builder.RegisterInstance(context).As<IRevitUiContext>().ExternallyOwned();
                    PopulateServices(builder, configure);
                });
                return new RevitServiceScope(lifetimeScope);
            }

            // Branch 4a — DB add-in path.
            var globalContext = _serviceProvider.GetRequiredService<IGlobalRevitContext>();
            var dbContext = new RevitContext(globalContext.Application);
            var dbLifetimeScope = _root.BeginLifetimeScope(builder =>
            {
                builder.RegisterInstance(dbContext).As<IRevitContext>().OwnedByLifetimeScope();
                PopulateServices(builder, configure);
            });
            return new RevitServiceScope(dbLifetimeScope);
        }

        // ── Branch 4b: Handler-kind entry point active in another add-in ──────────────────
        // env var >= 1, IRevitContext is absent, this add-in has no active entry point, and
        // IsCommandActive is false. The active entry point is a Revit event handler or DMU
        // updater belonging to a different add-in. The active document is not necessarily
        // ActiveUIDocument, so constructing a context from global state could silently
        // capture the wrong document. Return a plain child scope with no context objects.
        return new RevitServiceScope(OpenScope(configure));
    }

    /// <summary>
    ///     Explicit implementation of <see cref="IServiceScopeFactory.CreateScope" /> so that
    ///     <see cref="IRevitScopeFactory" /> is a drop-in replacement for
    ///     <see cref="IServiceScopeFactory" /> — delegates to
    ///     <see cref="CreateScope(Action{IServiceCollection}?)" /> with no additional service
    ///     registrations.
    /// </summary>
    IServiceScope IServiceScopeFactory.CreateScope() => CreateScope(null);

    /// <summary>
    ///     Opens a plain child scope, optionally populated with extra services.
    ///     Used for Branch 1 and Branch 2 where no context objects need to be registered.
    /// </summary>
    private ILifetimeScope OpenScope(Action<IServiceCollection>? configure)
        => configure is null
            ? _root.BeginLifetimeScope()
            : _root.BeginLifetimeScope(builder => PopulateServices(builder, configure));

    private void PopulateServices(ContainerBuilder builder, Action<IServiceCollection>? configure)
    {
        if (configure is null)
        {
            return;
        }

        var services = new ServiceCollection();
        configure(services);
        builder.PopulateRevit(services, _root);
    }

    /// <summary>
    ///     Thin <see cref="IServiceScope" /> wrapper around an Autofac <see cref="ILifetimeScope" />.
    ///     <see cref="ServiceProvider" /> is obtained via <see cref="ILifetimeScope.Resolve{T}" />,
    ///     which returns the Autofac-backed <see cref="IServiceProvider" /> already registered
    ///     for every child scope. Disposing this wrapper disposes the underlying lifetime scope.
    /// </summary>
    private sealed class RevitServiceScope : IServiceScope
    {
        private readonly ILifetimeScope _lifetimeScope;

        internal RevitServiceScope(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
            ServiceProvider = lifetimeScope.Resolve<IServiceProvider>();
        }

        public IServiceProvider ServiceProvider { get; }

        public void Dispose() => _lifetimeScope.Dispose();
    }
}
