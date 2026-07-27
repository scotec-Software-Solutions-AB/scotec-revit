// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Scotec.Revit.Resources;

namespace Scotec.Revit.RevitFamily;

/// <summary>
///     Implements <see cref="Autodesk.Revit.DB.IFamilyLoadOptions" /> to present interactive dialogs
///     that let the user decide how to handle existing families and shared families during a load operation.
/// </summary>
public sealed class FamilyLoadOptions : IFamilyLoadOptions
{
    /// <summary>
    ///     Gets a value indicating whether the user cancelled the last family load dialog.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the user cancelled the operation; otherwise, <c>false</c>.
    /// </value>
    public bool IsCancelled { get; private set; }

    /// <summary>
    ///     Called by Revit when a family being loaded already exists in the document.
    ///     Displays a dialog asking the user whether to overwrite the existing family.
    /// </summary>
    /// <param name="familyInUse">
    ///     <c>true</c> if the existing family is currently placed in the document; otherwise, <c>false</c>.
    /// </param>
    /// <param name="overwriteParameterValues">
    ///     Set to <c>true</c> to overwrite the existing type parameter values with those from the loaded family;
    ///     <c>false</c> to keep the current values.
    /// </param>
    /// <returns>
    ///     <c>true</c> to proceed with the load and overwrite the existing family;
    ///     <c>false</c> if the user cancelled the operation.
    ///     When <c>false</c> is returned, <see cref="IsCancelled"/> is set to <c>true</c>.
    /// </returns>
    public bool OnFamilyFound(bool familyInUse, /*[UnscopedRef]*/ out bool overwriteParameterValues)
    {
        var dialog = new TaskDialog(StringResources.FamilyFound_Title)
        {
            MainInstruction = StringResources.FamilyFound_MainInstruction,
            MainContent = familyInUse
                ? StringResources.FamilyFound_MainContent_InUse
                : StringResources.FamilyFound_MainContent,
            AllowCancellation = true
        };

        dialog.AddCommandLink(
            TaskDialogCommandLinkId.CommandLink1,
            StringResources.FamilyFound_Overwrite_Header,
            StringResources.FamilyFound_Overwrite_Description
        );

        dialog.AddCommandLink(
            TaskDialogCommandLinkId.CommandLink2,
            StringResources.FamilyFound_OverwriteWithParams_Header,
            StringResources.FamilyFound_OverwriteWithParams_Description
        );

        dialog.CommonButtons = TaskDialogCommonButtons.Cancel;
        dialog.DefaultButton = TaskDialogResult.CommandLink1;

        var result = dialog.Show();

        switch (result)
        {
            case TaskDialogResult.CommandLink1:
            {
                overwriteParameterValues = false;
                return true;
            }

            case TaskDialogResult.CommandLink2:
            {
                overwriteParameterValues = true;
                return true;
            }

            default:
            {
                overwriteParameterValues = false;
                IsCancelled = true;
                return false; // Cancel
            }
        }
    }

    /// <summary>
    ///     Called by Revit when a shared family being loaded already exists in the document.
    ///     Displays a dialog asking the user whether to use the version from the family source or keep the project version.
    /// </summary>
    /// <param name="sharedFamily">
    ///     The <see cref="Autodesk.Revit.DB.Family" /> already present in the document.
    /// </param>
    /// <param name="familyInUse">
    ///     <c>true</c> if the existing shared family is currently placed in the document; otherwise, <c>false</c>.
    /// </param>
    /// <param name="source">
    ///     Set to <see cref="Autodesk.Revit.DB.FamilySource.Family" /> to reload from the external file,
    ///     or <see cref="Autodesk.Revit.DB.FamilySource.Project" /> to keep the version already in the project.
    /// </param>
    /// <param name="overwriteParameterValues">
    ///     Set to <c>true</c> to overwrite the existing type parameter values with those from the reloaded family;
    ///     <c>false</c> to keep the current values.
    /// </param>
    /// <returns>
    ///     <c>true</c> to proceed with the load operation;
    ///     <c>false</c> if the user cancelled the operation.
    /// </returns>
    public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, /*[UnscopedRef]*/ out FamilySource source,
                                    /*[UnscopedRef]*/ out bool overwriteParameterValues)
    {
        var familyName = sharedFamily.Name;

        var dialog = new TaskDialog(StringResources.SharedFamilyFound_Title)
        {
            MainInstruction = string.Format(StringResources.SharedFamilyFound_MainInstruction, familyName),
            MainContent = familyInUse
                ? StringResources.SharedFamilyFound_MainContent_InUse
                : StringResources.SharedFamilyFound_MainContent,
            AllowCancellation = true
        };

        dialog.AddCommandLink(
            TaskDialogCommandLinkId.CommandLink1,
            StringResources.SharedFamilyFound_FromManager_Header,
            StringResources.SharedFamilyFound_FromManager_Description
        );

        dialog.AddCommandLink(
            TaskDialogCommandLinkId.CommandLink2,
            StringResources.SharedFamilyFound_FromManagerWithParams_Header,
            StringResources.SharedFamilyFound_FromManagerWithParams_Description
        );

        dialog.AddCommandLink(
            TaskDialogCommandLinkId.CommandLink3,
            StringResources.SharedFamilyFound_FromProject_Header,
            StringResources.SharedFamilyFound_FromProject_Description
        );

        dialog.CommonButtons = TaskDialogCommonButtons.Cancel;
        dialog.DefaultButton = TaskDialogResult.CommandLink1;

        var result = dialog.Show();

        switch (result)
        {
            case TaskDialogResult.CommandLink1:
                source = FamilySource.Family; // take from file
                overwriteParameterValues = false; // keep existing param values
                return true;

            case TaskDialogResult.CommandLink2:
                source = FamilySource.Family; // take from file
                overwriteParameterValues = true; // overwrite param values
                return true;

            case TaskDialogResult.CommandLink3:
                source = FamilySource.Project; // keep project version
                overwriteParameterValues = false; // irrelevant here, but set safely
                return true;

            default:
                source = FamilySource.Project;
                overwriteParameterValues = false;
                return false; // Cancel
        }
    }
}
