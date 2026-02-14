// Copyright © 2023 - 2025 Olaf Meyer
// Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.DB;

namespace Scotec.Revit.Links;

/// <summary>
///     Represents a node in a Revit link graph, containing information about a document,
///     its associated link instance (if any), and the total transformation from the child to the host.
/// </summary>
public class RevitLinkGraphNode
{
    /// <summary>
    ///     Gets or sets the <see cref="Document" /> associated with this node.
    /// </summary>
    public required Document Document { get; set; }

    /// <summary>
    ///     Gets or sets the <see cref="RevitLinkInstance" /> associated with this node.
    ///     This is <c>null</c> for the host document.
    /// </summary>
    public RevitLinkInstance? Instance { get; set; } // Null for host

    /// <summary>
    ///     Gets or sets the total <see cref="Transform" /> from the child to the host document.
    /// </summary>
    public required Transform TotalTransform { get; set; } // Child → Host
}
