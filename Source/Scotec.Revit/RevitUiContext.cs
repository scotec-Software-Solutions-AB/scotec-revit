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
        // Revit API: ActiveUIDocument may be null when no document is open; guard before dereferencing.
        : base((uiApplication ?? throw new ArgumentNullException(nameof(uiApplication))).ActiveUIDocument?.Document
               ?? throw new ArgumentException("No active UI document is open.", nameof(uiApplication)))
    {
        UiApplication = uiApplication;
        UiDocument = uiApplication.ActiveUIDocument;
    }

    public UIApplication UiApplication
    {
        get
        {
            if (Disposed) throw new ObjectDisposedException(nameof(RevitUiContext));
            // Revit API: UIApplication.IsValidObject must be checked before access after potential application lifecycle events.
            if (!field.IsValidObject) throw new InvalidOperationException("The Revit UI application is no longer valid.");
            return field;
        }
        private init => field = value;
    }

    public UIDocument UiDocument
    {
        get
        {
            if (Disposed) throw new ObjectDisposedException(nameof(RevitUiContext));
            // Revit API: UIDocument.IsValidObject must be checked before access after potential document lifecycle events.
            if (!field.IsValidObject) throw new InvalidOperationException("The Revit UI document is no longer valid.");
            return field;
        }
        private init => field = value;
    }

    // Lazy property: reflects the view active at the moment of access rather than at handler invocation start.
    public View ActiveView => UiDocument.ActiveView;
}
