// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Scotec.Revit;

public interface IRevitUiContext : IRevitContext
{
    /// <summary>
    ///     Gets the current Revit UI application.
    /// </summary>
    UIApplication UiApplication { get; }

    /// <summary>
    ///     Gets the current Revit UI document.
    /// </summary>
    UIDocument UiDocument { get; }

    /// <summary>
    ///     Gets the active view in the current Revit UI document.
    /// </summary>
    View ActiveView { get; }
}
