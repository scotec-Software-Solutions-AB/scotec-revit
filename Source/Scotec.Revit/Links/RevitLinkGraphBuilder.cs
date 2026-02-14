// Copyright © 2023 - 2025 Olaf Meyer
// Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace Scotec.Revit.Links;

/// <summary>
///     Provides functionality to build a graph of Revit link documents and their transformations.
/// </summary>
public static class RevitLinkGraphBuilder
{
    /// <summary>
    ///     Builds a graph of Revit link documents starting from the specified host document.
    /// </summary>
    /// <param name="hostDoc">The host <see cref="Document" /> from which the graph construction begins.</param>
    /// <returns>
    ///     A list of <see cref="RevitLinkGraphNode" /> objects representing the host document and all linked documents.
    ///     Each node includes the associated link instances (if any) and the cumulative transformations relative to the host.
    /// </returns>
    /// <remarks>
    ///     This method detects and prevents cycles in the link graph to ensure accurate and complete graph construction.
    /// </remarks>
    public static List<RevitLinkGraphNode> Build(Document hostDoc)
    {
        var result = new List<RevitLinkGraphNode>
        {
            new()
            {
                Document = hostDoc,
                TotalTransform = Transform.Identity,
                Instance = null
            }
        };

        // cycle detection only for the current recursion path
        var recursionStack = new HashSet<Document>();

        CollectLinksRecursive(hostDoc, Transform.Identity, result, recursionStack);
        return result;
    }

    /// <summary>
    ///     Recursively collects all linked documents and their transformations, adding them to the result list.
    /// </summary>
    /// <param name="currentDoc">The current <see cref="Document" /> being processed.</param>
    /// <param name="accumulatedTransform">
    ///     The accumulated <see cref="Transform" /> representing the transformation from the host document to the current
    ///     document.
    /// </param>
    /// <param name="result">
    ///     The list to which discovered <see cref="RevitLinkGraphNode" /> objects are added. Each node represents a linked
    ///     document
    ///     and includes its associated link instance and cumulative transformation.
    /// </param>
    /// <param name="recursionStack">
    ///     A set used to track documents currently being processed in the recursion stack. This prevents cycles in the link
    ///     graph.
    /// </param>
    /// <remarks>
    ///     This method ensures that cycles in the link graph are detected and avoided, preventing infinite recursion.
    ///     It processes each linked document, calculates its transformation relative to the host, and adds it to the result
    ///     list.
    /// </remarks>
    private static void CollectLinksRecursive(Document currentDoc, Transform accumulatedTransform, List<RevitLinkGraphNode> result,
                                              HashSet<Document> recursionStack)
    {
        if (!recursionStack.Add(currentDoc))
        {
            return; // circular reference in this branch
        }

        var links = new FilteredElementCollector(currentDoc)
                    .OfClass(typeof(RevitLinkInstance))
                    .Cast<RevitLinkInstance>();

        foreach (var link in links)
        {
            if (!link.IsValidObject)
            {
                continue;
            }

            var childDoc = link.GetLinkDocument();
            if (childDoc == null)
            {
                continue; // unloaded / not available
            }

            // Consider GetTotalTransform() if you need true-north adjusted transforms.
            var linkTransform = link.GetTransform();

            var combined = accumulatedTransform.Multiply(linkTransform);

            result.Add(new RevitLinkGraphNode
            {
                Document = childDoc,
                TotalTransform = combined,
                Instance = link
            });

            CollectLinksRecursive(childDoc, combined, result, recursionStack);
        }

        recursionStack.Remove(currentDoc);
    }
}
