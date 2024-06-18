// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autofac;
using Autofac.Extensions.DependencyInjection;

namespace Scotec.Revit;

/// <summary>
/// </summary>
[Obsolete("This class is deprecated and will be removed in future versions. Please use RevitCommandAvailability instead.")]
public abstract class CommandAvailability : RevitCommandAvailability
{
}
