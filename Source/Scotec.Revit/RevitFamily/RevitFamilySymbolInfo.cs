// // Copyright © 2023 - 2025 Olaf Meyer
// // Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using System.Xml.Linq;

namespace Scotec.Revit.RevitFamily;

/// <summary>
///     Represents information about a Revit family symbol, including its title and associated data.
/// </summary>
/// <remarks>
///     This class provides functionality to manage and access details of a specific Revit family symbol.
///     It initializes the symbol information from an XML element, extracting relevant data such as the title.
/// </remarks>
public class RevitFamilySymbolInfo
{
    // Define the namespaces
    private static readonly XNamespace AtomNamespace = "http://www.w3.org/2005/Atom";
    private static readonly XNamespace AutodeskNamespace = "urn:schemas-autodesk-com:partatom";

    /// <summary>
    ///     Initializes a new instance of the <see cref="RevitFamilySymbolInfo" /> class using the specified XML element.
    /// </summary>
    /// <param name="symbolElement">
    ///     The XML element containing the data for the Revit family symbol.
    ///     This element is expected to include a child element with the title of the symbol.
    /// </param>
    /// <remarks>
    ///     The constructor extracts and sets the title of the Revit family symbol from the provided XML element.
    /// </remarks>
    public RevitFamilySymbolInfo(XElement symbolElement)
    {
        Title = symbolElement.Element(AtomNamespace + "title")?.Value ?? string.Empty;
        //TODO: Get symbol parameters.
    }

    /// <summary>
    ///     Gets the title of the Revit family symbol.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the title of the Revit family symbol.
    /// </value>
    /// <remarks>
    ///     The title is extracted from the XML element provided during the initialization of the
    ///     <see cref="RevitFamilySymbolInfo" /> instance.
    ///     If the title is not present in the XML element, it defaults to an empty string.
    /// </remarks>
    public string Title { get; private set; }
}
