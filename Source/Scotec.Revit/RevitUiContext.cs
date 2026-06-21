// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Scotec.Revit;

/// <summary>
///     Sealed implementation of <see cref="IRevitUiContext" /> that extends
///     <see cref="RevitContext" /> with UI-layer objects captured at construction time:
///     <see cref="UiApplication" />, <see cref="UiDocument" />, and the lazily evaluated
///     <see cref="ActiveView" />.
/// </summary>
/// <remarks>
///     <para>
///         <see cref="UiApplication" /> and, when available, <see cref="UiDocument" /> are
///         stored directly as properties. Like all Revit API objects, their lifetimes are
///         managed by the Revit host independently of .NET. Capturing strong references once
///         inside the safe execution window — within a Revit UI event callback or external
///         command — and validating them via <c>IsValidObject</c> on every access is the
///         correct Revit API pattern for stable, guarded access.
///     </para>
///     <para>
///         When no document is open at construction time, <see cref="UiDocument" />,
///         <see cref="RevitContext.Document" />, and <see cref="ActiveView" /> are
///         <see langword="null" />; <see cref="UiApplication" /> and
///         <see cref="RevitContext.Application" /> are always set.
///     </para>
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
    /// <param name="uiApplication">The active Revit UI application. Must not be <see langword="null" />.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="uiApplication" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    ///     <see cref="UiApplication" /> is stored directly so that it can be validated via
    ///     <c>IsValidObject</c> on every access. The Revit host controls the
    ///     <see cref="UIApplication" /> lifetime independently of .NET; the only safe moment
    ///     to capture the reference is during construction, which must occur inside a Revit
    ///     UI event callback or external command.
    ///     <para>
    ///         When <see cref="UIApplication.ActiveUIDocument" /> is non-<see langword="null" />
    ///         at construction time, <see cref="UiDocument" /> and
    ///         <see cref="RevitContext.Document" /> are also stored. Otherwise both remain
    ///         <see langword="null" />.
    ///     </para>
    /// </remarks>
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
    ///     Gets the Revit UI application captured at context creation.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    ///     Thrown when this context has been disposed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the underlying <see cref="UIApplication" /> is no longer valid.
    /// </exception>
    /// <remarks>
    ///     The <see cref="UIApplication" /> object is stored directly as a property because
    ///     the Revit host manages its lifetime independently of .NET. The reference is captured
    ///     once at construction time — inside the safe execution window of a Revit UI event or
    ///     command — and validated via <c>IsValidObject</c> on every access, ensuring consumers
    ///     receive either a valid object or a clear exception, never a silently stale reference.
    /// </remarks>
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
    ///     Gets the active Revit UI document captured at context creation, or
    ///     <see langword="null" /> when no document was open at construction time.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    ///     Thrown when this context has been disposed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the UI document reference is non-<see langword="null" /> and the
    ///     underlying <see cref="UIDocument" /> is no longer valid.
    /// </exception>
    /// <remarks>
    ///     The <see cref="UIDocument" /> object is stored directly as a property because the
    ///     Revit host controls document lifetime independently of .NET. The reference is
    ///     captured once at construction time — inside the safe execution window — and
    ///     validated via <c>IsValidObject</c> on every access, ensuring consumers receive
    ///     either a valid object or a clear exception, never a silently stale reference.
    /// </remarks>
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
    ///     Gets the view currently active in <see cref="UiDocument" />, or
    ///     <see langword="null" /> when no document is open.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    ///     Thrown when this context has been disposed (propagated from <see cref="UiDocument" />).
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the returned <see cref="View" /> is non-<see langword="null" /> and
    ///     <c>IsValidObject</c> returns <see langword="false" />, indicating the view has been
    ///     closed or invalidated by the Revit host.
    /// </exception>
    /// <remarks>
    ///     Unlike <see cref="UiApplication" /> and <see cref="UiDocument" />, the active view
    ///     is <em>not</em> captured at construction time. The active view can change during the
    ///     lifetime of a single handler invocation (for example when the user switches views),
    ///     so storing it would risk returning a stale reference. Instead, <c>ActiveView</c> is
    ///     evaluated lazily on each access via <see cref="UIDocument.ActiveView" />, with an
    ///     <c>IsValidObject</c> guard to ensure the returned reference is still valid.
    /// </remarks>
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
