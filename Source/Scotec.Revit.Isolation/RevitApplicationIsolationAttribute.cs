// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;

namespace Scotec.Revit.Isolation;

/// <summary>
/// Represents an attribute that can be applied to classes implementing 
/// <c>IExternalApplication</c> to indicate that they should be executed 
/// in an isolated context.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RevitApplicationIsolationAttribute : Attribute
{
}