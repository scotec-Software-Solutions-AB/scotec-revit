// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;

namespace Scotec.Revit.Isolation;

/// <summary>
///     Marks an assembly as a Revit add-in assembly.
/// </summary>
/// <remarks>
///     Assemblies decorated with this attribute are identified as containing components or functionality
///     specific to Revit add-ins. This attribute can be used to provide additional context or metadata
///     for the assembly, such as the <see cref="ContextName" /> property.
///     <br /><br />
///     <b>Usage:</b> This attribute should be applied at the assembly level.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class RevitAddinAssemblyAttribute : Attribute
{
    /// <summary>
    ///     Gets or setsvthe name of the context associated with the Revit add-in assembly.
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
}
