// Copyright (c) 2023 - 2026 Olaf Meyer
// Copyright (c) 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autofac;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

namespace Scotec.Revit;

/// <summary>
///     Handles the Revit <see cref="UIControlledApplication.ViewActivated" /> event.
/// </summary>
/// <remarks>
///     Only available when the application is registered as <c>IExternalApplication</c> (via <see cref="RevitApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="ViewActivatedEventArgs" />, the
///         <see cref="UIApplication" />, the active <see cref="UIDocument" />, the active <see cref="Document" />,
///         and the newly activated <see cref="View" />.
///     </para>
/// </remarks>
public abstract class RevitViewActivatedHandler : RevitEventHandler<ViewActivatedEventArgs>
{
    private readonly UIControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="UIControlledApplication.ViewActivated" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application.</param>
    protected RevitViewActivatedHandler(UIControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.ViewActivated += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.ViewActivated -= HandleEvent;
    }

    /// <inheritdoc />
    protected override void RegisterEventContext(ContainerBuilder builder, object sender, ViewActivatedEventArgs args)
    {
        if (sender is UIApplication uiApplication)
        {
            builder.RegisterInstance(uiApplication).ExternallyOwned();

            var uiDocument = uiApplication.ActiveUIDocument;

            if (uiDocument is not null)
            {
                builder.RegisterInstance(uiDocument).ExternallyOwned();

                var document = uiDocument.Document;

                if (document is not null)
                {
                    builder.RegisterInstance(document).ExternallyOwned();
                }
            }
        }

        var view = args.CurrentActiveView;

        if (view is not null)
        {
            builder.RegisterInstance(view).ExternallyOwned();
        }
    }
}

/// <summary>
///     Handles the Revit <see cref="UIControlledApplication.Idling" /> event.
/// </summary>
/// <remarks>
///     Only available when the application is registered as <c>IExternalApplication</c> (via <see cref="RevitApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="IdlingEventArgs" /> and the
///         <see cref="UIApplication" /> sender.
///     </para>
///     <para>
///         <strong>Performance note:</strong> this event fires repeatedly during idle periods.
///         Avoid blocking work. Use <see cref="IdlingEventArgs.SetRaiseWithoutDelay" /> only when continuous
///         polling is truly required, and revert to the default behavior as soon as possible.
///     </para>
/// </remarks>
public abstract class RevitIdlingHandler : RevitEventHandler<IdlingEventArgs>
{
    private readonly UIControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="UIControlledApplication.Idling" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application.</param>
    protected RevitIdlingHandler(UIControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.Idling += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.Idling -= HandleEvent;
    }

    /// <inheritdoc />
    protected override void RegisterEventContext(ContainerBuilder builder, object sender, IdlingEventArgs args)
    {
        if (sender is UIApplication uiApplication)
        {
            builder.RegisterInstance(uiApplication).ExternallyOwned();
        }
    }
}

/// <summary>
///     Handles the Revit <see cref="UIControlledApplication.ApplicationClosing" /> event.
/// </summary>
/// <remarks>
///     Only available when the application is registered as <c>IExternalApplication</c> (via <see cref="RevitApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="ApplicationClosingEventArgs" />.
///     </para>
/// </remarks>
public abstract class RevitApplicationClosingHandler : RevitEventHandler<ApplicationClosingEventArgs>
{
    private readonly UIControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="UIControlledApplication.ApplicationClosing" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application.</param>
    protected RevitApplicationClosingHandler(UIControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.ApplicationClosing += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.ApplicationClosing -= HandleEvent;
    }
}

/// <summary>
///     Handles the Revit <see cref="UIControlledApplication.DialogBoxShowing" /> event.
/// </summary>
/// <remarks>
///     Only available when the application is registered as <c>IExternalApplication</c> (via <see cref="RevitApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="DialogBoxShowingEventArgs" />.
///         Use the event args to suppress or override specific Revit dialog boxes.
///     </para>
/// </remarks>
public abstract class RevitDialogBoxShowingHandler : RevitEventHandler<DialogBoxShowingEventArgs>
{
    private readonly UIControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="UIControlledApplication.DialogBoxShowing" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application.</param>
    protected RevitDialogBoxShowingHandler(UIControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.DialogBoxShowing += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.DialogBoxShowing -= HandleEvent;
    }
}

/// <summary>
///     Handles the Revit <see cref="UIControlledApplication.DisplayingOptionsDialog" /> event.
/// </summary>
/// <remarks>
///     Only available when the application is registered as <c>IExternalApplication</c> (via <see cref="RevitApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="DisplayingOptionsDialogEventArgs" />.
///         Use the event args to add custom tabs to the Revit Options dialog.
///     </para>
/// </remarks>
public abstract class RevitDisplayingOptionsDialogHandler : RevitEventHandler<DisplayingOptionsDialogEventArgs>
{
    private readonly UIControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="UIControlledApplication.DisplayingOptionsDialog" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application.</param>
    protected RevitDisplayingOptionsDialogHandler(UIControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.DisplayingOptionsDialog += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.DisplayingOptionsDialog -= HandleEvent;
    }
}

/// <summary>
///     Handles the Revit <see cref="UIControlledApplication.FabricationPartBrowserChanged" /> event.
/// </summary>
/// <remarks>
///     Only available when the application is registered as <c>IExternalApplication</c> (via <see cref="RevitApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="FabricationPartBrowserChangedEventArgs" />.
///     </para>
/// </remarks>
public abstract class RevitFabricationPartBrowserChangedHandler : RevitEventHandler<FabricationPartBrowserChangedEventArgs>
{
    private readonly UIControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="UIControlledApplication.FabricationPartBrowserChanged" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application.</param>
    protected RevitFabricationPartBrowserChangedHandler(UIControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.FabricationPartBrowserChanged += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.FabricationPartBrowserChanged -= HandleEvent;
    }
}

/// <summary>
///     Handles the Revit <see cref="UIControlledApplication.FormulaEditing" /> event.
/// </summary>
/// <remarks>
///     Only available when the application is registered as <c>IExternalApplication</c> (via <see cref="RevitApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="FormulaEditingEventArgs" />.
///     </para>
/// </remarks>
public abstract class RevitFormulaEditingHandler : RevitEventHandler<FormulaEditingEventArgs>
{
    private readonly UIControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="UIControlledApplication.FormulaEditing" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application.</param>
    protected RevitFormulaEditingHandler(UIControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.FormulaEditing += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.FormulaEditing -= HandleEvent;
    }
}

/// <summary>
///     Handles the Revit <see cref="UIControlledApplication.TransferredProjectStandards" /> event.
/// </summary>
/// <remarks>
///     Only available when the application is registered as <c>IExternalApplication</c> (via <see cref="RevitApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="TransferredProjectStandardsEventArgs" />.
///     </para>
/// </remarks>
public abstract class RevitTransferredProjectStandardsHandler : RevitEventHandler<TransferredProjectStandardsEventArgs>
{
    private readonly UIControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="UIControlledApplication.TransferredProjectStandards" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application.</param>
    protected RevitTransferredProjectStandardsHandler(UIControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.TransferredProjectStandards += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.TransferredProjectStandards -= HandleEvent;
    }
}

/// <summary>
///     Handles the Revit <see cref="UIControlledApplication.TransferringProjectStandards" /> event.
/// </summary>
/// <remarks>
///     Only available when the application is registered as <c>IExternalApplication</c> (via <see cref="RevitApp" />).
///     <para>
///         The per-invocation DI scope registers <see cref="TransferringProjectStandardsEventArgs" />.
///     </para>
/// </remarks>
public abstract class RevitTransferringProjectStandardsHandler : RevitEventHandler<TransferringProjectStandardsEventArgs>
{
    private readonly UIControlledApplication _application;

    /// <summary>
    ///     Initializes a new instance and subscribes to <see cref="UIControlledApplication.TransferringProjectStandards" />.
    /// </summary>
    /// <param name="application">The Revit UI controlled application.</param>
    protected RevitTransferringProjectStandardsHandler(UIControlledApplication application)
        : base(application.ActiveAddInId.GetGUID())
    {
        _application = application;
    }

    /// <inheritdoc />
    protected override void Subscribe()
    {
        _application.TransferringProjectStandards += HandleEvent;
    }

    /// <inheritdoc />
    protected override void Unsubscribe()
    {
        _application.TransferringProjectStandards -= HandleEvent;
    }
}
