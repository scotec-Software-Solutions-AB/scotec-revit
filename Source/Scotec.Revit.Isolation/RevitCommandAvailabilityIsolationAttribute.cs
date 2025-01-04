// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;

namespace Scotec.Revit.Isolation;

/// <summary>
/// Represents an attribute that marks implementations of 
/// <see cref="Autodesk.Revit.UI.IExternalCommandAvailability"/> for execution in an isolated context.
/// </summary>
/// <remarks>
/// This attribute is intended to be applied to classes that implement 
/// <see cref="Autodesk.Revit.UI.IExternalCommandAvailability"/> to ensure they are executed 
/// in a specific isolated environment.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class RevitCommandAvailabilityIsolationAttribute : Attribute
{
}