using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Scotec.Revit.RevitFamily;

/// <summary>
///     Provides access to the metadata stored in the <c>BasicFileInfo</c> stream of a Revit file.
///     This stream contains key/value pairs describing the model identity, worksharing state,
///     authoring application, and format version, among other properties.
/// </summary>
/// <remarks>
///     Use <see cref="Parse(Stream)" /> or <see cref="Parse(byte[])" /> to create an instance
///     from a raw <c>BasicFileInfo</c> stream or byte array extracted from a Revit compound document.
/// </remarks>
public sealed class RevitBasicFileInfo
{
    private static readonly Dictionary<string, string[]> PropertyKeys = new(StringComparer.Ordinal)
    {
        [nameof(IsForeign)] = ["IsForeign", "Foreign"],
        [nameof(ClientAppName)] = ["ClientAppName", "Client App Name"],
        [nameof(Author)] = ["Author"],
        [nameof(ModelIdentity)] = ["ModelIdentity", "Unique Document GUID"],
        [nameof(LatestCentralEpisodeGUID)] =
        [
            "LatestCentralEpisodeGUID",
            "LatestCentralEpisodeGuid",
            "Central model's episode GUID corresponding to the last reload latest"
        ],
        [nameof(LatestCentralVersion)] =
        [
            "LatestCentralVersion",
            "Central model's version number corresponding to the last reload latest"
        ],
        [nameof(AllLocalChangesSavedToCentral)] =
        [
            "AllLocalChangesSavedToCentral",
            "All local changes saved to central"
        ],
        [nameof(CentralPath)] = ["CentralPath", "Central Path"],
        [nameof(Username)] = ["Username"],
        [nameof(LanguageWhenSaved)] = ["LanguageWhenSaved", "Language When Saved"],
        [nameof(Format)] = ["Format"],
        [nameof(IsSavedInLaterVersion)] = ["IsSavedInLaterVersion", "SavedInLaterVersion"],
        [nameof(IsSavedInCurrentVersion)] = ["IsSavedInCurrentVersion", "SavedInCurrentVersion"],
        [nameof(IsCreatedLocal)] = ["IsCreatedLocal", "CreatedLocal"],
        [nameof(IsInProgress)] = ["IsInProgress", "InProgress"],
        [nameof(IsCentral)] = ["IsCentral", "Central"],
        [nameof(IsLocal)] = ["IsLocal", "Local"],
        [nameof(IsWorkshared)] = ["IsWorkshared", "Worksharing"]
    };

    private readonly Dictionary<string, string> _values;

    private RevitBasicFileInfo(Dictionary<string, string> values)
    {
        _values = values;
    }

    /// <summary>
    ///     All parsed key/value pairs.
    /// </summary>
    public IReadOnlyDictionary<string, string> Values => _values;

    /// <summary>
    ///     Gets a value indicating whether the file was saved by a non-Autodesk application.
    /// </summary>
    public bool IsForeign => GetBool(nameof(IsForeign));

    /// <summary>
    ///     Gets the client application name.
    /// </summary>
    public string? ClientAppName => GetPropertyValue(nameof(ClientAppName));

    /// <summary>
    ///     Gets the author of the file.
    /// </summary>
    public string? Author => GetPropertyValue(nameof(Author));

    /// <summary>
    ///     Gets the GUID that identifies the model.
    /// </summary>
    public Guid? ModelIdentity => GetGuid(nameof(ModelIdentity));

    /// <summary>
    ///     Gets the central model episode GUID corresponding to the last reload latest.
    /// </summary>
    public Guid? LatestCentralEpisodeGUID => GetGuid(nameof(LatestCentralEpisodeGUID));

    /// <summary>
    ///     Gets the central model version corresponding to the last reload latest.
    /// </summary>
    public int? LatestCentralVersion => GetInt(nameof(LatestCentralVersion));

    /// <summary>
    ///     Gets a value indicating whether all local changes are saved to central.
    /// </summary>
    public bool AllLocalChangesSavedToCentral => GetBool(nameof(AllLocalChangesSavedToCentral));

    /// <summary>
    ///     Gets the central model path.
    /// </summary>
    public string? CentralPath => GetPropertyValue(nameof(CentralPath));

    /// <summary>
    ///     Gets the username stored in the file information.
    /// </summary>
    public string? Username => GetPropertyValue(nameof(Username));

    /// <summary>
    ///     Gets the language active when the file was last saved.
    /// </summary>
    public string? LanguageWhenSaved => GetPropertyValue(nameof(LanguageWhenSaved));

    /// <summary>
    ///     Gets the file format indicator.
    /// </summary>
    public string? Format => GetPropertyValue(nameof(Format));

    /// <summary>
    ///     Gets a value indicating whether the file was saved in a later Revit version.
    /// </summary>
    public bool IsSavedInLaterVersion => GetBool(nameof(IsSavedInLaterVersion));

    /// <summary>
    ///     Gets a value indicating whether the file was saved in the current Revit version.
    /// </summary>
    public bool IsSavedInCurrentVersion => GetBool(nameof(IsSavedInCurrentVersion));

    /// <summary>
    ///     Gets a value indicating whether the file is a created local file.
    /// </summary>
    public bool IsCreatedLocal => GetBool(nameof(IsCreatedLocal));

    /// <summary>
    ///     Gets a value indicating whether the file is in progress of becoming central.
    /// </summary>
    public bool IsInProgress => GetBool(nameof(IsInProgress));

    /// <summary>
    ///     Gets a value indicating whether the file is the central model.
    /// </summary>
    public bool IsCentral => GetBool(nameof(IsCentral));

    /// <summary>
    ///     Gets a value indicating whether the file is a local model.
    /// </summary>
    public bool IsLocal => GetBool(nameof(IsLocal));

    /// <summary>
    ///     Gets a value indicating whether the file is workshared.
    /// </summary>
    public bool IsWorkshared => GetBool(nameof(IsWorkshared));

    /// <summary>
    ///     Parses a <c>BasicFileInfo</c> stream extracted from a Revit compound document.
    /// </summary>
    /// <param name="stream">The raw <c>BasicFileInfo</c> stream. Must not be <see langword="null" />.</param>
    /// <returns>A <see cref="RevitBasicFileInfo" /> populated with the key/value pairs found in the stream.</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="stream" /> is <see langword="null" />.</exception>
    public static RevitBasicFileInfo Parse(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var memory = new MemoryStream();
        stream.CopyTo(memory);

        return Parse(memory.ToArray());
    }

    /// <summary>
    ///     Parses raw <c>BasicFileInfo</c> bytes extracted from a Revit compound document.
    /// </summary>
    /// <param name="bytes">The raw byte array. Must not be <see langword="null" />.</param>
    /// <returns>A <see cref="RevitBasicFileInfo" /> populated with the key/value pairs found in the data.</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="bytes" /> is <see langword="null" />.</exception>
    public static RevitBasicFileInfo Parse(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        var text = Decode(bytes);

        var values = ParseValues(text);

        return new RevitBasicFileInfo(values);
    }

    /// <summary>
    ///     Creates a <see cref="RevitBasicFileInfo" /> instance from a <c>BasicFileInfo</c> stream.
    /// </summary>
    /// <param name="stream">The raw <c>BasicFileInfo</c> stream. Must not be <see langword="null" />.</param>
    /// <returns>A <see cref="RevitBasicFileInfo" /> populated with the key/value pairs found in the stream.</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="stream" /> is <see langword="null" />.</exception>
    public static RevitBasicFileInfo FromStream(Stream stream)
    {
        return Parse(stream);
    }

    /// <summary>
    ///     Creates a <see cref="RevitBasicFileInfo" /> instance from raw <c>BasicFileInfo</c> bytes.
    /// </summary>
    /// <param name="bytes">The raw byte array. Must not be <see langword="null" />.</param>
    /// <returns>A <see cref="RevitBasicFileInfo" /> populated with the key/value pairs found in the data.</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="bytes" /> is <see langword="null" />.</exception>
    public static RevitBasicFileInfo FromBytes(byte[] bytes)
    {
        return Parse(bytes);
    }

    /// <summary>
    ///     Gets the raw value associated with the exact <paramref name="key" /> as it appears in the file.
    /// </summary>
    /// <param name="key">The exact key to look up. Must not be <see langword="null" />.</param>
    /// <returns>The raw string value, or <see langword="null" /> if the key is not present.</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="key" /> is <see langword="null" />.</exception>
    public string? GetValue(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return _values.TryGetValue(key, out var value)
            ? value
            : null;
    }

    /// <summary>
    ///     Gets the raw string value for the specified property, trying all known key aliases
    ///     for that property before falling back to a direct key lookup.
    /// </summary>
    /// <param name="propertyName"
    ///     >The name of the property as defined on <see cref="RevitBasicFileInfo" /> (e.g. <c>nameof(IsForeign)</c>),
    ///     or any raw key present in the file. Must not be <see langword="null" />.
    /// </param>
    /// <returns>The raw string value, or <see langword="null" /> if no matching key is found.</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="propertyName" /> is <see langword="null" />.</exception>
    public string? GetPropertyValue(string propertyName)
    {
        ArgumentNullException.ThrowIfNull(propertyName);

        if (PropertyKeys.TryGetValue(propertyName, out var keys))
        {
            foreach (var key in keys)
            {
                if (_values.TryGetValue(key, out var value))
                {
                    return value;
                }
            }
        }

        return GetValue(propertyName);
    }

    /// <summary>
    ///     Gets the value of the specified property parsed as a <see cref="Guid" />.
    /// </summary>
    /// <param name="propertyName">The property name. See <see cref="GetPropertyValue" /> for resolution rules.</param>
    /// <returns>The parsed <see cref="Guid" />, or <see langword="null" /> if the property is absent or cannot be parsed.</returns>
    public Guid? GetGuid(string propertyName)
    {
        var value = GetPropertyValue(propertyName);

        return Guid.TryParse(value, out var guid)
            ? guid
            : null;
    }

    /// <summary>
    ///     Gets the value of the specified property parsed as a 32-bit integer.
    /// </summary>
    /// <param name="propertyName">The property name. See <see cref="GetPropertyValue" /> for resolution rules.</param>
    /// <returns>The parsed integer, or <see langword="null" /> if the property is absent or cannot be parsed.</returns>
    public int? GetInt(string propertyName)
    {
        var value = GetPropertyValue(propertyName);

        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
            ? number
            : null;
    }

    private bool GetBool(string propertyName)
    {
        return TryGetBool(propertyName, out var value) && value;
    }

    private bool TryGetBool(string propertyName, out bool value)
    {
        value = false;

        var text = GetPropertyValue(propertyName);

        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return bool.TryParse(text, out value);
    }

    private static Dictionary<string, string> ParseValues(string text)
    {
        var result = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);

        using var reader = new StringReader(text);

        while (reader.ReadLine() is { } line)
        {
            line = line.Trim();

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var index = line.IndexOf(':');

            if (index <= 0)
            {
                continue;
            }

            var key = line[..index].Trim();
            var value = line[(index + 1)..].Trim();

            result[key] = value;
        }

        return result;
    }

    /// <summary>
    ///     Decodes the mixed UTF16/ASCII encoding used inside Revit BasicFileInfo streams.
    /// </summary>
    private static string Decode(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        var candidates = new[]
        {
            DecodeMixedAscii(bytes),
            Encoding.Unicode.GetString(bytes),
            Encoding.BigEndianUnicode.GetString(bytes),
            Encoding.UTF8.GetString(bytes),
            Encoding.ASCII.GetString(bytes)
        };

        string? bestText = null;
        var bestScore = int.MinValue;

        foreach (var candidate in candidates)
        {
            var normalized = NormalizeWhitespace(candidate);
            var score = Score(normalized);
            if (score > bestScore)
            {
                bestScore = score;
                bestText = normalized;
            }
        }

        return bestText ?? string.Empty;
    }

    private static string DecodeMixedAscii(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length);

        for (var i = 0; i < bytes.Length;)
        {
            var b0 = bytes[i];
            var b1 = i + 1 < bytes.Length
                ? bytes[i + 1]
                : (byte)0;

            // UTF16-LE ASCII
            // Example: 57 00 6F 00 ...
            if (IsTextByte(b0) && b1 == 0x00)
            {
                AppendDecodedChar(sb, b0);
                i += 2;
                continue;
            }

            // UTF16-BE ASCII
            // Example: 00 57 00 6F ...
            if (b0 == 0x00 && IsTextByte(b1))
            {
                AppendDecodedChar(sb, b1);
                i += 2;
                continue;
            }

            // Plain ASCII
            if (IsTextByte(b0))
            {
                AppendDecodedChar(sb, b0);
            }
            else
            {
                sb.Append(' ');
            }

            i++;
        }

        return NormalizeWhitespace(sb.ToString());
    }

    private static bool IsTextByte(byte value)
    {
        return value is >= 0x20 and <= 0x7E or (byte)'\r' or (byte)'\n' or (byte)'\t';
    }

    private static void AppendDecodedChar(StringBuilder builder, byte value)
    {
        builder.Append((char)value);
    }

    private static int Score(string text)
    {
        var score = 0;

        foreach (var keys in PropertyKeys.Values)
        {
            foreach (var key in keys)
            {
                if (text.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    score += 10;
                    break;
                }
            }
        }

        foreach (var c in text)
        {
            if (c == ':')
            {
                score++;
            }
            else if (c == '\n')
            {
                score += 2;
            }
        }

        return score;
    }

    private static string NormalizeWhitespace(string text)
    {
        using var reader = new StringReader(text);
        var sb = new StringBuilder(text.Length);

        var isFirstLine = true;

        while (reader.ReadLine() is { } line)
        {
            var normalizedLine = NormalizeLine(line);

            if (normalizedLine.Length == 0)
            {
                continue;
            }

            if (!isFirstLine)
            {
                sb.AppendLine();
            }

            sb.Append(normalizedLine);
            isFirstLine = false;
        }

        return sb.ToString();
    }

    private static string NormalizeLine(string text)
    {
        var sb = new StringBuilder(text.Length);

        var previousWhitespace = false;

        foreach (var c in text)
        {
            if (char.IsWhiteSpace(c))
            {
                if (!previousWhitespace)
                {
                    sb.Append(' ');
                    previousWhitespace = true;
                }
            }
            else
            {
                sb.Append(c);
                previousWhitespace = false;
            }
        }

        return sb.ToString()
            .Trim()
            .Replace(" :", ":", StringComparison.Ordinal);
    }
}