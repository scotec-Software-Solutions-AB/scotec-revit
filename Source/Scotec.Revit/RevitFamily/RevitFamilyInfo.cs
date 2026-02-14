// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using OpenMcdf;
using Scotec.Extensions.Linq;
using Scotec.Extensions.Utilities;

namespace Scotec.Revit.RevitFamily;

/// <summary>
///     Represents information about a Revit family, including its symbols, preview image, and associated data.
/// </summary>
/// <remarks>
///     This class provides functionality to manage and access Revit family data, such as symbol names and preview images.
///     It initializes and processes the family data from a specified file path or stream.
///     The class enables access to family information without loading the entire family file into memory.
/// </remarks>
public class RevitFamilyInfo : IDisposable
{
    // Define the namespaces
    private static readonly XNamespace AtomNamespace = "http://www.w3.org/2005/Atom";
    private static readonly XNamespace AutodeskNamespace = "urn:schemas-autodesk-com:partatom";

    private readonly Func<Stream> _familyStreamLoader;
    private readonly string? _infoStorageName;
    private readonly Dictionary<string, Stream> _infoStreams = [];
    private readonly object _initializationLock = new();
    private readonly ILogger? _logger;
    private Stream? _preview;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RevitFamilyInfo" /> class with the specified family stream loader
    ///     and optional information storage name.
    /// </summary>
    /// <param name="familyStreamLoader">
    ///     A function that provides a <see cref="Stream" /> containing the Revit family data.
    /// </param>
    /// <param name="infoStorageName">
    ///     An optional name for the information storage associated with the Revit family data. Defaults to <c>null</c>.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown when <paramref name="familyStreamLoader" /> is <c>null</c>.
    /// </exception>
    /// <remarks>
    ///     This constructor initializes the Revit family information using the provided stream loader function.
    ///     The optional information storage name can be used to identify or categorize the family data.
    /// </remarks>
    public RevitFamilyInfo(Func<Stream> familyStreamLoader, string? infoStorageName = null) : this(familyStreamLoader, infoStorageName, null)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RevitFamilyInfo" /> class with the specified family stream loader,
    ///     optional information storage name, and logger.
    /// </summary>
    /// <param name="familyStreamLoader">
    ///     A function that provides a <see cref="Stream" /> containing the Revit family data.
    /// </param>
    /// <param name="infoStorageName">
    ///     An optional name for the storage associated with the Revit family data. If not provided, defaults to <c>null</c>.
    /// </param>
    /// <param name="logger">
    ///     An instance of <see cref="ILogger" /> used for logging operations. If not provided, defaults to <c>null</c>.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown when <paramref name="familyStreamLoader" /> is <c>null</c>.
    /// </exception>
    /// <remarks>
    ///     This constructor enables the initialization of the Revit family information with deferred loading of the family
    ///     data.
    ///     The optional storage name can be used to identify or associate metadata with the family data.
    ///     The logger is utilized to record diagnostic and error information during the processing of the family data.
    /// </remarks>
    public RevitFamilyInfo(Func<Stream> familyStreamLoader, string? infoStorageName, ILogger? logger)
    {
        _familyStreamLoader = familyStreamLoader;
        _infoStorageName = infoStorageName;
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
    ///     Gets the product name associated with the Revit family.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the product name extracted from the family data.
    /// </value>
    /// <remarks>
    ///     This property is populated during the initialization of the <see cref="RevitFamilyInfo" /> instance
    ///     by parsing the family data. It represents the product information defined within the Revit family file.
    /// </remarks>
    public string Product { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the version of the product associated with the Revit family.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the version of the product.
    /// </value>
    /// <remarks>
    ///     This property is extracted from the family data and indicates the version of the product
    ///     that the Revit family is associated with. It is initialized during the loading of the family data.
    /// </remarks>
    public string ProductVersion { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the date and time when the Revit family was last updated.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime" /> representing the last update timestamp of the Revit family.
    /// </value>
    /// <remarks>
    ///     This property is populated during the loading of the family data from the associated file or stream.
    ///     It reflects the most recent modification date as specified in the family metadata.
    /// </remarks>
    public DateTime Updated { get; private set; }

    /// <summary>
    ///     Releases all resources used by the <see cref="RevitFamilyInfo" /> instance.
    /// </summary>
    /// <remarks>
    ///     This method is used to clean up resources such as streams and other disposable objects
    ///     associated with the <see cref="RevitFamilyInfo" /> instance. It ensures that unmanaged
    ///     resources are properly released to avoid memory leaks.
    /// </remarks>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Initializes the Revit family information by loading its symbols, preview image, and associated data.
    /// </summary>
    /// <remarks>
    ///     This method ensures that the Revit family data is fully loaded and ready for use.
    ///     It processes the family stream to extract relevant information, such as symbols and the preview image.
    ///     Although this method is called internally on first use, it can also be invoked externally to force loading,
    ///     for example, in a background thread to improve performance or pre-load data.
    /// </remarks>
    /// <exception cref="Exception">
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
                using var root = RootStorage.Open(GetFamilyStream(), StorageModeFlags.LeaveOpen);
                LoadPartAtom(root);
                LoadImage(root);
                LoadStreams(root);
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
    ///     Attempts to retrieve a data stream associated with the specified stream name.
    /// </summary>
    /// <param name="streamName">
    ///     The name of the stream to retrieve. This is used as a key to locate the associated data stream.
    /// </param>
    /// <param name="stream">
    ///     When this method returns, contains the <see cref="Stream" /> associated with the specified stream name,
    ///     if the operation is successful; otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    ///     <c>true</c> if a data stream associated with the specified stream name is found and successfully retrieved;
    ///     otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This method checks if a data stream exists for the given stream name in the internal collection.
    ///     If found, it creates a copy of the stream and returns it through the <paramref name="stream" /> parameter.
    /// </remarks>
    public bool TryGetInfoDataStream(string streamName, [NotNullWhen(true)] out Stream? stream)
    {
        if (_infoStreams.TryGetValue(streamName, out var infoStream))
        {
            stream = new MemoryStream();
            infoStream.Position = 0;
            infoStream.CopyTo(stream);
            stream.Position = 0;
            infoStream.Position = 0;

            return true;
        }

        stream = null;
        return false;
    }

    /// <summary>
    ///     Attempts to retrieve a stream from the Revit family data based on the specified path.
    /// </summary>
    /// <param name="path">
    ///     The path to the desired stream within the Revit family data. The path segments are separated by '/'.
    /// </param>
    /// <param name="stream">
    ///     When this method returns, contains the <see cref="Stream" /> object if the stream is found; otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the stream is successfully retrieved; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This method navigates through the storage structure of the Revit family data to locate the specified stream.
    ///     The path is split into segments, and each segment is used to traverse the storage hierarchy.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown when <paramref name="path" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if the Revit family data stream cannot be accessed or is in an invalid state.
    /// </exception>
    public bool TryGetStream(string path, [NotNullWhen(true)] out Stream? stream)
    {
        using var root = RootStorage.Open(GetFamilyStream(), StorageModeFlags.LeaveOpen);

        return TryGetStream(root, path.Split('/'), out stream);
    }

    /// <summary>
    ///     Releases all resources used by the <see cref="RevitFamilyInfo" /> instance.
    /// </summary>
    /// <remarks>
    ///     This method is used to clean up resources such as streams and other disposable objects
    ///     associated with the <see cref="RevitFamilyInfo" /> instance. It ensures that unmanaged
    ///     resources are properly released to avoid memory leaks.
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _preview?.Dispose();
            _infoStreams.Values.ForAll(stream => stream.Dispose());
        }
    }

    /// <summary>
    ///     Attempts to retrieve a stream from the specified storage and path.
    /// </summary>
    /// <param name="storage">
    ///     The <see cref="Storage" /> object representing the root storage to search within.
    /// </param>
    /// <param name="path">
    ///     An array of strings representing the hierarchical path to the desired stream.
    /// </param>
    /// <param name="stream">
    ///     When this method returns, contains the <see cref="Stream" /> object if the operation is successful; otherwise,
    ///     <c>null</c>.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the stream is successfully retrieved; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This method recursively navigates the specified storage hierarchy to locate and retrieve the desired stream.
    ///     If the path contains a single element, it attempts to open the stream directly. If successful, the stream is copied
    ///     into a new <see cref="MemoryStream" /> for further use.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="storage" /> or <paramref name="path" /> is <c>null</c>.
    /// </exception>
    private bool TryGetStream(Storage storage, string[] path, [NotNullWhen(true)] out Stream? stream)
    {
        stream = null;
        if (path.Length == 1)
        {
            if (!storage.TryOpenStream(path[0], out var storageStream))
            {
                return false;
            }

            stream = new MemoryStream();
            storageStream.Position = 0;
            storageStream.CopyTo(stream);
            stream.Position = 0;

            storageStream.Dispose();
            return true;
        }

        if (storage.TryOpenStorage(path[0], out var childStorage))
        {
            return TryGetStream(childStorage, path[1..], out stream);
        }

        return false;
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
    /// <param name="storage">
    ///     The <see cref="RootStorage" /> containing the Revit family data.
    /// </param>
    /// <remarks>
    ///     This method attempts to locate and extract the "RevitPreview4.0" stream from the provided compound file.
    ///     If successful, it processes the stream to extract a PNG image and stores it in memory.
    ///     If the stream is not found or the extraction fails, the method logs the issue and continues without throwing an
    ///     exception.
    /// </remarks>
    /// <exception cref="Exception">
    ///     Thrown if an unexpected error occurs during the image extraction process.
    /// </exception>
    private void LoadImage(RootStorage storage)
    {
        try
        {
            if (!storage.TryOpenStream("RevitPreview4.0", out var stream))
            {
                _logger?.LogDebug("The 'RevitPreview4.0' stream could not be found in the family file.");
                return;
            }

            var buffer = new byte[stream.Length];
            stream.ReadExactly(buffer, 0, buffer.Length);
            stream.Dispose();
            var image = PngExtractor.ExtractPng(buffer);
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
    /// <param name="storage">The compound file containing the "PartAtom" stream.</param>
    /// <remarks>
    ///     This method extracts and parses the "PartAtom" stream to retrieve family metadata, such as the title and family
    ///     types. It updates the <see cref="Title" /> property with the family name and populates the
    ///     <see cref="FamilySymbolInfos" /> collection with the family symbols extracted from the stream.
    /// </remarks>
    /// <exception cref="Exception">
    ///     Thrown if the "PartAtom" stream is not found in the compound file or if the family description cannot be loaded.
    /// </exception>
    private void LoadPartAtom(RootStorage storage)
    {
        try
        {
            if (!storage.TryOpenStream("PartAtom", out var atomStream))
            {
                throw new Exception("The 'PartAtom' stream could not be found in the family file.");
            }

            var buffer = new byte[atomStream.Length];
            atomStream.ReadExactly(buffer, 0, buffer.Length);
            atomStream.Dispose();
            var xml = Encoding.UTF8.GetString(buffer);

            var document = XDocument.Parse(xml);
            var root = document.Root;
            if (root is null)
            {
                throw new Exception("Could not extract the family description from the 'PartAtom' stream.");
            }

            // Extract the title
            Title = root.Element(AtomNamespace + "title")?.Value ?? string.Empty;

            // Extract the product and product version
            Product = document.Descendants(AutodeskNamespace + "product").First().Value;
            ProductVersion = document.Descendants(AutodeskNamespace + "product-version").First().Value;

            // Extract the updated date
            Updated = DateTime.Parse(document.Descendants(AutodeskNamespace + "updated").First().Value);

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

    /// <summary>
    ///     Loads streams from the specified root storage within the compound file.
    /// </summary>
    /// <param name="rootStorage">
    ///     The <see cref="RootStorage" /> representing the root storage of the compound file.
    /// </param>
    /// <remarks>
    ///     This method enumerates the entries within the specified storage and processes each entry of type
    ///     <see cref="EntryType.Stream" />. For each stream entry, it attempts to load the stream into memory.
    ///     If a stream cannot be opened or an error occurs during the process, the corresponding entry is set to <c>null</c>,
    ///     and an error is logged.
    /// </remarks>
    /// <exception cref="System.Exception">
    ///     Thrown if an unexpected error occurs while attempting to load a stream.
    /// </exception>
    private void LoadStreams(RootStorage rootStorage)
    {
        if (string.IsNullOrEmpty(_infoStorageName) || !rootStorage.TryOpenStorage(_infoStorageName, out var storage))
        {
            return;
        }

        var streamEntries = storage.EnumerateEntries().Where(e => e.Type == EntryType.Stream);

        foreach (var streamEntry in streamEntries)
        {
            var streamName = streamEntry.Name;
            try
            {
                if (storage.TryOpenStream(streamName, out var stream))
                {
                    var memoryStream = new MemoryStream();
                    stream.CopyTo(memoryStream);
                    stream.Dispose();
                    memoryStream.Position = 0;
                    _infoStreams[streamName] = memoryStream;
                }
                else
                {
                    _logger?.LogInformation("Stream '{streamName}' not found in the compound file.", streamName);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to load stream '{streamName}'.", streamName);
            }
        }
    }
}
