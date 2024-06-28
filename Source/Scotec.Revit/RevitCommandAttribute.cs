// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.UI;

namespace Scotec.Revit;

/// <summary>
///     The RevitCommandAttribute can be used to mark implementations
///     of <see cref="IExternalCommand" /> for execution in an isolated context.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RevitCommandAttribute : Attribute
{
}
