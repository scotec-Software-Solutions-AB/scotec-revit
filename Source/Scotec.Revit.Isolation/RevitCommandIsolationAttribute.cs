// // Copyright © 2023 - 2024 Olaf Meyer
// // Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.UI;

namespace Scotec.Revit.Isolation;

/// <summary>
///     Represents an attribute used to mark implementations of <see cref="IExternalCommand" />
///     for execution within an isolated context in Revit.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RevitCommandIsolationAttribute : Attribute
{
    public RevitCommandIsolationAttribute()
    {
    }

    public string? ContextName { get; set; }
}
