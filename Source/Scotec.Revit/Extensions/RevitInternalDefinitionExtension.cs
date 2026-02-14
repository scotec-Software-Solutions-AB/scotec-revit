// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.DB;

namespace Scotec.Revit.Extensions;

/// <summary>
///     Provides extension methods for the <see cref="Autodesk.Revit.DB.InternalDefinition" /> class.
/// </summary>
public static class RevitInternalDefinitionExtension
{
    /// <summary>
    ///     Extracts a <see cref="Guid" /> from the specified <see cref="InternalDefinition" /> instance.
    /// </summary>
    /// <param name="definition">
    ///     The <see cref="InternalDefinition" /> instance from which to extract the GUID.
    /// </param>
    /// <returns>
    ///     A <see cref="Guid" /> if the <see cref="InternalDefinition" /> contains a valid GUID; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    ///     This method attempts to parse the GUID from the <see cref="InternalDefinition" />'s type identifier.
    ///     If the type identifier is empty or does not contain a valid GUID, the method returns <c>null</c>.
    /// </remarks>
    public static Guid? ExtractGuid(this InternalDefinition definition)
    {
        const int guidLength = 32;

        var forgeTypeId = definition.GetTypeId();
        if (forgeTypeId.Empty())
        {
            return null;
        }

        var typeId = forgeTypeId.TypeId;
        var position = typeId.LastIndexOf(':');
        if (position < 0)
        {
            return null;
        }

        return Guid.TryParseExact(typeId.Substring(position + 1, guidLength), "N", out var guid)
            ? guid
            : null;
    }

    /// <summary>
    ///     Determines whether the specified <see cref="InternalDefinition" /> represents a shared parameter.
    /// </summary>
    /// <param name="definition">
    ///     The <see cref="InternalDefinition" /> instance to evaluate.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the <see cref="InternalDefinition" /> represents a shared parameter; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This method checks the type identifier of the <see cref="InternalDefinition" /> to determine if it starts with
    ///     "revit.local.shared", which indicates that the parameter is shared.
    /// </remarks>
    public static bool IsShared(this InternalDefinition definition)
    {
        var typeId = definition.GetTypeId();
        return !typeId.Empty()
               && typeId.TypeId.StartsWith("revit.local.shared", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Determines whether the specified <see cref="InternalDefinition" /> represents a project parameter.
    /// </summary>
    /// <param name="definition">
    ///     The <see cref="InternalDefinition" /> instance to evaluate.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the <see cref="InternalDefinition" /> represents a project parameter; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This method checks the type identifier of the <see cref="InternalDefinition" /> to determine if it starts with
    ///     "revit.local.project", which indicates that the parameter is a project parameter.
    /// </remarks>
    public static bool IsProject(this InternalDefinition definition)
    {
        var typeId = definition.GetTypeId();
        return !typeId.Empty()
               && typeId.TypeId.StartsWith("revit.local.project", StringComparison.OrdinalIgnoreCase);
    }
}
