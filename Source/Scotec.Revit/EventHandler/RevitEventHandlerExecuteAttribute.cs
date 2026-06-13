// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;

namespace Scotec.Revit.EventHandler;

/// <summary>
///     Marks a method as the execute entry point for a <see cref="RevitEventHandler{TEventArgs}" />.
/// </summary>
/// <remarks>
///     Apply this attribute to a single method in a derived class. The framework discovers and invokes
///     it with parameters resolved from the per-invocation DI scope. If no method is marked, the framework
///     falls back to <see cref="RevitEventHandler{TEventArgs}.OnExecute" />.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class RevitEventHandlerExecuteAttribute : Attribute;
