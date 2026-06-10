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
///     Handles the Revit <see cref="ControlledApplication.FailuresProcessing" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="FailuresProcessingEventArgs" />.
///         Use <see cref="FailuresProcessingEventArgs.GetFailuresAccessor" /> to inspect and resolve failures.
///     </para>
/// </remarks>
public abstract class RevitFailuresProcessingHandler : RevitEventHandler<FailuresProcessingEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.FailuresProcessing" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitFailuresProcessingHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.FailuresProcessing += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.FailuresProcessing -= HandleEvent;
    }
}

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.FileExported" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="FileExportedEventArgs" /> and the
///         <see cref="Document" /> from which the export was performed.
///     </para>
/// </remarks>
public abstract class RevitFileExportedHandler : RevitEventHandler<FileExportedEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.FileExported" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitFileExportedHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.FileExported += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.FileExported -= HandleEvent;
    }

    /// <inheritdoc />
    protected override void RegisterEventContext(IServiceCollection services, object sender, FileExportedEventArgs args)
    {
        var document = args.Document;

        if (document is not null)
        {
            services.AddSingleton(document);
        }
    }
}

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.FileExporting" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="FileExportingEventArgs" /> and the
///         <see cref="Document" /> being exported.
///     </para>
/// </remarks>
public abstract class RevitFileExportingHandler : RevitEventHandler<FileExportingEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.FileExporting" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitFileExportingHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.FileExporting += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.FileExporting -= HandleEvent;
    }

    /// <inheritdoc />
    protected override void RegisterEventContext(IServiceCollection services, object sender, FileExportingEventArgs args)
    {
        var document = args.Document;

        if (document is not null)
        {
            services.AddSingleton(document);
        }
    }
}

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.FileImported" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="FileImportedEventArgs" /> and the
///         <see cref="Document" /> into which the import was performed.
///     </para>
/// </remarks>
public abstract class RevitFileImportedHandler : RevitEventHandler<FileImportedEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.FileImported" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitFileImportedHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.FileImported += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.FileImported -= HandleEvent;
    }

    /// <inheritdoc />
    protected override void RegisterEventContext(IServiceCollection services, object sender, FileImportedEventArgs args)
    {
        var document = args.Document;

        if (document is not null)
        {
            services.AddSingleton(document);
        }
    }
}

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.FileImporting" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="FileImportingEventArgs" /> and the
///         <see cref="Document" /> being imported into.
///     </para>
/// </remarks>
public abstract class RevitFileImportingHandler : RevitEventHandler<FileImportingEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.FileImporting" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitFileImportingHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.FileImporting += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.FileImporting -= HandleEvent;
    }

    /// <inheritdoc />
    protected override void RegisterEventContext(IServiceCollection services, object sender, FileImportingEventArgs args)
    {
        var document = args.Document;

        if (document is not null)
        {
            services.AddSingleton(document);
        }
    }
}

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.LinkedResourceOpened" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="LinkedResourceOpenedEventArgs" />.
///     </para>
/// </remarks>
public abstract class RevitLinkedResourceOpenedHandler : RevitEventHandler<LinkedResourceOpenedEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.LinkedResourceOpened" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitLinkedResourceOpenedHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.LinkedResourceOpened += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.LinkedResourceOpened -= HandleEvent;
    }
}

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.LinkedResourceOpening" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="LinkedResourceOpeningEventArgs" />.
///     </para>
/// </remarks>
public abstract class RevitLinkedResourceOpeningHandler : RevitEventHandler<LinkedResourceOpeningEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.LinkedResourceOpening" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitLinkedResourceOpeningHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.LinkedResourceOpening += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.LinkedResourceOpening -= HandleEvent;
    }
}

/// <summary>
///     Handles the Revit <see cref="ControlledApplication.ProgressChanged" /> event.
/// </summary>
/// <remarks>
///     Available when the application is registered as either <c>IExternalApplication</c> (via <see cref="RevitApp" />)
///     or <c>IExternalDBApplication</c> (via <see cref="RevitDbApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="ProgressChangedEventArgs" />.
///     </para>
///     <para>
///         <strong>Performance note:</strong> this event fires very frequently during long operations.
///         Avoid expensive work inside this handler.
///     </para>
/// </remarks>
public abstract class RevitProgressChangedHandler : RevitEventHandler<ProgressChangedEventArgs>
{
    private readonly ControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="ControlledApplication.ProgressChanged" />.
    /// </summary>
    /// <param name="application">The Revit controlled application.</param>
    protected RevitProgressChangedHandler(ControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.ProgressChanged += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.ProgressChanged -= HandleEvent;
    }
}
