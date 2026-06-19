// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit.EventHandler;

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.DocumentChanged" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="Autodesk.Revit.DB.Events.DocumentChangedEventArgs" /> and the
///         modified <see cref="Document" />.
///     </para>
///     <para>
///         <strong>Performance note:</strong> this event fires very frequently (after every successful transaction).
///         Avoid expensive operations and long-running services inside this handler.
///     </para>
/// </remarks>
[PublicAPI]
public class RevitDocumentChangedHandler : RevitAppSingleEventHandler<DocumentChangedEventArgs>
{

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.DocumentChanged" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    public RevitDocumentChangedHandler(ControlledApplication application)
        : base(application)
    {
        Subscribe();
    }

    /// <inheritdoc />
    protected sealed override void Subscribe()
    {
        Application.DocumentChanged += HandleEvent;
    }

    /// <inheritdoc />
    protected sealed override void Unsubscribe()
    {
        Application.DocumentChanged -= HandleEvent;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Creates an <see cref="IRevitContext" /> from the <see cref="Autodesk.Revit.DB.Events.DocumentChangedEventArgs.GetDocument" /> result
    ///     when a document is available.
    /// </remarks>
    protected sealed override IRevitContext? CreateContext(Application? sender, DocumentChangedEventArgs args)
    {
        var document = args.GetDocument();
        return document is not null ? new RevitContext(document) : base.CreateContext(sender, args);
    }
}
