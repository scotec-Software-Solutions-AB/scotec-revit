// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit;

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.DocumentOpened" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="DocumentOpenedEventArgs" /> and the opened
///         <see cref="Document" />.
///     </para>
/// </remarks>
public abstract class RevitDocumentOpenedHandler : RevitEventHandler<DocumentOpenedEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.DocumentOpened" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitDocumentOpenedHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.DocumentOpened += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.DocumentOpened -= HandleEvent;
    }

    /// <inheritdoc />
    protected override void RegisterEventContext(IServiceCollection services, object sender, DocumentOpenedEventArgs args)
    {
        var document = args.Document;

        if (document is not null)
        {
            services.AddSingleton(document);
        }
    }
}

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.DocumentOpening" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         No <see cref="Document" /> is available at this stage; the document has not yet been created.
///         Use <see cref="DocumentOpeningEventArgs.PathName" /> to identify the file being opened.
///     </para>
/// </remarks>
public abstract class RevitDocumentOpeningHandler : RevitEventHandler<DocumentOpeningEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.DocumentOpening" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitDocumentOpeningHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.DocumentOpening += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.DocumentOpening -= HandleEvent;
    }
}

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.DocumentClosing" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="DocumentClosingEventArgs" /> and the
///         <see cref="Document" /> that is about to be closed.
///     </para>
/// </remarks>
public abstract class RevitDocumentClosingHandler : RevitEventHandler<DocumentClosingEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.DocumentClosing" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitDocumentClosingHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.DocumentClosing += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.DocumentClosing -= HandleEvent;
    }

    /// <inheritdoc />
    protected override void RegisterEventContext(IServiceCollection services, object sender, DocumentClosingEventArgs args)
    {
        var document = args.Document;

        if (document is not null)
        {
            services.AddSingleton(document);
        }
    }
}

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
public abstract class RevitDocumentClosedHandler : RevitEventHandler<DocumentClosedEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.DocumentClosed" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitDocumentClosedHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.DocumentClosed += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.DocumentClosed -= HandleEvent;
    }
}

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.DocumentCreated" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="DocumentCreatedEventArgs" /> and the newly created
///         <see cref="Document" />.
///     </para>
/// </remarks>
public abstract class RevitDocumentCreatedHandler : RevitEventHandler<DocumentCreatedEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.DocumentCreated" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitDocumentCreatedHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.DocumentCreated += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.DocumentCreated -= HandleEvent;
    }

    /// <inheritdoc />
    protected override void RegisterEventContext(IServiceCollection services, object sender, DocumentCreatedEventArgs args)
    {
        var document = args.Document;

        if (document is not null)
        {
            services.AddSingleton(document);
        }
    }
}

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.DocumentCreating" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         No <see cref="Document" /> exists yet at this stage.
///     </para>
/// </remarks>
public abstract class RevitDocumentCreatingHandler : RevitEventHandler<DocumentCreatingEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.DocumentCreating" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitDocumentCreatingHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.DocumentCreating += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.DocumentCreating -= HandleEvent;
    }
}

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.DocumentSaving" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="DocumentSavingEventArgs" /> and the
///         <see cref="Document" /> being saved.
///     </para>
/// </remarks>
public abstract class RevitDocumentSavingHandler : RevitEventHandler<DocumentSavingEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.DocumentSaving" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitDocumentSavingHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.DocumentSaving += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.DocumentSaving -= HandleEvent;
    }

    /// <inheritdoc />
    protected override void RegisterEventContext(IServiceCollection services, object sender, DocumentSavingEventArgs args)
    {
        var document = args.Document;

        if (document is not null)
        {
            services.AddSingleton(document);
        }
    }
}

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.DocumentSaved" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="DocumentSavedEventArgs" /> and the
///         <see cref="Document" /> that was saved.
///     </para>
/// </remarks>
public abstract class RevitDocumentSavedHandler : RevitEventHandler<DocumentSavedEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.DocumentSaved" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitDocumentSavedHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.DocumentSaved += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.DocumentSaved -= HandleEvent;
    }

    /// <inheritdoc />
    protected override void RegisterEventContext(IServiceCollection services, object sender, DocumentSavedEventArgs args)
    {
        var document = args.Document;

        if (document is not null)
        {
            services.AddSingleton(document);
        }
    }
}

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.DocumentSavingAs" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="DocumentSavingAsEventArgs" /> and the
///         <see cref="Document" /> being saved to a new path.
///     </para>
/// </remarks>
public abstract class RevitDocumentSavingAsHandler : RevitEventHandler<DocumentSavingAsEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.DocumentSavingAs" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitDocumentSavingAsHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.DocumentSavingAs += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.DocumentSavingAs -= HandleEvent;
    }

    /// <inheritdoc />
    protected override void RegisterEventContext(IServiceCollection services, object sender, DocumentSavingAsEventArgs args)
    {
        var document = args.Document;

        if (document is not null)
        {
            services.AddSingleton(document);
        }
    }
}

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.DocumentSavedAs" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="DocumentSavedAsEventArgs" /> and the
///         <see cref="Document" /> that was saved to a new path.
///     </para>
/// </remarks>
public abstract class RevitDocumentSavedAsHandler : RevitEventHandler<DocumentSavedAsEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.DocumentSavedAs" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitDocumentSavedAsHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.DocumentSavedAs += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.DocumentSavedAs -= HandleEvent;
    }

    /// <inheritdoc />
    protected override void RegisterEventContext(IServiceCollection services, object sender, DocumentSavedAsEventArgs args)
    {
        var document = args.Document;

        if (document is not null)
        {
            services.AddSingleton(document);
        }
    }
}

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.DocumentChanged" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="DocumentChangedEventArgs" /> and the
///         modified <see cref="Document" />.
///     </para>
///     <para>
///         <strong>Performance note:</strong> this event fires very frequently (after every successful transaction).
///         Avoid expensive operations and long-running services inside this handler.
///     </para>
/// </remarks>
public abstract class RevitDocumentChangedHandler : RevitEventHandler<DocumentChangedEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.DocumentChanged" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitDocumentChangedHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.DocumentChanged += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.DocumentChanged -= HandleEvent;
    }

    /// <inheritdoc />
    protected override void RegisterEventContext(IServiceCollection services, object sender, DocumentChangedEventArgs args)
    {
        var document = args.GetDocument();

        if (document is not null)
        {
            services.AddSingleton(document);
        }
    }
}

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
public abstract class RevitDocumentPrintingHandler : RevitEventHandler<DocumentPrintingEventArgs>
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
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.DocumentPrinting += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.DocumentPrinting -= HandleEvent;
    }

    /// <inheritdoc />
    protected override void RegisterEventContext(IServiceCollection services, object sender, DocumentPrintingEventArgs args)
    {
        var document = args.Document;

        if (document is not null)
        {
            services.AddSingleton(document);
        }
    }
}

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.DocumentPrinted" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="DocumentPrintedEventArgs" /> and the
///         <see cref="Document" /> that was printed.
///     </para>
/// </remarks>
public abstract class RevitDocumentPrintedHandler : RevitEventHandler<DocumentPrintedEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.DocumentPrinted" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitDocumentPrintedHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.DocumentPrinted += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.DocumentPrinted -= HandleEvent;
    }

    /// <inheritdoc />
    protected override void RegisterEventContext(IServiceCollection services, object sender, DocumentPrintedEventArgs args)
    {
        var document = args.Document;

        if (document is not null)
        {
            services.AddSingleton(document);
        }
    }
}

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
public abstract class RevitDocumentSynchronizingWithCentralHandler : RevitEventHandler<DocumentSynchronizingWithCentralEventArgs>
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
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.DocumentSynchronizingWithCentral += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
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
public abstract class RevitDocumentSynchronizedWithCentralHandler : RevitEventHandler<DocumentSynchronizedWithCentralEventArgs>
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
