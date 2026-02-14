// Copyright © 2023 - 2025 Olaf Meyer
// Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Scotec.Revit.Extensions;
using Scotec.Revit.Links;

namespace Scotec.Revit.Spatial;

/// <summary>
///     Resolves spatial containment relationships for Revit elements, identifying their containing Rooms or Spaces.
/// </summary>
/// <remarks>
///     This class is designed to work with both host and linked documents, allowing for the determination of spatial
///     containment across different levels of Revit link hierarchies. It provides functionality to locate the container
///     (Room or Space) for a given element based on its location and specified search parameters.
/// </remarks>
public sealed class RevitSpatialContainmentResolver
{
    private readonly Document _hostDoc;
    private readonly List<RevitLinkGraphNode> _nodes;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RevitSpatialContainmentResolver" /> class using a host document.
    /// </summary>
    /// <param name="hostDocument">The host <see cref="Document" /> for which spatial containment will be resolved.</param>
    /// <remarks>
    ///     This constructor builds the Revit link graph for the provided host document and prepares the resolver
    ///     to perform spatial containment queries across the host and all linked documents.
    /// </remarks>
    public RevitSpatialContainmentResolver(Document hostDocument)
        : this(RevitLinkGraphBuilder.Build(hostDocument))
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RevitSpatialContainmentResolver" /> class.
    /// </summary>
    /// <param name="linkGraphNodes">
    ///     A list of <see cref="RevitLinkGraphNode" /> objects representing the Revit link graph.
    ///     This graph must include a host/root node (where <see cref="RevitLinkGraphNode.Instance" /> is <c>null</c>).
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown when <paramref name="linkGraphNodes" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    ///     Thrown when the provided <paramref name="linkGraphNodes" /> does not contain a valid host/root node.
    /// </exception>
    /// <remarks>
    ///     This constructor sets up the resolver to work with the provided Revit link graph, enabling spatial containment
    ///     resolution for elements across host and linked documents. Use this overload if you already have a pre-built link
    ///     graph.
    /// </remarks>
    public RevitSpatialContainmentResolver(List<RevitLinkGraphNode> linkGraphNodes)
    {
        _nodes = linkGraphNodes ?? throw new ArgumentNullException(nameof(linkGraphNodes));

        _hostDoc = _nodes.FirstOrDefault(n => n.Instance == null)?.Document
                   ?? throw new ArgumentException("Link graph must contain a host/root node (Instance == null).", nameof(linkGraphNodes));
    }

    /// <summary>
    ///     Finds a container (Room and/or Space) for the given element.
    /// </summary>
    /// <param name="element">The <see cref="Autodesk.Revit.DB.Element" /> for which to find the containing Room or Space.</param>
    /// <param name="elementOccurrenceInstance">
    ///     The <see cref="RevitLinkInstance" /> occurrence that places the element's document, or <c>null</c> for host
    ///     elements.
    /// </param>
    /// <param name="mode">The search mode specifying whether to look for Rooms, Spaces, or both.</param>
    /// <param name="phase">
    ///     The <see cref="Phase" /> in which to search for the container, or <c>null</c> for the default
    ///     phase.
    /// </param>
    /// <param name="searchHostOnlyContainers">
    ///     If <c>true</c>, only containers in the host document are considered; if <c>false</c>, containers in linked
    ///     documents are also considered.
    /// </param>
    /// <returns>
    ///     A <see cref="RevitSpatialContainmentResult" /> describing the found container, or <c>null</c> if no suitable
    ///     container is found.
    /// </returns>
    /// <remarks>
    ///     This method determines the spatial container (Room or Space) for a given element by transforming its location
    ///     into the coordinate system of each candidate container document and searching for a containing Room or Space.
    ///     The search can be limited to the host document or extended to include linked documents, and the search mode
    ///     controls whether Rooms, Spaces, or both are considered. The method returns the first valid container found.
    /// </remarks>
    public RevitSpatialContainmentResult? FindContainerForElement(Element element,
                                                                  RevitLinkInstance? elementOccurrenceInstance,
                                                                  RevitSpatialContainerSearchMode mode = RevitSpatialContainerSearchMode.RoomsThenSpaces,
                                                                  Phase? phase = null,
                                                                  bool searchHostOnlyContainers = true)
    {
        var point = element.GetElementSamplePoint();
        if (point is null)
        {
            return null;
        }

        // Compute the element point in HOST coordinates
        var elemPointInHost = GetElementPointInHost(element, point, ref elementOccurrenceInstance);
        if (elemPointInHost is null)
        {
            return null;
        }

        // Search candidate container documents
        var containerNodes = searchHostOnlyContainers
            ? _nodes.Where(n => n.Instance == null) // host only
            : _nodes; // host + linked container docs

        foreach (var containerNode in containerNodes)
        {
            var pointInContainerDoc = containerNode.TotalTransform.Inverse.OfPoint(elemPointInHost);

            var container = TryGetContainer(containerNode.Document, pointInContainerDoc, phase, mode);
            if (container is null)
            {
                continue;
            }

            if (!IsValidContainer(container))
            {
                continue;
            }

            return new RevitSpatialContainmentResult
            {
                Container = container,
                ContainerDocument = containerNode.Document,
                ElementOccurrenceLinkInstance = elementOccurrenceInstance,
                ContainerOccurrenceLinkInstance = containerNode.Instance
            };
        }

        return null;
    }

    /// <summary>
    ///     Computes the location of an element in host document coordinates.
    /// </summary>
    /// <param name="element">The <see cref="Autodesk.Revit.DB.Element" /> whose point is to be transformed.</param>
    /// <param name="elementPointInElementDoc">The point in the element's own document coordinates.</param>
    /// <param name="elementOccurrenceInstance">
    ///     Reference to the <see cref="RevitLinkInstance" /> occurrence that places the element's document.
    ///     This will be set to <c>null</c> for host elements.
    /// </param>
    /// <returns>
    ///     The <see cref="XYZ" /> point in host document coordinates, or <c>null</c> if the transformation cannot be
    ///     performed.
    /// </returns>
    /// <remarks>
    ///     This method determines the correct transformation for the element's location, handling both host and linked
    ///     elements.
    ///     For host elements, the point is returned unchanged. For linked elements, the method finds the appropriate link
    ///     instance
    ///     and applies the accumulated transform to map the point into the host document's coordinate system.
    /// </remarks>
    public XYZ? GetElementPointInHost(Element element, XYZ elementPointInElementDoc, ref RevitLinkInstance? elementOccurrenceInstance)
    {
        if (element.Document.Equals(_hostDoc))
        {
            elementOccurrenceInstance = null; // host element => no occurrence
            return elementPointInElementDoc;
        }

        // Linked element: must supply the occurrence instance
        if (elementOccurrenceInstance is not { IsValidObject: true })
        {
            return null;
        }

        var instance = elementOccurrenceInstance;

        // Find the graph node for the exact occurrence instance (instance element id + its parent doc)
        var occNode = _nodes.FirstOrDefault(n =>
            n.Instance != null
            && n.Instance.Id == instance.Id
            && ReferenceEquals(n.Instance.Document, instance.Document));

        if (occNode is null)
        {
            return null;
        }

        // Ensure this occurrence actually links the element's document
        if (!occNode.Document.Equals(element.Document))
        {
            return null;
        }

        // TotalTransform maps element.Document -> host
        return occNode.TotalTransform.OfPoint(elementPointInElementDoc);
    }

    /// <summary>
    ///     Attempts to find a spatial container (Room or Space) at a given point in a document.
    /// </summary>
    /// <param name="doc">The <see cref="Document" /> in which to search for the container.</param>
    /// <param name="pointInDoc">The <see cref="XYZ" /> point in the document's coordinate system.</param>
    /// <param name="phase">The <see cref="Phase" /> in which to search, or <c>null</c> for the default phase.</param>
    /// <param name="mode">The search mode specifying whether to look for Rooms, Spaces, or both.</param>
    /// <returns>
    ///     The found <see cref="SpatialElement" /> (Room or Space), or <c>null</c> if no container is found at the point.
    /// </returns>
    /// <remarks>
    ///     This method checks for a Room or Space at the specified point in the given document, using the provided search mode
    ///     to determine the order and type of containers to check. If multiple containers overlap, only the first found is
    ///     returned.
    ///     The logic can be extended to return all containers or use custom selection criteria if needed.
    /// </remarks>
    private static SpatialElement? TryGetContainer(Document doc,
                                                   XYZ pointInDoc,
                                                   Phase? phase,
                                                   RevitSpatialContainerSearchMode mode)
    {
        // There could multiple containers at the same point (e.g. overlapping rooms/spaces, or linked doc containers intersecting host doc containers).
        // So just returning the first one found based on the specified search mode. Adjust logic if you want to return all containers at that point instead.
        // Probably add a parameter to specify whether to return first found vs. all found containers, or even a custom IComparer<SpatialElement>
        // to determine which container(s) to return when multiple are found.
        return mode switch
        {
            RevitSpatialContainerSearchMode.RoomsOnly => TryRoom(),
            RevitSpatialContainerSearchMode.SpacesOnly => TrySpace(),
            RevitSpatialContainerSearchMode.RoomsThenSpaces => TryRoom() ?? TrySpace(),
            RevitSpatialContainerSearchMode.SpacesThenRooms => TrySpace() ?? TryRoom(),
            _ => TryRoom() ?? TrySpace()
        };

        SpatialElement? TrySpace()
        {
            var s = phase == null ? doc.GetSpaceAtPoint(pointInDoc) : doc.GetSpaceAtPoint(pointInDoc, phase);
            return s;
        }

        SpatialElement? TryRoom()
        {
            var r = phase == null ? doc.GetRoomAtPoint(pointInDoc) : doc.GetRoomAtPoint(pointInDoc, phase);
            return r;
        }
    }

    /// <summary>
    ///     Determines whether a spatial container (Room or Space) is valid for containment.
    /// </summary>
    /// <param name="container">The <see cref="SpatialElement" /> to validate.</param>
    /// <returns>
    ///     <c>true</c> if the container is valid (e.g., has area &gt; 0); otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This method checks the area of the container to ensure it is a valid Room or Space for containment.
    ///     For Rooms, the area must be greater than zero. For Spaces, the area must also be greater than zero,
    ///     but you may adjust the logic to consider volume or other criteria as needed.
    /// </remarks>
    private static bool IsValidContainer(SpatialElement container)
    {
        // Keep your original "Area > 0" style validity check.
        // Spaces sometimes have Volume but Area==0 depending on settings; adjust if needed.
        return container switch
        {
            Room r => r.Area > 0,
            Space s => s.Area > 0, // or: (s.Area > 0 || s.Volume > 0)
            _ => true
        };
    }
}
