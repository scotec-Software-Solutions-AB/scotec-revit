// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Microsoft.Extensions.DependencyInjection;
using Scotec.Revit.EventHandler;

namespace Scotec.Revit.Test;

[SuppressMessage("ReSharper", "AsyncVoidEventHandlerMethod")]
public partial class TestRevitDialog
{
    private readonly IGlobalRevitUiContext _globalContext;
    private readonly RevitTask _revitTask;
    private readonly RevitSelectionChangedHandler _selectionChangedHandler;

    public TestRevitDialog(
        IGlobalRevitUiContext globalContext,
        RevitTask revitTask,
        UIControlledApplication application) : base(globalContext.UiApplication)
    {
        _globalContext = globalContext;
        _revitTask = revitTask;
        InitializeComponent();

        // Scoped to this dialog: the handler is active only while the window is open.
        // Disposing it in OnClosed unsubscribes from SelectionChanged automatically.
        _selectionChangedHandler = new RevitSelectionChangedHandler(application);
        _selectionChangedHandler.AddHandler((IRevitUiContext context, SelectionChangedEventArgs args) =>
        {
            // Revit API: GetSelectedElements() is valid only on the Revit API thread,
            // which is guaranteed here because the handler fires on the Revit event thread.
            var selectedIds = args.GetSelectedElements();
            TaskDialog.Show("Selection Changed", $"Selected element count: {selectedIds.Count}");
        });

        Closed += async (_,_) => await _revitTask.Run(_ => OnClosed());
    }


    private async void DialogButton_Click(object sender, RoutedEventArgs e)
    {
        var result = await _revitTask.Run(context =>
        {
            TaskDialog.Show("Revit Task 1", "This is a message from the Revit task!");
            return true;
        });
        await _revitTask.Run(context => { TaskDialog.Show("Revit Task 2", "This is a message from the Revit task!"); });
        await _revitTask.Run(MyRevitTask);
    }

    private void OnClosed()
    {
        // Disposing the handler unsubscribes from UIControlledApplication.SelectionChanged.
        _selectionChangedHandler.Dispose();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Register additional services if needed.
    }

    private void MyRevitTask(IRevitUiContext context)
    {
        TaskDialog.Show("Revit Task 3", "This is a message from the Revit task!");
    }
}
