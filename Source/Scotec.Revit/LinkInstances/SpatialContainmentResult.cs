// // Copyright © 2023 - 2025 Olaf Meyer
// // Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using Autodesk.Revit.DB;

namespace Scotec.Revit.LinkInstances;

public sealed class SpatialContainmentResult
{
    /// <summary>
    ///     The container element (Room, Space, Area, etc.).
    /// </summary>
    public SpatialElement Container { get; set; } = null!;

    /// <summary>
    ///     The document that owns the container (host or a linked document).
    /// </summary>
    public Document ContainerDocument { get; set; } = null!;

    /// <summary>
    ///     The occurrence of the ELEMENT's document (the link instance you passed in).
    ///     Null for host elements.
    /// </summary>
    public RevitLinkInstance? ElementOccurrenceLinkInstance { get; set; }

    /// <summary>
    ///     The occurrence of the CONTAINER's document (if the container is in a linked doc).
    ///     Null if the container is in the host doc.
    /// </summary>
    public RevitLinkInstance? ContainerOccurrenceLinkInstance { get; set; }
}
