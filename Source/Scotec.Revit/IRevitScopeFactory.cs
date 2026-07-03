// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit;

/// <summary>
///     Creates a child <see cref="IServiceScope" /> from the current DI scope, registering
///     <see cref="IRevitContext" /> and <see cref="IRevitUiContext" /> only when necessary.
/// </summary>
/// <remarks>
///     <para>
///         <see cref="CreateScope" /> applies the following decision tree on every call:
///     </para>
///     <list type="number">
///         <item>
///             <strong><see cref="RevitContextTracker.IsActive" /> is <see langword="false" /></strong>
///             (env var is <c>0</c> or absent) — no Revit entry point is executing anywhere in
///             the process. A plain child scope is returned with no context objects.
///         </item>
///         <item>
///             <strong><see cref="RevitContextTracker.IsActive" /> is <see langword="true" /></strong>
///             and <see cref="IRevitContext" /> is already resolvable from the current scope —
///             this add-in's own entry point owns the context. A plain child scope is returned;
///             <see cref="IRevitContext" /> / <see cref="IRevitUiContext" /> are inherited
///             through the Autofac scope chain automatically.
///         </item>
///         <item>
///             <strong><see cref="RevitContextTracker.IsActive" /> is <see langword="true" /></strong>
///             but <see cref="IRevitContext" /> is <em>not</em> resolvable from the current scope —
///             the active entry point belongs to a different add-in. A fresh context is constructed
///             from the application-lifetime <see cref="IGlobalRevitUiContext" /> (UI add-in) or
///             <see cref="IGlobalRevitContext" /> (DB add-in) and registered in the new child scope.
///         </item>
///     </list>
///     <para>
///         This factory is registered automatically by <see cref="RevitApp" /> and
///         <see cref="RevitDbApp" />. No manual call to <c>AddRevitScopeFactory()</c> is required.
///         The returned <see cref="IServiceScope" /> is owned by the caller and must be disposed
///         when the operation is complete.
///     </para>
/// </remarks>
[PublicAPI]
public interface IRevitScopeFactory : IServiceScopeFactory
{
    /// <summary>
    ///     Creates a new child scope, registering Revit context objects as described in the
    ///     class-level remarks.
    /// </summary>
    /// <param name="configure">
    ///     An optional callback to register additional services into the scope before it is
    ///     returned. Pass <see langword="null" /> when no extra registrations are needed.
    /// </param>
    /// <returns>
    ///     A new <see cref="IServiceScope" />. The caller is responsible for disposing the scope.
    /// </returns>
    new IServiceScope CreateScope(Action<IServiceCollection>? configure = null);
}
