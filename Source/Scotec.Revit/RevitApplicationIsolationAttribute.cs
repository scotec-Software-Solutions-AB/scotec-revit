// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.UI;

namespace Scotec.Revit;

/// <summary>
///     Represents an attribute used to mark implementations of
///     <see cref="IExternalApplication"/> for execution in an isolated context.
/// </summary>
/// <remarks>
///     This attribute is marked as deprecated and will be removed in a future version.
///     To ensure compatibility with future updates, reference the package 
///     <c>Scotec.Revit.Isolation</c> and use the 
///     <c>Scotec.Revit.Isolation.RevitApplicationIsolation</c> attribute instead.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
[Obsolete("This attribute is marked as deprecated and will be removed in a future version. Reference package Scotec.Revit.Isolation and use the Scotec.Revit.Isolation.RevitApplicationIsolation attribute instead.")]

public class RevitApplicationIsolationAttribute : Attribute
{
}
