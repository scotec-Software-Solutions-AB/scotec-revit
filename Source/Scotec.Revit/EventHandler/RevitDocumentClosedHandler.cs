// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using JetBrains.Annotations;

namespace Scotec.Revit.EventHandler;

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.DocumentClosed" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The <see cref="Document" /> has already been destroyed at this point. Only
///         <see cref="DocumentClosedEventArgs.Status" /> is available from the event args.
///     </para>
/// </remarks>
[PublicAPI] 
public abstract class RevitDocumentClosedHandler : RevitAppPostEventHandler<DocumentClosedEventArgs>
{

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.DocumentClosed" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitDocumentClosedHandler(ControlledApplication application)
        : base(application)
    {
        Subscribe();
    }

    /// <inheritdoc />
    protected sealed override void Subscribe()
    {
        Application.DocumentClosed += HandleEvent;
    }

    /// <inheritdoc />
    protected sealed override void Unsubscribe()
    {
        Application.DocumentClosed -= HandleEvent;
    }
}
