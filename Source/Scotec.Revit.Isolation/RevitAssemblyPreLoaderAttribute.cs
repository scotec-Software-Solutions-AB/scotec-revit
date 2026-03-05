// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;

namespace Scotec.Revit.Isolation;

/// <summary>
///     Marks a class as a custom assembly dependency resolver for Revit add-ins.
/// </summary>
/// <remarks>
///     Classes decorated with this attribute are recognized by the source generator as candidates
///     for pre-loading assemblies within a Revit add-in context.
///     <br /><br />
///     <b>Note:</b> Classes marked with this attribute must implement the
///     <see cref="Scotec.Revit.Loader.IRevitAssemblyPreLoader" /> interface.
///     Only one class per assembly should be marked with this attribute.
///     <br />
///     <b>Note:</b> The <see cref="Scotec.Revit.Loader.IRevitAssemblyPreLoader" /> interface is only available in
///     assemblies marked with the <see cref="RevitAddinAssemblyAttribute" />.
/// </remarks>
public sealed class RevitAssemblyPreLoaderAttribute : Attribute;
