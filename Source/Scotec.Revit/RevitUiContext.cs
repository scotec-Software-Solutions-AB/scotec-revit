// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Scotec.Revit;

/// <summary>
///     Extends <see cref="RevitContext" /> with UI-layer objects: <see cref="UIApplication" />,
///     <see cref="RevitUiContext.UiDocument" />, and <see cref="RevitUiContext.ActiveView" />.
/// </summary>
/// <remarks>
///     When no document is open at construction time, <see cref="RevitUiContext.UiDocument" />,
///     <see cref="RevitContext.Document" />, and <see cref="RevitUiContext.ActiveView" /> are
///     <c>null</c>; <see cref="RevitUiContext.UiApplication" /> and
///     <see cref="RevitContext.Application" /> are always set.
///     <para>
///         All property accessors throw <see cref="ObjectDisposedException" /> after
///         <see cref="RevitContext.Dispose" /> has been called, and
///         <see cref="InvalidOperationException" /> when the underlying Revit API object is
///         no longer valid.
///     </para>
/// </remarks>
internal sealed class RevitUiContext : RevitContext, IRevitUiContext
{
    /// <summary>
    ///     Initializes a new instance from the given <see cref="UIApplication" />.
    /// </summary>
    /// <param name="uiApplication">The active Revit UI application.</param>
    /// <remarks>
    ///     When <see cref="UIApplication.ActiveUIDocument" /> is non-null at construction time,
    ///     <see cref="RevitContext.Document" /> and <see cref="RevitUiContext.UiDocument" /> are
    ///     populated. Otherwise both remain <c>null</c>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="uiApplication" /> is <c>null</c>.
    /// </exception>
    public RevitUiContext(UIApplication uiApplication)
        // Revit API: ActiveUIDocument may be null when no document is open.
        // Pass Application only; Document is set below when a UI document is available.
        : base((uiApplication ?? throw new ArgumentNullException(nameof(uiApplication))).Application)
    {
        UiApplication = uiApplication;

        if (uiApplication.ActiveUIDocument is { } uiDocument)
        {
            // Set Document on the base class via private protected init.
            Document = uiDocument.Document;
            UiDocument = uiDocument;
        }
    }

    /// <summary>
    ///     Gets the Revit UI application.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    ///     Thrown when the context has been disposed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when <see cref="UIApplication.IsValidObject" /> returns <c>false</c>,
    ///     indicating the UI application object is no longer valid.
    /// </exception>
    public UIApplication UiApplication
    {
        get
        {
            ObjectDisposedException.ThrowIf(Disposed, typeof(RevitUiContext));
            // Revit API: UIApplication.IsValidObject must be checked before access after potential application lifecycle events.
            if (!field.IsValidObject) throw new InvalidOperationException("The Revit UI application is no longer valid.");
            return field;
        }
        private init;
    }

    /// <summary>
    ///     Gets the active Revit UI document, or <c>null</c> when no document was open at
    ///     construction time.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    ///     Thrown when the context has been disposed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the UI document reference is non-null and
    ///     <see cref="UIDocument.IsValidObject" /> returns <c>false</c>,
    ///     indicating the document has been closed or invalidated.
    /// </exception>
    public UIDocument? UiDocument
    {
        get
        {
            ObjectDisposedException.ThrowIf(Disposed, typeof(RevitUiContext));
            // Revit API: UIDocument.IsValidObject must be checked before access after potential document lifecycle events.
            if (field is not null && !field.IsValidObject) throw new InvalidOperationException("The Revit UI document is no longer valid.");
            return field;
        }
        private init;
    }

    /// <summary>
    ///     Gets the view currently active in <see cref="RevitUiContext.UiDocument" />, or
    ///     <c>null</c> when no document is open.
    /// </summary>
    /// <remarks>
    ///     This is a lazy property: it reflects the view active at the moment of access
    ///     rather than at handler invocation start.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the returned <see cref="View" /> reference is non-null and
    ///     <see cref="View.IsValidObject" /> returns <c>false</c>,
    ///     indicating the view has been closed or invalidated.
    /// </exception>
    // Revit API: View.IsValidObject must be checked before access after potential document lifecycle events.
    public View? ActiveView
    {
        get
        {
            var view = UiDocument?.ActiveView;
            if (view is not null && !view.IsValidObject) throw new InvalidOperationException("The active Revit view is no longer valid.");
            return view;
        }
    }
}
