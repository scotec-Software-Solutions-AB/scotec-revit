// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

namespace Scotec.Revit;

/// <summary>
///     Identifies the category of a Revit-controlled entry point, used by
///     <see cref="RevitContextTracker.Activate" /> to determine which tracking
///     counters to update.
/// </summary>
internal enum RevitEntryPointKind
{
    /// <summary>
    ///     A document-stable entry point whose active document is reliably
    ///     <see cref="Autodesk.Revit.UI.UIApplication.ActiveUIDocument" /> at the time
    ///     of the call.
    ///     Includes:
    ///     <list type="bullet">
    ///         <item><see cref="Autodesk.Revit.UI.IExternalCommand.Execute" /></item>
    ///         <item><see cref="Autodesk.Revit.UI.IExternalEventHandler.Execute" /></item>
    ///         <item><see cref="Autodesk.Revit.UI.IExternalCommandAvailability.IsCommandAvailable" /></item>
    ///     </list>
    /// </summary>
    Command,

    /// <summary>
    ///     An entry point that may operate on a document other than the current active
    ///     document.
    ///     Includes Revit application and document event handler dispatch and DMU updater
    ///     execution (<see cref="Autodesk.Revit.DB.IUpdater.Execute" />).
    /// </summary>
    Handler
}
