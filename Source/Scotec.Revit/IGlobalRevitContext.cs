// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.ApplicationServices;

namespace Scotec.Revit;

/// <summary>
///     Provides application-lifetime access to the Revit
///     <see cref="Autodesk.Revit.ApplicationServices.Application" /> for use outside
///     individual event handler or command scopes (e.g. as a singleton in the DI container).
/// </summary>
/// <remarks>
///     Unlike <see cref="IRevitContext" />, which is only available inside a scoped Revit event or
///     command invocation, this interface is registered as a singleton in the root DI container
///     and is always resolvable regardless of whether a Revit callback is currently executing.
/// </remarks>
public interface IGlobalRevitContext
{
    /// <summary>
    ///     Gets the Revit application.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the underlying <see cref="Autodesk.Revit.ApplicationServices.Application" />
    ///     is no longer valid.
    /// </exception>
    Application Application { get; }
}
