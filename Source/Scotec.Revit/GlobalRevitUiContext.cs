// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Scotec.Revit;

/// <summary>
///     A long-lived, application-scoped implementation of <see cref="IGlobalRevitUiContext" /> that
///     wraps the Revit <see cref="Autodesk.Revit.UI.UIApplication" /> for use as a singleton outside individual event
///     handler or command scopes.
/// </summary>
/// <remarks>
///     Use this for UI add-ins derived from <see cref="RevitApp" />. <see cref="UiDocument" />,
///     <see cref="ActiveDocument" />, and <see cref="ActiveView" /> reflect the state at the moment
///     of access and return <c>null</c> when no document is currently open.
/// </remarks>
public class GlobalRevitUiContext : GlobalRevitContext, IGlobalRevitUiContext
{
    /// <summary>
    ///     Initializes a new instance with the given Revit UI controlled application.
    /// </summary>
    /// <param name="uiControlledApplication">The Revit UI controlled application provided at add-in startup.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="uiControlledApplication" /> is <c>null</c>.
    /// </exception>
    public GlobalRevitUiContext(UIControlledApplication uiControlledApplication)
        : base((uiControlledApplication ?? throw new ArgumentNullException(nameof(uiControlledApplication))).ControlledApplication)
    {
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the underlying <see cref="Autodesk.Revit.UI.UIApplication" /> is no longer valid.
    /// </exception>
    public UIApplication UiApplication
    {
        get
        {
            // Revit API: UIApplication wraps the Application; construct from the validated Application field.
            var uiApp = new UIApplication(Application);
            if (!uiApp.IsValidObject) throw new InvalidOperationException("The Revit UI application is no longer valid.");
            return uiApp;
        }
    }

    /// <inheritdoc />
    public UIDocument? UiDocument => UiApplication.ActiveUIDocument;

    /// <inheritdoc />
    public Document? ActiveDocument => UiDocument?.Document;

    /// <inheritdoc />
    public View? ActiveView
    {
        get
        {
            var view = UiDocument?.ActiveView;
            // Revit API: View.IsValidObject must be checked before access after potential document lifecycle events.
            if (view is not null && !view.IsValidObject)
                throw new InvalidOperationException("The active Revit view is no longer valid.");
            return view;
        }
    }
}
