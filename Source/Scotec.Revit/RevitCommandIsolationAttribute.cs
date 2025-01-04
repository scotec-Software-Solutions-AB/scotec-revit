// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.UI;

namespace Scotec.Revit;

/// <summary>
/// Represents an attribute that marks implementations of <see cref="IExternalCommand"/>
/// for execution in an isolated context.
/// </summary>
/// <remarks>
/// This attribute is marked as deprecated and will be removed in a future version.
/// It is recommended to reference the Scotec.Revit.Isolation package and use the
/// <c>Scotec.Revit.Isolation.RevitCommandIsolation</c> attribute instead.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
[Obsolete("This attribute is marked as deprecated and will be removed in a future version. Reference package Scotec.Revit.Isolation and use the Scotec.Revit.Isolation.RevitCommandIsolation attribute instead.")]

public class RevitCommandIsolationAttribute : Attribute
{
}
