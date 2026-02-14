// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.DB;

namespace Scotec.Revit.Isolation;

/// <summary>
///     Defines an attribute to configure isolation settings for Revit applications.
/// </summary>
/// <remarks>
///     Apply this attribute to classes implementing the <see cref="IExternalDBApplication" /> interface
///     to specify isolation behavior during Revit command execution.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class RevitDbApplicationIsolationAttribute : Attribute
{
    /// <summary>
    ///     Gets or sets the name of the assembly load context used to execute the Revit application.
    /// </summary>
    /// <value>
    ///     The name of the assembly load context. If left null or empty, the assembly name containing
    ///     the associated  will be used as the default context name.
    /// </value>
    /// <remarks>
    ///     A new assembly load context will be created if the specified one does not already exist.
    /// </remarks>
    public string? ContextName { get; set; }
}
