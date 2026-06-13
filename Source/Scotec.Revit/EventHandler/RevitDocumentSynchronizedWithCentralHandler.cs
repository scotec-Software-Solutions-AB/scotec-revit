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
///     Handles the Revit <see cref="ControlledApplication.DocumentSynchronizedWithCentral" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="DocumentSynchronizedWithCentralEventArgs" /> and the
///         <see cref="Document" /> that was synchronized.
///     </para>
/// </remarks>
public abstract class RevitDocumentSynchronizedWithCentralHandler : RevitPostDocumentEventHandler<DocumentSynchronizedWithCentralEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to
    ///     <see cref="ControlledApplication.DocumentSynchronizedWithCentral" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitDocumentSynchronizedWithCentralHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.DocumentSynchronizedWithCentral += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.DocumentSynchronizedWithCentral -= HandleEvent;
    }

    /// <inheritdoc />
    protected override void RegisterEventContext(IServiceCollection services, object sender, DocumentSynchronizedWithCentralEventArgs args)
    {
        var document = args.Document;

        if (document is not null)
        {
            services.AddSingleton(document);
        }
    }
}
