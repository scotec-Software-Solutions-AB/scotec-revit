// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Scotec.Revit;

/// <summary>
///     Extends <see cref="IRevitContext" /> with UI-layer access: <see cref="UIApplication" />,
///     <see cref="IRevitUiContext.UiDocument" />, and the lazily evaluated
///     <see cref="IRevitUiContext.ActiveView" />.
/// </summary>
/// <remarks>
///     <para>
///         Obtained within the scope of a Revit UI event handler or external command where a
///         <see cref="UIApplication" /> is available. Implementations capture
///         <see cref="UiApplication" /> and, when a document is open,
///         <see cref="UiDocument" /> at construction time inside that safe execution window.
///     </para>
///     <para>
///         When no document is open, <see cref="IRevitUiContext.UiDocument" /> and
///         <see cref="IRevitUiContext.ActiveView" /> return <see langword="null" />.
///         <see cref="UiApplication" /> and <see cref="IRevitContext.Application" /> are
///         always set.
///     </para>
/// </remarks>
public interface IRevitUiContext : IRevitContext
{
    /// <summary>
    ///     Gets the Revit UI application captured at context creation.
    /// </summary>
    /// <exception cref="System.ObjectDisposedException">
    ///     Thrown when the context has been disposed.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the underlying <see cref="UIApplication" /> is no longer valid.
    /// </exception>
    UIApplication UiApplication { get; }

    /// <summary>
    ///     Gets the Revit UI document captured at context creation, or
    ///     <see langword="null" /> when no document was open at construction time.
    /// </summary>
    /// <exception cref="System.ObjectDisposedException">
    ///     Thrown when the context has been disposed.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the UI document reference is non-<see langword="null" /> and the
    ///     underlying <see cref="UIDocument" /> is no longer valid.
    /// </exception>
    UIDocument? UiDocument { get; }

    /// <summary>
    ///     Gets the view currently active in <see cref="IRevitUiContext.UiDocument" />,
    ///     or <see langword="null" /> when no document is open.
    /// </summary>
    /// <exception cref="System.ObjectDisposedException">
    ///     Thrown when the context has been disposed (propagated from <see cref="UiDocument" />).
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the returned <see cref="View" /> is non-<see langword="null" /> and the
    ///     underlying <see cref="View" /> is no longer valid.
    /// </exception>
    /// <remarks>
    ///     Unlike <see cref="UiApplication" /> and <see cref="UiDocument" />, the active view
    ///     is not captured at construction time. The active view can change during a single
    ///     handler invocation, so it is evaluated lazily on each access via
    ///     <see cref="UIDocument.ActiveView" />, with an <c>IsValidObject</c> guard to ensure
    ///     the returned reference is still valid.
    /// </remarks>
    View? ActiveView { get; }
}
