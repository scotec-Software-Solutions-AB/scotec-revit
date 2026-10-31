// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System.Globalization;
using System.Resources;

namespace Scotec.Revit.Resources;

/// <summary>
///     Provides strongly-typed access to localised strings defined in
///     <c>StringResources.resx</c> (and its culture-specific variants).
/// </summary>
internal static class StringResources
{
    private static readonly ResourceManager ResourceManager =
        new("Scotec.Revit.Resources.StringResources", typeof(StringResources).Assembly);

    /// <summary>Returns the localised string for the given resource key.</summary>
    /// <param name="key">The resource key.</param>
    /// <returns>The localised string, or the key itself when no match is found.</returns>
    private static string Get(string key)
        => ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;

    // -------------------------------------------------------------------------
    // OnFamilyFound dialog
    // -------------------------------------------------------------------------

    /// <summary>Title of the "family already exists" task dialog.</summary>
    internal static string FamilyFound_Title
        => Get(nameof(FamilyFound_Title));

    /// <summary>Main instruction of the "family already exists" task dialog.</summary>
    internal static string FamilyFound_MainInstruction
        => Get(nameof(FamilyFound_MainInstruction));

    /// <summary>Main content shown when the family is currently in use.</summary>
    internal static string FamilyFound_MainContent_InUse
        => Get(nameof(FamilyFound_MainContent_InUse));

    /// <summary>Main content shown when the family is not currently in use.</summary>
    internal static string FamilyFound_MainContent
        => Get(nameof(FamilyFound_MainContent));

    /// <summary>Header of the "overwrite family" command link.</summary>
    internal static string FamilyFound_Overwrite_Header
        => Get(nameof(FamilyFound_Overwrite_Header));

    /// <summary>Description of the "overwrite family" command link.</summary>
    internal static string FamilyFound_Overwrite_Description
        => Get(nameof(FamilyFound_Overwrite_Description));

    /// <summary>Header of the "overwrite family and parameters" command link.</summary>
    internal static string FamilyFound_OverwriteWithParams_Header
        => Get(nameof(FamilyFound_OverwriteWithParams_Header));

    /// <summary>Description of the "overwrite family and parameters" command link.</summary>
    internal static string FamilyFound_OverwriteWithParams_Description
        => Get(nameof(FamilyFound_OverwriteWithParams_Description));

    // -------------------------------------------------------------------------
    // OnSharedFamilyFound dialog
    // -------------------------------------------------------------------------

    /// <summary>Title of the "shared family already exists" task dialog.</summary>
    internal static string SharedFamilyFound_Title
        => Get(nameof(SharedFamilyFound_Title));

    /// <summary>
    ///     Main instruction of the "shared family already exists" task dialog.
    ///     Use <see cref="string.Format(string,object)" /> to substitute <c>{0}</c> with the family name.
    /// </summary>
    internal static string SharedFamilyFound_MainInstruction
        => Get(nameof(SharedFamilyFound_MainInstruction));

    /// <summary>Main content shown when the shared family is currently in use.</summary>
    internal static string SharedFamilyFound_MainContent_InUse
        => Get(nameof(SharedFamilyFound_MainContent_InUse));

    /// <summary>Main content shown when the shared family is not currently in use.</summary>
    internal static string SharedFamilyFound_MainContent
        => Get(nameof(SharedFamilyFound_MainContent));

    /// <summary>Header of the "use family from family manager" command link.</summary>
    internal static string SharedFamilyFound_FromManager_Header
        => Get(nameof(SharedFamilyFound_FromManager_Header));

    /// <summary>Description of the "use family from family manager" command link.</summary>
    internal static string SharedFamilyFound_FromManager_Description
        => Get(nameof(SharedFamilyFound_FromManager_Description));

    /// <summary>Header of the "use family from family manager and overwrite parameters" command link.</summary>
    internal static string SharedFamilyFound_FromManagerWithParams_Header
        => Get(nameof(SharedFamilyFound_FromManagerWithParams_Header));

    /// <summary>Description of the "use family from family manager and overwrite parameters" command link.</summary>
    internal static string SharedFamilyFound_FromManagerWithParams_Description
        => Get(nameof(SharedFamilyFound_FromManagerWithParams_Description));

    /// <summary>Header of the "use family from project" command link.</summary>
    internal static string SharedFamilyFound_FromProject_Header
        => Get(nameof(SharedFamilyFound_FromProject_Header));

    /// <summary>Description of the "use family from project" command link.</summary>
    internal static string SharedFamilyFound_FromProject_Description
        => Get(nameof(SharedFamilyFound_FromProject_Description));
}
