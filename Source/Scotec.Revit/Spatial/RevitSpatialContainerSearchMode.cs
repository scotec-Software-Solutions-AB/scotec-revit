// // Copyright © 2023 - 2025 Olaf Meyer
// // Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

namespace Scotec.Revit.Spatial;

/// <summary>
/// Specifies the mode for searching spatial containers in a Revit model.
/// </summary>
public enum RevitSpatialContainerSearchMode
{
    /// <summary>
    /// Specifies that only rooms should be considered when searching for spatial containers in a Revit model.
    /// </summary>
    RoomsOnly,
    
    /// <summary>
    /// Specifies that only spaces should be considered when searching for spatial containers in a Revit model.
    /// </summary>      
    SpacesOnly,
    
    /// <summary>
    /// Specifies that rooms should be searched first, and if none are found, spaces should be searched.
    /// </summary>
    RoomsThenSpaces,
    
    /// <summary>
    /// Specifies that spaces should be searched first, and if none are found, rooms should be searched.
    /// </summary>
    SpacesThenRooms
}
