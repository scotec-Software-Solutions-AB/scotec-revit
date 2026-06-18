// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace Scotec.Revit;

public interface IRevitContext
{
    /// <summary>
    ///     Gets the current Revit application.
    /// </summary>
    Application Application { get; }

    /// <summary>
    ///     Gets the current Revit document, or throws <see cref="System.InvalidOperationException" />
    ///     when no document is available (e.g. commands executed without an open document).
    /// </summary>
    Document? Document { get; }
}
