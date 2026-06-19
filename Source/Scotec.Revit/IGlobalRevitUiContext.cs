// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Scotec.Revit;

/// <summary>
///     Extends <see cref="IGlobalRevitContext" /> with application-lifetime UI-layer access for
///     <see cref="RevitApp" />-based add-ins.
/// </summary>
/// <remarks>
///     Registered as a singleton in the root DI container for add-ins derived from <see cref="RevitApp" />.
///     <see cref="UiDocument" />, <see cref="ActiveDocument" />, and <see cref="ActiveView" /> reflect
///     the state at the moment of access and return <c>null</c> when no document is currently open.
///     <para>
///         This interface is intended for lightweight, read-only checks such as ribbon state or command
///         availability. Document data should be accessed inside a scoped <see cref="IRevitContext" />
///         rather than through this singleton.
///     </para>
/// </remarks>
public interface IGlobalRevitUiContext : IGlobalRevitContext
{
    /// <summary>
    ///     Gets the Revit UI application.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the underlying <see cref="Autodesk.Revit.UI.UIApplication" /> is no longer valid.
    /// </exception>
    UIApplication UiApplication { get; }

    /// <summary>
    ///     Gets the currently active Revit UI document, or <c>null</c> when no document is open.
    /// </summary>
    UIDocument? UiDocument { get; }

    /// <summary>
    ///     Gets the document associated with the currently active UI document,
    ///     or <c>null</c> when no document is open.
    /// </summary>
    Document? ActiveDocument { get; }

    /// <summary>
    ///     Gets the view currently active in <see cref="UiDocument" />,
    ///     or <c>null</c> when no document is open.
    /// </summary>
    /// <remarks>
    ///     Reflects the view active at the moment of access rather than at any earlier point.
    /// </remarks>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the returned <see cref="Autodesk.Revit.DB.View" /> reference is no longer valid.
    /// </exception>
    View? ActiveView { get; }
}
