// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace Scotec.Revit;

internal class RevitContext : IRevitContext
{
    public RevitContext(Document document)
    {
        Document = document;
        Application = document.Application;
    }

    protected bool Disposed { get; private set; }

    public Application Application
    {
        get => Disposed ? throw new ObjectDisposedException(nameof(RevitContext)) : field;
        set => field = Disposed ? throw new ObjectDisposedException(nameof(RevitContext)) : value;
    }

    public Document Document
    {
        get => Disposed ? throw new ObjectDisposedException(nameof(RevitContext)) : field;
        set => field = Disposed ? throw new ObjectDisposedException(nameof(RevitContext)) : value;
    }

    public void Dispose()
    {
        Disposed = true;
        GC.SuppressFinalize(this);
    }
}
