// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit;

/// <summary>
///     Provides shared reflection helpers used by <see cref="RevitCommand"/> and <see cref="RevitCommandAvailability"/>
///     to locate and invoke methods whose parameters are resolved from a DI container.
/// </summary>
internal static class RevitReflectionHelper
{
    /// <summary>
    ///     Walks the type hierarchy from <paramref name="concreteType"/> up to (but not including)
    ///     <paramref name="stopType"/>, returning the first method named <paramref name="name"/> whose return type
    ///     matches <paramref name="returnType"/> and that satisfies the optional <paramref name="predicate"/>.
    /// </summary>
    /// <param name="concreteType">The concrete type to start the search from.</param>
    /// <param name="stopType">The base type at which to stop (exclusive).</param>
    /// <param name="name">The method name to search for.</param>
    /// <param name="returnType">The required return type of the method.</param>
    /// <param name="predicate">An optional additional filter applied to each candidate method.</param>
    /// <returns>The matching <see cref="MethodInfo"/>, or <c>null</c> if none is found.</returns>
    internal static MethodInfo? FindMethod(Type concreteType, Type stopType, string name, Type returnType,
                                           Func<MethodInfo, bool>? predicate = null)
    {
        var type = concreteType;
        while (type != null && type != stopType)
        {
            var method = type
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .FirstOrDefault(m =>
                    m.Name == name &&
                    m.ReturnType == returnType &&
                    (predicate == null || predicate(m)));

            if (method != null)
            {
                return method;
            }

            type = type.BaseType;
        }

        return null;
    }

    /// <summary>
    ///     Resolves the parameters of <paramref name="method"/> from <paramref name="serviceProvider"/>,
    ///     substituting any type present in <paramref name="passthroughs"/> with its corresponding instance directly
    ///     instead of resolving it from the container.
    ///     Invokes the method on <paramref name="instance"/> and returns the raw return value.
    /// </summary>
    /// <param name="instance">The object on which to invoke the method.</param>
    /// <param name="method">The method to invoke.</param>
    /// <param name="serviceProvider">The scoped <see cref="IServiceProvider"/> used to resolve parameters.</param>
    /// <param name="passthroughs">
    ///     An optional map of parameter types to instances that should be passed directly rather than resolved from DI.
    /// </param>
    /// <returns>The raw return value of the invoked method.</returns>
    internal static object? Invoke(object instance, MethodInfo method, IServiceProvider serviceProvider,
                                   IReadOnlyDictionary<Type, object>? passthroughs = null)
    {
        var resolvedParameters = method.GetParameters()
            .Select(p => passthroughs != null && passthroughs.TryGetValue(p.ParameterType, out var passthrough)
                ? passthrough
                : serviceProvider.GetRequiredService(p.ParameterType))
            .ToArray();

        return method.Invoke(instance, resolvedParameters);
    }
}
