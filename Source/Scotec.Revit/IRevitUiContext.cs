// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Scotec.Revit;

/// <summary>
///     Extends <see cref="IRevitContext" /> with UI-layer access: <see cref="UIApplication" />,
///     <see cref="IRevitUiContext.UiDocument" />, and <see cref="IRevitUiContext.ActiveView" />.
/// </summary>
/// <remarks>
///     Implemented within the scope of a Revit UI event handler or external command where
///     a <see cref="UIApplication" /> is available. When no document is open,
///     <see cref="IRevitUiContext.UiDocument" /> and <see cref="IRevitUiContext.ActiveView" />
///     return <c>null</c>.
/// </remarks>
public interface IRevitUiContext : IRevitContext
{
    /// <summary>
    ///     Gets the current Revit UI application.
    /// </summary>
    /// <exception cref="System.ObjectDisposedException">
    ///     Thrown when the context has been disposed.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the underlying <see cref="UIApplication" /> is no longer valid.
    /// </exception>
    UIApplication UiApplication { get; }

    /// <summary>
    ///     Gets the current Revit UI document, or <c>null</c> when no document is open.
    /// </summary>
    /// <exception cref="System.ObjectDisposedException">
    ///     Thrown when the context has been disposed.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the UI document reference is non-null and the underlying
    ///     <see cref="UIDocument" /> is no longer valid.
    /// </exception>
    UIDocument? UiDocument { get; }

    /// <summary>
    ///     Gets the view currently active in <see cref="IRevitUiContext.UiDocument" />,
    ///     or <c>null</c> when no document is open.
    /// </summary>
    /// <remarks>
    ///     Reflects the view active at the moment of access rather than at handler invocation start.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the returned <see cref="View" /> reference is non-null and the underlying
    ///     <see cref="View" /> is no longer valid.
    /// </exception>
    View? ActiveView { get; }
}
