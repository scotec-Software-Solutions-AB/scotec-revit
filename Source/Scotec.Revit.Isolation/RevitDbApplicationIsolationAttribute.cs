// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;

namespace Scotec.Revit.Isolation;

/// <summary>
///     This attribute can be used to mark implementations
///     of IExternalDBApplication for execution in an isolated context.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RevitDbApplicationIsolationAttribute : Attribute
{
}