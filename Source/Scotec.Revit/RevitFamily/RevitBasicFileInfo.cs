using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Scotec.Revit.RevitFamily;

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
    ///     Parses a Revit BasicFileInfo stream.
    /// </summary>
    public static RevitBasicFileInfo Parse(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var memory = new MemoryStream();
        stream.CopyTo(memory);

        return Parse(memory.ToArray());
    }

    /// <summary>
    ///     Parses raw BasicFileInfo bytes.
    /// </summary>
    public static RevitBasicFileInfo Parse(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        var text = Decode(bytes);

        var values = ParseValues(text);

        return new RevitBasicFileInfo(values);
    }

    /// <summary>
    ///     Creates a <see cref="RevitBasicFileInfo" /> instance from a BasicFileInfo stream.
    /// </summary>
    public static RevitBasicFileInfo FromStream(Stream stream)
    {
        return Parse(stream);
    }

    /// <summary>
    ///     Creates a <see cref="RevitBasicFileInfo" /> instance from raw BasicFileInfo bytes.
    /// </summary>
    public static RevitBasicFileInfo FromBytes(byte[] bytes)
    {
        return Parse(bytes);
    }

    /// <summary>
    ///     Gets a raw value by key.
    /// </summary>
    public string? GetValue(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return _values.TryGetValue(key, out var value)
            ? value
            : null;
    }

    /// <summary>
    ///     Gets a value using the known aliases of a <see cref="Autodesk.Revit.DB.BasicFileInfo" /> property.
    /// </summary>
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
    ///     Gets a GUID value by property name.
    /// </summary>
    public Guid? GetGuid(string propertyName)
    {
        var value = GetPropertyValue(propertyName);

        return Guid.TryParse(value, out var guid)
            ? guid
            : null;
    }

    /// <summary>
    ///     Gets an integer value by property name.
    /// </summary>
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