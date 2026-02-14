// // Copyright © 2023 - 2025 Olaf Meyer
// // Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using Autodesk.Revit.DB;

namespace Scotec.Revit.LinkInstances;

public class LinkGraphNode
{
    public Document Document { get; set; }
    public RevitLinkInstance? Instance { get; set; } // Null for host
    public Transform TotalTransform { get; set; } // Child → Host
}
