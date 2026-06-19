// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using JetBrains.Annotations;

namespace Scotec.Revit.EventHandler;

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.DocumentSynchronizingWithCentral" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="DocumentSynchronizingWithCentralEventArgs" /> and the
///         <see cref="Document" /> being synchronized.
///     </para>
/// </remarks>
[PublicAPI]
public abstract class RevitDocumentSynchronizingWithCentralHandler : RevitAppPreDocumentEventHandler<DocumentSynchronizingWithCentralEventArgs>
{

    /// <summary>
    ///     Initializes a new instance and subscribes to
    ///     <see cref="ControlledApplication.DocumentSynchronizingWithCentral" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitDocumentSynchronizingWithCentralHandler(ControlledApplication application)
        : base(application)
    {
        Subscribe();
    }

    /// <inheritdoc />
    protected sealed override void Subscribe()
    {
        Application.DocumentSynchronizingWithCentral += HandleEvent;
    }

    /// <inheritdoc />
    protected sealed override void Unsubscribe()
    {
        Application.DocumentSynchronizingWithCentral -= HandleEvent;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Creates an <see cref="IRevitContext" /> from <see cref="Autodesk.Revit.DB.Events.DocumentSynchronizingWithCentralEventArgs.Document" />
    ///     when a document is available.
    /// </remarks>
    protected override IRevitContext? CreateContext(Application? sender, DocumentSynchronizingWithCentralEventArgs args)
    {
        return args.Document is not null ? new RevitContext(args.Document) : null;
    }
}
