// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Microsoft.Extensions.DependencyInjection;

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
public abstract class RevitDocumentSynchronizingWithCentralHandler : RevitPreDocumentEventHandler<DocumentSynchronizingWithCentralEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to
    ///     <see cref="ControlledApplication.DocumentSynchronizingWithCentral" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitDocumentSynchronizingWithCentralHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
        Subscribe();
    }

    /// <inheritdoc />
    protected sealed override void Subscribe()
    {
        _application.DocumentSynchronizingWithCentral += HandleEvent;
    }

    /// <inheritdoc />
    protected sealed override void Unsubscribe()
    {
        _application.DocumentSynchronizingWithCentral -= HandleEvent;
    }

    /// <inheritdoc />
    protected override void RegisterEventContext(IServiceCollection services, object sender, DocumentSynchronizingWithCentralEventArgs args)
    {
        var document = args.Document;

        if (document is not null)
        {
            services.AddSingleton(document);
        }
    }
}
