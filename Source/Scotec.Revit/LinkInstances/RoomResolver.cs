using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;

namespace Scotec.Revit.LinkInstances
{
    public enum SpatialContainerSearchMode
    {
        RoomsOnly,
        SpacesOnly,
        RoomsThenSpaces,
        SpacesThenRooms
    }

    public sealed class SpatialContainmentResolver
    {
        private readonly List<LinkGraphNode> _nodes;
        private readonly Document _hostDoc;

        public SpatialContainmentResolver(List<LinkGraphNode> linkGraphNodes)
        {
            _nodes = linkGraphNodes ?? throw new ArgumentNullException(nameof(linkGraphNodes));

            _hostDoc = _nodes.FirstOrDefault(n => n.Instance == null)?.Document
                ?? throw new ArgumentException("Link graph must contain a host/root node (Instance == null).", nameof(linkGraphNodes));
        }

        /// <summary>
        /// Finds a container (Room and/or Space) for the given element.
        ///
        /// For host elements: pass elementOccurrenceInstance = null.
        /// For linked elements: pass the specific RevitLinkInstance occurrence that places the element's document.
        ///
        /// By default searches containers in the HOST document only (most common use case).
        /// Set searchHostOnlyContainers=false if you also want to consider containers living inside linked docs.
        /// </summary>
        public SpatialContainmentResult? FindContainerForElement(
            Element element,
            RevitLinkInstance? elementOccurrenceInstance,
            SpatialContainerSearchMode mode = SpatialContainerSearchMode.RoomsThenSpaces,
            Phase? phase = null,
            bool searchHostOnlyContainers = true)
        {
            if (element?.Location is not LocationPoint lp)
                return null;

            // 1) Compute the element point in HOST coordinates
            var elemPointInHost = GetElementPointInHost(element, lp.Point, ref elementOccurrenceInstance);
            if (elemPointInHost == null)
                return null;

            // 2) Search candidate container documents
            IEnumerable<LinkGraphNode> containerNodes = searchHostOnlyContainers
                ? _nodes.Where(n => n.Instance == null) // host only
                : _nodes;                               // host + linked container docs

            foreach (var containerNode in containerNodes)
            {
                XYZ pointInContainerDoc = containerNode.TotalTransform.Inverse.OfPoint(elemPointInHost);

                SpatialElement? container = TryGetContainer(containerNode.Document, pointInContainerDoc, phase, mode);
                if (container == null)
                    continue;

                if (!IsValidContainer(container))
                    continue;

                return new SpatialContainmentResult
                {
                    Container = container,
                    ContainerDocument = containerNode.Document,
                    ElementOccurrenceLinkInstance = elementOccurrenceInstance,
                    ContainerOccurrenceLinkInstance = containerNode.Instance
                };
            }

            return null;
        }

        private XYZ? GetElementPointInHost(Element element, XYZ elementPointInElementDoc, ref RevitLinkInstance? elementOccurrenceInstance)
        {
            if (element.Document.Equals(_hostDoc))
            {
                elementOccurrenceInstance = null; // host element => no occurrence
                return elementPointInElementDoc;
            }

            // Linked element: must supply the occurrence instance
            if (elementOccurrenceInstance == null || !elementOccurrenceInstance.IsValidObject)
                return null;
            
            var instance = elementOccurrenceInstance;

            // Find the graph node for the exact occurrence instance (instance element id + its parent doc)
            var occNode = _nodes.FirstOrDefault(n =>
                n.Instance != null
                && n.Instance.Id == instance.Id
                && ReferenceEquals(n.Instance.Document, instance.Document));

            if (occNode == null)
                return null;

            // Ensure this occurrence actually links the element's document
            if (!occNode.Document.Equals(element.Document))
                return null;

            // TotalTransform maps element.Document -> host
            return occNode.TotalTransform.OfPoint(elementPointInElementDoc);
        }

        private static SpatialElement? TryGetContainer(
            Document doc,
            XYZ pointInDoc,
            Phase? phase,
            SpatialContainerSearchMode mode)
        {
            SpatialElement? TryRoom()
            {
                Room? r = phase == null ? doc.GetRoomAtPoint(pointInDoc) : doc.GetRoomAtPoint(pointInDoc, phase);
                return r;
            }

            SpatialElement? TrySpace()
            {
                Space? s = phase == null ? doc.GetSpaceAtPoint(pointInDoc) : doc.GetSpaceAtPoint(pointInDoc, phase);
                return s;
            }

            // There could multiple containers at the same point (e.g. overlapping rooms/spaces, or linked doc containers intersecting host doc containers).
            // So just returning the first one found based on the specified search mode. Adjust logic if you want to return all containers at that point instead.
            // Probably add a parameter to specify whether to return first found vs. all found containers, or even a custom IComparer<SpatialElement>
            // to determine which container(s) to return when multiple are found.
            return mode switch
            {
                SpatialContainerSearchMode.RoomsOnly => TryRoom(),
                SpatialContainerSearchMode.SpacesOnly => TrySpace(),
                SpatialContainerSearchMode.RoomsThenSpaces => TryRoom() ?? TrySpace(),
                SpatialContainerSearchMode.SpacesThenRooms => TrySpace() ?? TryRoom(),
                _ => TryRoom() ?? TrySpace()
            };
        }

        private static bool IsValidContainer(SpatialElement container)
        {
            // Keep your original "Area > 0" style validity check.
            // Spaces sometimes have Volume but Area==0 depending on settings; adjust if needed.
            return container switch
            {
                Room r => r.Area > 0,
                Space s => s.Area > 0,   // or: (s.Area > 0 || s.Volume > 0)
                _ => true
            };
        }
    }
}
