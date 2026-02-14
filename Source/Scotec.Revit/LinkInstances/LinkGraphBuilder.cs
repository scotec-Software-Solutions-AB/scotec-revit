// // Copyright © 2023 - 2025 Olaf Meyer
// // Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace Scotec.Revit.LinkInstances;

public static class LinkGraphBuilder
{
    public static List<LinkGraphNode> Build(Document hostDoc)
    {
        var result = new List<LinkGraphNode>
        {
            new LinkGraphNode
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

    private static void CollectLinksRecursive(
        Document currentDoc,
        Transform accumulatedTransform,
        List<LinkGraphNode> result,
        HashSet<Document> recursionStack)
    {
        if (!recursionStack.Add(currentDoc))
            return; // circular reference in this branch

        var links = new FilteredElementCollector(currentDoc)
                    .OfClass(typeof(RevitLinkInstance))
                    .Cast<RevitLinkInstance>();

        foreach (var link in links)
        {
            if (!link.IsValidObject)
                continue;

            var childDoc = link.GetLinkDocument();
            if (childDoc == null)
                continue; // unloaded / not available

            // Consider GetTotalTransform() if you need true-north adjusted transforms.
            var linkTransform = link.GetTransform();

            var combined = accumulatedTransform.Multiply(linkTransform);

            result.Add(new LinkGraphNode
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
