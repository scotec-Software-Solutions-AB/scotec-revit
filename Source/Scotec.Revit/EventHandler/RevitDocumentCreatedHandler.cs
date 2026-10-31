// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Events;
using JetBrains.Annotations;

namespace Scotec.Revit.EventHandler;

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.DocumentCreated" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="Autodesk.Revit.DB.Events.DocumentCreatedEventArgs" /> and the newly created
///         <see cref="Autodesk.Revit.DB.Document" />.
///     </para>
/// </remarks>
[PublicAPI]
public class RevitDocumentCreatedHandler : RevitAppPostDocumentEventHandler<DocumentCreatedEventArgs>
{
    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.DocumentCreated" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    public RevitDocumentCreatedHandler(ControlledApplication application)
        : base(application)
    {
        Subscribe();
    }

    /// <inheritdoc />
    protected sealed override void Subscribe()
    {
        Application.DocumentCreated += HandleEvent;
    }

    /// <inheritdoc />
    protected sealed override void Unsubscribe()
    {
        Application.DocumentCreated -= HandleEvent;
    }
}