// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Runtime.Loader;

namespace Scotec.Revit.Isolation;

/// <summary>
///     Represents an attribute used to define a shared <see cref="AssemblyLoadContext" /> within the Revit environment.
/// </summary>
/// <remarks>
///     This attribute is intended to be applied to elements that require a specific shared context
///     to be identified and utilized during execution. The context name is provided during initialization.
/// </remarks>
public class RevitSharedIsolationContextAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RevitSharedIsolationContextAttribute" /> class with the specified context name.
    /// </summary>
    /// <param name="contextName">
    ///     The name of the shared <see cref="AssemblyLoadContext" /> to be used within the Revit environment.
    /// </param>
    /// <remarks>
    ///     The <paramref name="contextName" /> parameter identifies the specific shared <see cref="AssemblyLoadContext" />
    ///     required for execution. This ensures proper isolation and context management.
    /// </remarks>
    public RevitSharedIsolationContextAttribute(string contextName)
    {
        ContextName = contextName;
    }

    /// <summary>
    ///     Gets the name of the shared context associated with this attribute.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the name of the shared <see cref="AssemblyLoadContext" />
    ///     used within the Revit environment.
    /// </value>
    /// <remarks>
    ///     The context name is used to identify and manage the specific shared <see cref="AssemblyLoadContext" />
    ///     required for proper isolation and execution.
    /// </remarks>
    private string ContextName { get; }
}
