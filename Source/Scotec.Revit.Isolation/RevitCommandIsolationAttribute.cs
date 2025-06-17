// // Copyright © 2023 - 2025 Olaf Meyer
// // Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.UI;

namespace Scotec.Revit.Isolation;

/// <summary>
///     Defines an attribute to configure isolation settings for Revit commands.
/// </summary>
/// <remarks>
///     Apply this attribute to classes implementing the <see cref="IExternalCommand" /> interface
///     to specify isolation behavior during Revit command execution.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class RevitCommandIsolationAttribute : Attribute
{
    /// <summary>
    ///     Gets or sets the name of the assembly load context used to execute the command.
    /// </summary>
    /// <value>
    ///     The name of the assembly load context. If left null or empty, the assembly name containing
    ///     the associated class will be used as the default context name.
    /// </value>
    /// <remarks>
    ///     A new assembly load context will be created if the specified one does not already exist.
    /// </remarks>
    public string? ContextName { get; set; }
}
