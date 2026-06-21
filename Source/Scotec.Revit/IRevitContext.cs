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
/// <remarks>
///     Implementations capture Revit API object references at construction time, which must
///     occur inside the safe execution window established by the Revit host (a UI event
///     callback or external command). Once the context is disposed, all property accessors
///     throw <see cref="System.ObjectDisposedException" />.
/// </remarks>
public interface IRevitContext
{
    /// <summary>
    ///     Gets the Revit application captured at context creation.
    /// </summary>
    /// <exception cref="System.ObjectDisposedException">
    ///     Thrown when the context has been disposed.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the underlying <see cref="Autodesk.Revit.ApplicationServices.Application" />
    ///     is no longer valid.
    /// </exception>
    Application Application { get; }

    /// <summary>
    ///     Gets the Revit document captured at context creation, or <see langword="null" />
    ///     when no document was available (e.g. commands executed without an open document).
    /// </summary>
    /// <exception cref="System.ObjectDisposedException">
    ///     Thrown when the context has been disposed.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the document reference is non-<see langword="null" /> and the underlying
    ///     <see cref="Autodesk.Revit.DB.Document" /> is no longer valid.
    /// </exception>
    Document? Document { get; }
}
