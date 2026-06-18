// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Scotec.Revit;

internal sealed class RevitUiContext : RevitContext, IRevitUiContext
{
    public RevitUiContext(UIApplication uiApplication) : base(uiApplication.ActiveUIDocument.Document)
    {
        UiApplication = uiApplication;
        UiDocument = uiApplication.ActiveUIDocument;
    }

    public UIApplication UiApplication
    {
        get => Disposed ? throw new ObjectDisposedException(nameof(RevitUiContext)) : field;
        private init => field = Disposed ? throw new ObjectDisposedException(nameof(RevitUiContext)) : value;
    }

    public UIDocument UiDocument
    {
        get => Disposed ? throw new ObjectDisposedException(nameof(RevitUiContext)) : field;
        private init => field = Disposed ? throw new ObjectDisposedException(nameof(RevitUiContext)) : value;
    }

    // Lazy property, to reflect the view active at the moment of access rather than at handler invocation start.
    public View ActiveView => UiDocument.ActiveView;
}
