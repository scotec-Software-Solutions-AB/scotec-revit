// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace Scotec.Revit;

internal class RevitContext : IRevitContext, IDisposable
{
    /// <summary>
    ///     Initializes a new instance for contexts where a document is available,
    ///     for example event handlers and transaction commands.
    /// </summary>
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
    protected internal RevitContext(Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        Application = application;
        // Document is intentionally left null for no-document contexts.
    }

    protected bool Disposed { get; private set; }

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

    public void Dispose()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
    }
}