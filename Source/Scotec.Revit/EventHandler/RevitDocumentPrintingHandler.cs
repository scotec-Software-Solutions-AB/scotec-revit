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
///     Handles the Revit <see cref="ControlledApplication.DocumentPrinting" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="DocumentPrintingEventArgs" /> and the
///         <see cref="Document" /> being printed.
///     </para>
/// </remarks>
public abstract class RevitDocumentPrintingHandler : RevitPreDocumentEventHandler<Application, DocumentPrintingEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.DocumentPrinting" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitDocumentPrintingHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
        Subscribe();
    }

    /// <inheritdoc />
    protected sealed override void Subscribe()
    {
        _application.DocumentPrinting += HandleEvent;
    }

    /// <inheritdoc />
    protected sealed override void Unsubscribe()
    {
        _application.DocumentPrinting -= HandleEvent;
    }
}
