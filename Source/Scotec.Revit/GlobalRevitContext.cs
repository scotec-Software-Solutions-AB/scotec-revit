// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.ApplicationServices;
using Scotec.Revit.EventHandler;

namespace Scotec.Revit;

/// <summary>
///     A long-lived, application-scoped implementation of <see cref="IGlobalRevitContext" /> that
///     wraps the Revit <see cref="Autodesk.Revit.ApplicationServices.Application" /> for use
///     as a singleton outside individual event handler or command scopes.
/// </summary>
/// <remarks>
///     Suitable for DB-level add-ins (<see cref="RevitDbApp" />) that do not have a
///     <see cref="Autodesk.Revit.UI.UIApplication" />. For UI add-ins use <see cref="GlobalRevitUiContext" />.
///     <para>
///         <see cref="GlobalRevitContext.Application" /> becomes available after Revit fires
///         <c>ApplicationInitialized</c>. Accessing it before that event will throw.
///     </para>
///     <para>
///         Unlike the per-invocation <see cref="RevitContext" />, this singleton does not carry
///         a document reference. Use <see cref="IGlobalRevitUiContext.UiDocument" /> or
///         <see cref="IGlobalRevitUiContext.ActiveDocument" /> from the UI context instead.
///     </para>
/// </remarks>
public class GlobalRevitContext : IGlobalRevitContext
{
    /// <summary>
    ///     Initializes a new instance. Subscribes to <c>ApplicationInitialized</c> once
    ///     to capture the <see cref="GlobalRevitContext.Application" /> reference, then unsubscribes.
    /// </summary>
    /// <param name="application">The Revit controlled application provided at add-in startup.</param>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown when <paramref name="application" /> is <c>null</c>.
    /// </exception>
    public GlobalRevitContext(ControlledApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);
        Application = null!; // Will be set in the handler before any access is possible.

        var initializedHandler = new RevitApplicationInitializedHandler(application);
        var initializedHandlerHandle = new SerialDisposable();
        initializedHandlerHandle.Disposable = initializedHandler.AddHandler(context =>
        {
            Application = context.Application;  
            initializedHandlerHandle.Dispose();
        });
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    ///     Thrown when the underlying <see cref="Autodesk.Revit.ApplicationServices.Application" />
    ///     is no longer valid.
    /// </exception>
    public Application Application
    {
        get
        {
            // Revit API: Application.IsValidObject must be checked before access after potential application lifecycle events.
            if (!field.IsValidObject) throw new InvalidOperationException("The Revit application is no longer valid.");
            return field;
        }
        protected set;
    }
}