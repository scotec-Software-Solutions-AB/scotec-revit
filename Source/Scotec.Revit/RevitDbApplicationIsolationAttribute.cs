﻿// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.DB;

namespace Scotec.Revit;

/// <summary>
///     This attribute can be used to mark implementations
///     of <see cref="IExternalDBApplication"/> for execution in an isolated context.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RevitDbApplicationIsolationAttribute : Attribute
{
}
