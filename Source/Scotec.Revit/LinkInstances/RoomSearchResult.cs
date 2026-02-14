// // Copyright © 2023 - 2025 Olaf Meyer
// // Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace Scotec.Revit.LinkInstances;

public class RoomSearchResult
{
    public Room Room { get; set; }
    public Document RoomDocument { get; set; }
    public RevitLinkInstance? LinkInstance { get; set; }
}
