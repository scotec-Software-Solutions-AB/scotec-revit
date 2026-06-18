// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace Scotec.Revit;

/// <summary>
///     Provides access to the Revit <see cref="Autodesk.Revit.ApplicationServices.Application" />
///     and the current <see cref="Autodesk.Revit.DB.Document" /> within the scope of a
///     Revit event handler or external command.
/// </summary>
public interface IRevitContext
{
    /// <summary>
    ///     Gets the current Revit application.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    ///     Thrown when the context has been disposed.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the underlying <see cref="Autodesk.Revit.ApplicationServices.Application" />
    ///     is no longer valid.
    /// </exception>
    Application Application { get; }

    /// <summary>
    ///     Gets the current Revit document, or <c>null</c> when no document is available
    ///     (e.g. commands executed without an open document).
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    ///     Thrown when the context has been disposed.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the document reference is non-null and the underlying
    ///     <see cref="Autodesk.Revit.DB.Document" /> is no longer valid.
    /// </exception>
    Document? Document { get; }
}
