// Copyright © 2023 - 2025 Olaf Meyer
// Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.DB;

namespace Scotec.Revit.Spatial;

/// <summary>
///     Represents the result of a spatial containment search in a Revit model.
/// </summary>
/// <remarks>
///     This class encapsulates information about the spatial container (e.g., Room or Space)
///     that contains a specific element in a Revit model. It includes details about the container,
///     the document it resides in, and any associated Revit link instances.
/// </remarks>
public sealed class RevitSpatialContainmentResult
{
    /// <summary>
    ///     Gets or sets the spatial container (e.g., Room or Space) that contains a specific element in a Revit model.
    /// </summary>
    /// <value>
    ///     A <see cref="Autodesk.Revit.DB.SpatialElement" /> representing the container that holds the element.
    /// </value>
    /// <remarks>
    ///     The container provides spatial context for the element, such as its enclosing Room or Space.
    ///     This property is typically populated as part of a spatial containment search.
    /// </remarks>
    public SpatialElement Container { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the <see cref="Autodesk.Revit.DB.Document" /> that contains the spatial container.
    /// </summary>
    /// <remarks>
    ///     This property represents the Revit document in which the spatial container (e.g., Room or Space) resides.
    ///     It is used to identify the context of the container within the Revit model, including whether it is in the host
    ///     document
    ///     or a linked document.
    /// </remarks>
    public Document ContainerDocument { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the <see cref="Autodesk.Revit.DB.RevitLinkInstance" /> that represents
    ///     the Revit link instance containing the element occurrence.
    /// </summary>
    /// <remarks>
    ///     This property is used to identify the Revit link instance in which the element
    ///     occurrence resides. It is <c>null</c> if the element is located in the host document.
    /// </remarks>
    public RevitLinkInstance? ElementOccurrenceLinkInstance { get; set; }

    /// <summary>
    ///     Gets or sets the <see cref="Autodesk.Revit.DB.RevitLinkInstance" /> that represents
    ///     the Revit link instance containing the container occurrence.
    /// </summary>
    /// <remarks>
    ///     This property is used to identify the Revit link instance in which the container
    ///     occurrence resides. It is <c>null</c> if the container is located in the host document.
    /// </remarks>
    public RevitLinkInstance? ContainerOccurrenceLinkInstance { get; set; }
}
