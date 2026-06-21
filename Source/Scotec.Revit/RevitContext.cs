// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace Scotec.Revit;

/// <summary>
///     Internal implementation of <see cref="IRevitContext"/> that captures the Revit
///     <see cref="Autodesk.Revit.ApplicationServices.Application"/> and
///     <see cref="Autodesk.Revit.DB.Document"/> at construction time.
/// </summary>
/// <remarks>
///     <para>
///         Revit API objects have an externally managed lifetime controlled by the Revit host.
///         They can become invalid at any point due to document close events, application
///         shutdown, or other Revit lifecycle transitions that are invisible to .NET.
///         This class therefore captures strong references to both objects at construction
///         time — the only safe moment, which is within a Revit event callback or external
///         command execution — and validates them via <c>IsValidObject</c> on every access.
///     </para>
///     <para>
///         The context is intended to be disposed after the associated Revit event or command
///         completes. Once disposed, accessing <see cref="Application"/> or
///         <see cref="Document"/> throws <see cref="ObjectDisposedException"/>.
///     </para>
/// </remarks>
internal class RevitContext : IRevitContext, IDisposable
{
    /// <summary>
    ///     Initializes a new instance for contexts where a document is available,
    ///     for example event handlers and transaction commands.
    /// </summary>
    /// <param name="document">The active Revit document. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="document"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    ///     Both <see cref="Document"/> and <see cref="Application"/> are captured directly from
    ///     <paramref name="document"/> at construction time. <see cref="Application"/> is
    ///     obtained once from <c>document.Application</c> and stored independently so that it
    ///     remains accessible and can be validated even if the document is later closed or
    ///     invalidated by the Revit host.
    /// </remarks>
    public RevitContext(Document document)
    {
        ArgumentNullException.ThrowIfNull(document);
        Document = document;
        Application = document.Application;
    }

    /// <summary>
    ///     Initializes a new instance for contexts where no document is required,
    ///     for example commands that run without an open document.
    /// </summary>
    /// <param name="application">The Revit application. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="application"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    ///     <see cref="Document"/> is <see langword="null"/> in this context.
    ///     <see cref="Application"/> is stored directly to capture the valid reference within
    ///     the safe execution window established by the Revit host.
    /// </remarks>
    protected internal RevitContext(Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        Application = application;
        // Document is intentionally left null for no-document contexts.
    }

    /// <summary>
    ///     Gets a value indicating whether this context has been disposed.
    /// </summary>
    /// <value>
    ///     <see langword="true"/> after <see cref="Dispose"/> has been called;
    ///     otherwise <see langword="false"/>.
    /// </value>
    protected bool Disposed { get; private set; }

    /// <summary>
    ///     Gets the Revit application captured at context creation.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    ///     Thrown when this context has been disposed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the underlying <see cref="Autodesk.Revit.ApplicationServices.Application"/>
    ///     is no longer valid.
    /// </exception>
    /// <remarks>
    ///     The <see cref="Autodesk.Revit.ApplicationServices.Application"/> object is stored
    ///     directly as a property rather than being accessed through the document on each call.
    ///     The Revit host manages the application lifetime independently of .NET, and the object
    ///     can become invalid at any time. Capturing the reference once at construction — inside
    ///     the safe execution window of a Revit event or command — and then validating it via
    ///     <c>IsValidObject</c> on every access is the correct pattern for guarded, stable
    ///     access to Revit API objects.
    /// </remarks>
    public Application Application
    {
        get
        {
            ObjectDisposedException.ThrowIf(Disposed, GetType());
            // Revit API: Application.IsValidObject must be checked before access after potential application lifecycle events.
            if (!field.IsValidObject) throw new InvalidOperationException("The Revit application is no longer valid.");
            return field;
        }
        private init;
    }

    /// <summary>
    ///     Gets the Revit document captured at context creation, or <see langword="null"/>
    ///     when no document was available at construction time.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    ///     Thrown when this context has been disposed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the document reference is non-<see langword="null"/> and the underlying
    ///     <see cref="Autodesk.Revit.DB.Document"/> is no longer valid.
    /// </exception>
    /// <remarks>
    ///     The <see cref="Autodesk.Revit.DB.Document"/> object is stored directly as a property
    ///     because the Revit host controls document lifetime independently of .NET garbage
    ///     collection. The reference is captured once inside the safe execution window of a
    ///     Revit event or command and validated via <c>IsValidObject</c> on every access,
    ///     ensuring that consumers receive either a valid object or a clear exception — never
    ///     a silently stale reference.
    /// </remarks>
    public Document? Document
    {
        get
        {
            ObjectDisposedException.ThrowIf(Disposed, GetType());
            if (field is null) return null;
            // Revit API: Document.IsValidObject must be checked before access after potential document lifecycle events.
            if (!field.IsValidObject) throw new InvalidOperationException("The Revit document is no longer valid.");
            return field;
        }
        // private protected: accessible from RevitUiContext (same assembly, derived type) during construction.
        private protected init;
    }

    /// <summary>
    ///     Marks this context as disposed, preventing further access to
    ///     <see cref="Application"/> and <see cref="Document"/>.
    /// </summary>
    /// <remarks>
    ///     Does not dispose the underlying Revit API objects. Their lifetime is owned and
    ///     managed exclusively by the Revit host.
    /// </remarks>
    public void Dispose()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
    }
}