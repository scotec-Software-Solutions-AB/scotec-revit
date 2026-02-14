// Copyright © 2023 - 2025 Olaf Meyer
// Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.DB;

namespace Scotec.Revit.Extensions;

/// <summary>
///     Provides extension methods for working with Revit <see cref="Autodesk.Revit.DB.Element" /> objects.
/// </summary>
public static class RevitElementExtensions
{
    /// <summary>
    ///     Retrieves a representative sample point for the specified Revit element.
    /// </summary>
    /// <param name="element">The Revit element for which to determine the sample point.</param>
    /// <returns>
    ///     A <see cref="XYZ" /> object representing the sample point of the element, or <c>null</c> if the sample point cannot
    ///     be determined.
    /// </returns>
    /// <remarks>
    ///     The method determines the sample point based on the element's location or bounding box:
    ///     - If the element has a <see cref="LocationPoint" />, the point is returned.
    ///     - If the element has a <see cref="LocationCurve" />, the midpoint of the curve is returned.
    ///     - If the element has a bounding box, the center of the bounding box is returned.
    ///     If none of these conditions are met, the method returns <c>null</c>.
    /// </remarks>
    public static XYZ? GetElementSamplePoint(this Element element)
    {
        var location = element.Location;
        switch (location)
        {
            case LocationPoint locationPoint:
            {
                return locationPoint.Point;
            }
            case LocationCurve locationCurve:
            {
                var curve = locationCurve.Curve;
                return (curve.GetEndPoint(0) + curve.GetEndPoint(1)) * 0.5;
            }
            default:
            {
                var boundingBox = element.get_BoundingBox(null);
                if (boundingBox == null)
                {
                    return null;
                }

                return (boundingBox.Min + boundingBox.Max) * 0.5;
            }
        }
    }
}
