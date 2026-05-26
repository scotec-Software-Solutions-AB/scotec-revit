<!--
SPDX-FileCopyrightText: Copyright © 2026 Olaf Meyer
SPDX-FileCopyrightText: Copyright © 2026 scotec Software Solutions AB
SPDX-License-Identifier: MIT
-->

# `RevitBasicFileInfo`

## Overview

`RevitBasicFileInfo` parses the **BasicFileInfo** stream embedded inside Revit files (`.rvt`, `.rfa`).
That stream contains metadata about the file — worksharing state, versioning, authorship, and more —
and can be read **without opening Revit**.

The class is `sealed` and can only be constructed via its static factory methods `Parse`,
`FromStream`, and `FromBytes`.

---

## Parsing Entry Points

| Method | Input | Description |
|---|---|---|
| `Parse(Stream)` | `Stream` | Reads all bytes from the stream and parses them |
| `Parse(byte[])` | `byte[]` | Parses raw bytes directly |
| `FromStream(Stream)` | `Stream` | Convenience wrapper around `Parse(Stream)` |
| `FromBytes(byte[])` | `byte[]` | Convenience wrapper around `Parse(byte[])` |

---

## Processing Pipeline

```
bytes / stream
    └─► Decode()           — detect and apply the best encoding
        └─► ParseValues()  — split lines into key:value pairs
            └─► RevitBasicFileInfo(_values)
                └─► Typed properties (IsCentral, Author, …)
```

---

## Encoding Detection (`Decode` + `DecodeMixedAscii`)

The BasicFileInfo stream uses a non-standard mixed encoding. `Decode` tries five strategies
and selects the best result by scoring:

| Strategy | Encoding |
|---|---|
| `DecodeMixedAscii` | UTF-16 LE/BE bytes interleaved with plain ASCII |
| `Encoding.Unicode` | Pure UTF-16 LE |
| `Encoding.BigEndianUnicode` | Pure UTF-16 BE |
| `Encoding.UTF8` | UTF-8 |
| `Encoding.ASCII` | Plain ASCII |

### Scoring Rules

Each decoded candidate is scored before selection:

| Signal | Points |
|---|---|
| Known property key found | +10 per key group |
| `:` character | +1 |
| Newline `\n` | +2 |

The candidate with the highest score is used.

---

## Whitespace Normalization

Before scoring and before value extraction, text is normalized by:

- Collapsing consecutive whitespace characters into a single space.
- Removing the space before `:` — e.g., `"Central Path :"` → `"Central Path:"`.
- Skipping blank lines.

This ensures consistent key extraction across all Revit versions.

---

## Property Key Aliases

Revit has changed key names across versions. The static `PropertyKeys` dictionary maps each
typed property name to its **known aliases**. When looking up a value, all aliases are tried
in order until a match is found.

**Examples:**

| Property | Aliases |
|---|---|
| `IsForeign` | `IsForeign`, `Foreign` |
| `CentralPath` | `CentralPath`, `Central Path` |
| `ModelIdentity` | `ModelIdentity`, `Unique Document GUID` |
| `IsWorkshared` | `IsWorkshared`, `Worksharing` |
| `LatestCentralEpisodeGUID` | `LatestCentralEpisodeGUID`, `LatestCentralEpisodeGuid`, `Central model's episode GUID corresponding to the last reload latest` |

---

## Value Access API

### Raw Access

| Method | Returns | Description |
|---|---|---|
| `GetValue(string key)` | `string?` | Direct dictionary lookup by exact key |
| `GetPropertyValue(string propertyName)` | `string?` | Resolves all known aliases for a property |

### Typed Access

| Method | Returns | Description |
|---|---|---|
| `GetGuid(string propertyName)` | `Guid?` | Parses the value as a `Guid` |
| `GetInt(string propertyName)` | `int?` | Parses the value as an `int` (invariant culture) |
| `GetBool(string propertyName)` | `bool` | Parses the value as a `bool`; returns `false` if missing |

### All Parsed Pairs

```
IReadOnlyDictionary<string, string> Values { get; }
```

Exposes all raw key/value pairs extracted from the stream.

---

## Exposed Properties

| Property | Type | Description |
|---|---|---|
| `IsForeign` | `bool` | Saved by a non-Autodesk application |
| `ClientAppName` | `string?` | Name of the authoring application |
| `Author` | `string?` | File author |
| `ModelIdentity` | `Guid?` | Unique model identifier |
| `LatestCentralEpisodeGUID` | `Guid?` | Central model episode GUID at last reload |
| `LatestCentralVersion` | `int?` | Central model version at last reload |
| `AllLocalChangesSavedToCentral` | `bool` | Whether all local changes are saved to central |
| `CentralPath` | `string?` | Path to the central model |
| `Username` | `string?` | Username stored in the file |
| `LanguageWhenSaved` | `string?` | Language active when the file was last saved |
| `Format` | `string?` | File format indicator |
| `IsSavedInLaterVersion` | `bool` | Saved by a newer Revit version |
| `IsSavedInCurrentVersion` | `bool` | Saved by the current Revit version |
| `IsCreatedLocal` | `bool` | Is a created local file |
| `IsInProgress` | `bool` | In progress of becoming central |
| `IsCentral` | `bool` | Is the central model |
| `IsLocal` | `bool` | Is a local model |
| `IsWorkshared` | `bool` | Is workshared |

---

## Usage Example

```
// Read the BasicFileInfo stream from a Revit file using a compound document reader (e.g., OpenMcdf):
byte[] basicFileInfoBytes = ReadBasicFileInfoStream("model.rvt");
var info = RevitBasicFileInfo.Parse(basicFileInfoBytes);

Console.WriteLine(info.Author);
Console.WriteLine(info.IsCentral);
Console.WriteLine(info.ModelIdentity);

// Access a raw value not covered by typed properties:
string? customValue = info.GetValue("SomeOtherKey");
```

---

## Notes

- The constructor is private. Use `Parse`, `FromStream`, or `FromBytes`.
- All `bool` properties return `false` when the key is absent or the value cannot be parsed.
- All `string?`, `Guid?`, and `int?` properties return `null` when the key is absent or the value cannot be parsed.
- The internal dictionary uses `OrdinalIgnoreCase` comparison to tolerate casing differences between Revit versions.
