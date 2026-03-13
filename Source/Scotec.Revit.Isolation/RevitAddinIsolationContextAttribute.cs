// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Runtime.Loader;

namespace Scotec.Revit.Isolation;

/// <summary>
///     An attribute used to define and provide metadata about the isolation context of a Revit add-in assembly.
/// </summary>
/// <remarks>
///     This attribute is intended to mark an assembly as a Revit add-in and to specify additional
///     context-related information, such as the <see cref="ContextName" /> and <see cref="SharedContextName" />.
///     <br /><br />
///     <b>Usage:</b> Apply this attribute at the assembly level to associate the assembly with a specific
///     <see cref="AssemblyLoadContext" /> in the Revit environment.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class RevitAddinIsolationContextAttribute : Attribute
{
    /// <summary>
    ///     Gets or sets the name of the context associated with the Revit add-in assembly.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the context name. This value provides additional
    ///     metadata or categorization for the assembly.
    /// </value>
    /// <remarks>
    ///     The context name is typically used to identify or group assemblies with similar functionality
    ///     or purpose within the scope of Revit add-ins.
    /// </remarks>
    public string? ContextName { get; set; }

    /// <summary>
    ///     Gets or sets the name of the shared context associated with the Revit add-in assembly.
    /// </summary>
    /// <remarks>
    ///     This property provides a mechanism to specify a shared context name that can be used to
    ///     group or identify related Revit add-in assemblies. It is particularly useful in scenarios
    ///     where multiple assemblies need to share a common context or metadata.
    /// </remarks>
    public string? SharedContextName { get; set; }
}
