using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using OpenMcdf;
using Scotec.Extensions.Utilities;

namespace Scotec.Revit.RevitFamily;

/// <summary>
/// Represents information about a Revit family, including its symbols, preview image, and associated data.
/// </summary>
/// <remarks>
/// This class provides functionality to manage and access Revit family data, such as symbols and preview images.
/// It initializes and processes the family data from a given file path and stream.
/// </remarks>
public class RevitFamilyInfo
{
    // Define the namespaces
    private static readonly XNamespace AtomNamespace = "http://www.w3.org/2005/Atom";
    private static readonly XNamespace AutodeskNamespace = "urn:schemas-autodesk-com:partatom";

    private readonly string _filePath;
    private IList<RevitFamilySymbolInfo> _familySymbols = [];
    private bool _isInitialized;
    private Stream? _preview;

    /// <summary>
    /// Initializes a new instance of the <see cref="RevitFamilyInfo"/> class with the specified file path and family stream.
    /// </summary>
    /// <param name="filePath">The file path of the Revit family.</param>
    /// <param name="familyStream">The stream containing the Revit family data.</param>
    /// <param name="disposeStream">
    /// A boolean value indicating whether the provided <paramref name="familyStream"/> should be disposed after use.
    /// Defaults to <c>true</c>.
    /// </param>
    /// <remarks>
    /// This constructor copies the provided family stream into memory and optionally disposes of the original stream.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown if <paramref name="filePath"/> or <paramref name="familyStream"/> is <c>null</c>.
    /// </exception>
    public RevitFamilyInfo(string filePath, Stream familyStream, bool disposeStream = true)
    {
        _filePath = filePath;
        var stream = new MemoryStream();
        familyStream.CopyTo(stream);

        Family = stream;

        if (disposeStream)
        {
            familyStream.Dispose();
        }
    }

    /// <summary>
    /// Gets a stream containing the preview image of the Revit family.
    /// </summary>
    /// <remarks>
    /// This property initializes the Revit family data if it has not been initialized yet.
    /// The returned stream is a copy of the internal preview stream, ensuring the original stream remains unaltered.
    /// </remarks>
    /// <returns>
    /// A <see cref="Stream"/> object containing the preview image of the Revit family.
    /// </returns>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown if the preview stream is not available or has not been properly initialized.
    /// </exception>
    public Stream? Preview
    {
        get
        {
            Initialize();

            if (_preview is null)
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
    /// Gets the collection of family symbol information associated with the Revit family.
    /// </summary>
    /// <remarks>
    /// This property provides access to the list of <see cref="RevitFamilySymbolInfo"/> objects,
    /// representing the symbols defined within the Revit family. The symbols are initialized
    /// and loaded from the family data during the first access.
    /// </remarks>
    /// <value>
    /// A list of <see cref="RevitFamilySymbolInfo"/> objects containing details about the family symbols.
    /// </value>
    public IList<RevitFamilySymbolInfo> FamilySymbolInfos
    {
        get
        {
            Initialize();
            return _familySymbols;
        }
        private set => _familySymbols = value;
    }

    /// <summary>
    /// Gets the stream containing the Revit family data.
    /// </summary>
    /// <remarks>
    /// This property provides access to the raw data of the Revit family as a stream.
    /// The stream is initialized during the construction of the <see cref="RevitFamilyInfo"/> instance
    /// and can be used to read or process the family data.
    /// </remarks>
    public Stream Family { get; }

    /// <summary>
    /// Gets the title of the Revit family.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> representing the title of the Revit family. 
    /// If the title is not available, a default value of "Missing family name" is used.
    /// </value>
    /// <remarks>
    /// The title is extracted from the family data during initialization.
    /// </remarks>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Retrieves the stream containing the Revit family data.
    /// </summary>
    /// <returns>
    /// A <see cref="Stream"/> object representing the Revit family data.
    /// </returns>
    /// <remarks>
    /// The returned stream is reset to the beginning of the data before being returned.
    /// </remarks>
    private Stream GetFamilyStream()
    {
        Family.Position = 0;
        return Family;
    }

    /// <summary>
    /// Loads the preview image from the specified compound file.
    /// </summary>
    /// <param name="compoundFile">The <see cref="CompoundFile"/> containing the Revit family data.</param>
    /// <remarks>
    /// This method attempts to locate and extract the "RevitPreview4.0" stream from the provided compound file.
    /// If successful, it processes the stream to extract a PNG image and stores it in memory.
    /// </remarks>
    /// <exception cref="System.Exception">
    /// Thrown if the "RevitPreview4.0" stream cannot be found in the compound file.
    /// </exception>
    private void LoadImage(CompoundFile compoundFile)
    {
        try
        {
            if (!compoundFile.RootStorage.TryGetStream("RevitPreview4.0", out var stream))
            {
                throw new Exception("The 'RevitPreview4.0' stream could not be found in the family file.");
            }

            var data = stream.GetData();
            var image = PngExtractor.ExtractPng(data);
            _preview = new MemoryStream(image);
        }
        catch (Exception)
        {
            //TODO: Log error;
        }
    }

    /// <summary>
    /// Initializes the Revit family information by loading its symbols, preview image, and associated data.
    /// </summary>
    /// <remarks>
    /// This method ensures that the Revit family data is fully loaded and ready for use. 
    /// It processes the family stream to extract relevant information, such as symbols and the preview image.
    /// </remarks>
    /// <exception cref="System.Exception">
    /// Thrown if there is an error while loading the Revit family data or its associated components.
    /// </exception>
    private void Initialize()
    {
        if (_isInitialized)
        {
            return;
        }

        using var stream = new CompoundFile(GetFamilyStream());

        LoadPartAtom(stream);
        LoadImage(stream);

        _isInitialized = true;
    }

    /// <summary>
    /// Loads and processes the "PartAtom" stream from the specified compound file.
    /// </summary>
    /// <param name="compoundFile">The compound file containing the "PartAtom" stream.</param>
    /// <remarks>
    /// This method extracts and parses the "PartAtom" stream to retrieve family metadata, such as the title and family types.
    /// It updates the <see cref="Title"/> property with the family name and populates the <see cref="FamilySymbolInfos"/> collection
    /// with the family symbols extracted from the stream.
    /// </remarks>
    /// <exception cref="System.Exception">
    /// Thrown if the "PartAtom" stream is not found in the compound file or if the family description cannot be loaded.
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
                throw new Exception("Could not load family description.");
            }

            // Extract the title
            Title = root.Element(AtomNamespace + "title")?.Value ?? "Missing family name";

            // Extract the product version
            //var productVersion = document.Descendants(autodeskNs + "product-version").FirstOrDefault()?.Value;

            //Extract family types
            FamilySymbolInfos = document.Descendants(AutodeskNamespace + "part")
                                        .Where(part => part.Attribute("type")?.Value == "user")
                                        .Select(part => new RevitFamilySymbolInfo(part)).ToList();
        }
        catch (Exception)
        {
            //TODO: Logging
            FamilySymbolInfos = new List<RevitFamilySymbolInfo>();
        }
    }
}
