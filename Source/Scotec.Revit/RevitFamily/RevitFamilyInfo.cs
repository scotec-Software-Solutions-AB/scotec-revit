// // Copyright © 2023 - 2024 Olaf Meyer
// // Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using OpenMcdf;
using Scotec.Extensions.Utilities;

namespace Scotec.Revit.RevitFamily;

/// <summary>
///     Represents information about a Revit family, including its symbols, preview image, and associated data.
/// </summary>
/// <remarks>
///     This class provides functionality to manage and access Revit family data, such as symbols and preview images.
///     It initializes and processes the family data from a given file path and stream.
/// </remarks>
public class RevitFamilyInfo
{
    // Define the namespaces
    private static readonly XNamespace AtomNamespace = "http://www.w3.org/2005/Atom";
    private static readonly XNamespace AutodeskNamespace = "urn:schemas-autodesk-com:partatom";

    private readonly Func<Stream> _familyStreamLoader;
    private readonly ILogger? _logger;
    private readonly object _initializationLock = new();
    private Stream? _preview;

    public RevitFamilyInfo(Func<Stream> familyStreamLoader) : this(familyStreamLoader, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RevitFamilyInfo"/> class with the specified family stream loader and logger.
    /// </summary>
    /// <param name="familyStreamLoader">
    /// A function that provides a <see cref="Stream"/> containing the Revit family data.
    /// </param>
    /// <param name="logger">
    /// An instance of <see cref="ILogger"/> used for logging operations.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="familyStreamLoader"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// This constructor enables deferred loading of the Revit family data using the provided stream loader function.
    /// The logger is utilized to record diagnostic and error information during the processing of the family data.
    /// </remarks>
    public RevitFamilyInfo(Func<Stream> familyStreamLoader, ILogger? logger)
    {
        _familyStreamLoader = familyStreamLoader;
        _logger = logger;
    }

    /// <summary>
    ///     Gets a value indicating whether the Revit family information has been successfully initialized.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the Revit family information is initialized; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    ///     This property reflects the initialization state of the Revit family data. 
    ///     It is set to <c>true</c> after the <see cref="Initialize" /> method completes successfully.
    /// </remarks>
    public bool IsInitialized { get; private set; }

    /// <summary>
    ///     Gets a stream containing the preview image of the Revit family, if available.
    /// </summary>
    /// <value>
    ///     A <see cref="Stream" /> representing the preview image of the Revit family, or <c>null</c> if the preview
    ///     image is not initialized or unavailable.
    /// </value>
    /// <remarks>
    ///     The returned stream is a copy of the original preview stream, ensuring that the original data remains
    ///     unaltered. The caller is responsible for disposing of the returned stream after use.
    /// </remarks>
    public Stream? Preview
    {
        get
        {
            if (!IsInitialized || _preview is null)
            {
                return null;
            }

            // Create a copy and return it.
            var stream = new MemoryStream();
            _preview.Position = 0;
            _preview.CopyTo(stream);

            return stream;
        }
    }

    /// <summary>
    ///     Gets the collection of symbols associated with the Revit family.
    /// </summary>
    /// <remarks>
    ///     Each symbol in the collection represents a specific type or variation of the Revit family.
    ///     The collection is populated during the initialization process by extracting symbol data
    ///     from the family file.
    /// </remarks>
    /// <value>
    ///     A list of <see cref="RevitFamilySymbolInfo" /> objects, where each object contains
    ///     information about a specific Revit family symbol.
    /// </value>
    public IList<RevitFamilySymbolInfo> FamilySymbolInfos { get; private set; } = [];

    /// <summary>
    ///     Gets the stream containing the Revit family data.
    /// </summary>
    /// <remarks>
    ///     This property provides access to the raw data of the Revit family as a stream.
    ///     The stream is initialized during the construction of the <see cref="RevitFamilyInfo" /> instance
    ///     and can be used to read or process the family data.
    /// </remarks>
    public Stream Family => GetFamilyStream();

    /// <summary>
    ///     Gets the title of the Revit family.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the title of the Revit family.
    ///     If the title is not available, a default value of "Missing family name" is used.
    /// </value>
    /// <remarks>
    ///     The title is extracted from the family data during initialization.
    /// </remarks>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    ///     Initializes the Revit family information by loading its symbols, preview image, and associated data.
    /// </summary>
    /// <remarks>
    ///     This method ensures that the Revit family data is fully loaded and ready for use.
    ///     It processes the family stream to extract relevant information, such as symbols and the preview image.
    ///     Although this method is called internally on first use, it can also be invoked externally to force loading,
    ///     for example, in a background thread to improve performance or pre-load data.
    /// </remarks>
    /// <exception cref="System.Exception">
    ///     Thrown if there is an error while loading the Revit family data or its associated components.
    /// </exception>
    public void Initialize()
    {
        if (IsInitialized)
        {
            return;
        }

        lock (_initializationLock)
        {
            if (IsInitialized) // Double-check to prevent reinitialization.
            {
                return;
            }

            try
            {
                using var stream = new CompoundFile(GetFamilyStream());
                LoadPartAtom(stream);
                LoadImage(stream);
                IsInitialized = true;
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Error while initializing the family info.");
                throw;
            }
        }
    }

    /// <summary>
    ///     Retrieves the stream containing the Revit family data.
    /// </summary>
    /// <returns>
    ///     A <see cref="Stream" /> object representing the Revit family data.
    /// </returns>
    /// <remarks>
    ///     The returned stream is reset to the beginning of the data before being returned.
    /// </remarks>
    private Stream GetFamilyStream()
    {
        return _familyStreamLoader();
    }

    /// <summary>
    ///     Loads the preview image from the specified compound file.
    /// </summary>
    /// <param name="compoundFile">
    ///     The <see cref="CompoundFile" /> containing the Revit family data.
    /// </param>
    /// <remarks>
    ///     This method attempts to locate and extract the "RevitPreview4.0" stream from the provided compound file.
    ///     If successful, it processes the stream to extract a PNG image and stores it in memory.
    ///     If the stream is not found or the extraction fails, the method logs the issue and continues without throwing an exception.
    /// </remarks>
    /// <exception cref="System.Exception">
    ///     Thrown if an unexpected error occurs during the image extraction process.
    /// </exception>
    private void LoadImage(CompoundFile compoundFile)
    {
        try
        {
            if (!compoundFile.RootStorage.TryGetStream("RevitPreview4.0", out var stream))
            {
                _logger?.LogDebug("The 'RevitPreview4.0' stream could not be found in the family file.");
                return;
            }

            var data = stream.GetData();
            var image = PngExtractor.ExtractPng(data);
            _preview = new MemoryStream(image);
        }
        catch (Exception e)
        {
            // We don't want the user prevent from using the family if the image is missing.
            _logger?.LogDebug(e, "The image could not be extracted from the family file.");
        }
    }

    /// <summary>
    ///     Loads and processes the "PartAtom" stream from the specified compound file.
    /// </summary>
    /// <param name="compoundFile">The compound file containing the "PartAtom" stream.</param>
    /// <remarks>
    ///     This method extracts and parses the "PartAtom" stream to retrieve family metadata, such as the title and family
    ///     types. It updates the <see cref="Title" /> property with the family name and populates the
    ///     <see cref="FamilySymbolInfos" /> collection with the family symbols extracted from the stream.
    /// </remarks>
    /// <exception cref="System.Exception">
    ///     Thrown if the "PartAtom" stream is not found in the compound file or if the family description cannot be loaded.
    /// </exception>
    private void LoadPartAtom(CompoundFile compoundFile)
    {
        try
        {
            if (!compoundFile.RootStorage.TryGetStream("PartAtom", out var stream))
            {
                throw new Exception("The 'PartAtom' stream could not be found in the family file.");
            }

            var data = stream.GetData();
            var xml = Encoding.UTF8.GetString(data);

            var document = XDocument.Parse(xml);
            var root = document.Root;
            if (root is null)
            {
                throw new Exception("Could not extract the family description from the 'PartAtom' stream.");
            }

            // Extract the title
            Title = root.Element(AtomNamespace + "title")?.Value ?? string.Empty;

            // Extract the product version
            //var productVersion = document.Descendants(autodeskNs + "product-version").FirstOrDefault()?.Value;

            //Extract family types
            FamilySymbolInfos = document.Descendants(AutodeskNamespace + "part")
                                        .Where(part => part.Attribute("type")?.Value == "user")
                                        .Select(part => new RevitFamilySymbolInfo(part)).ToList();
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error while reading the family description from the 'PartAtom' stream.");
            throw;
        }
    }
}
