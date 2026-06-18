// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Scotec.Revit;

internal sealed class RevitUiContext : RevitContext, IRevitUiContext
{
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

    // Lazy property: reflects the view active at the moment of access rather than at handler invocation start.
    public View? ActiveView => UiDocument?.ActiveView;
}
