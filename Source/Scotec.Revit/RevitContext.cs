// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace Scotec.Revit;

internal class RevitContext : IRevitContext, IDisposable
{
    public RevitContext(Document document)
    {
        Document = document;
        Application = document.Application;
    }

    protected bool Disposed { get; private set; }

    public Application Application
    {
        get
        {
            if (Disposed) throw new ObjectDisposedException(nameof(RevitContext));
            // Revit API: Application.IsValidObject must be checked before access after potential application lifecycle events.
            if (!field.IsValidObject) throw new InvalidOperationException("The Revit application is no longer valid.");
            return field;
        }
        private init => field = value;
    }

    public Document Document
    {
        get
        {
            if (Disposed) throw new ObjectDisposedException(nameof(RevitContext));
            // Revit API: Document.IsValidObject must be checked before access after potential document lifecycle events.
            if (!field.IsValidObject) throw new InvalidOperationException("The Revit document is no longer valid.");
            return field;
        }
        private init => field = value;
    }

    public void Dispose()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
    }
}
