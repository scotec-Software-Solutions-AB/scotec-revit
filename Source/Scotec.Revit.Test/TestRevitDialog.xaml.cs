// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit.Test;

[SuppressMessage("ReSharper", "AsyncVoidEventHandlerMethod")]
public partial class TestRevitDialog
{
    private readonly IGlobalRevitUiContext _globalContext;
    private readonly RevitTask _revitTask;

    public TestRevitDialog(IGlobalRevitUiContext globalContext, RevitTask revitTask) : base(globalContext.UiApplication)
    {
        _globalContext = globalContext;
        _revitTask = revitTask;
        InitializeComponent();

        // Revit API: SelectionChanged is raised on the UIApplication whenever the active
        // selection in the current document changes. Subscribe here and unsubscribe in
        // the Closed handler to match the dialog lifetime.
        _globalContext.UiApplication.SelectionChanged += OnSelectionChanged;
        Closed += OnClosed;
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

    private async void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        await _revitTask.Run(context =>
        {
            // Revit API: SelectionChangedEventArgs.GetSelectedIds() returns the current
            // element selection. Access is valid only on the Revit API thread.
            var selectedIds = e.GetSelectedIds();
            TaskDialog.Show("Selection Changed", $"Selected element count: {selectedIds.Count}");
        });
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        // Revit API: Always unsubscribe from UIApplication events when the subscribing
        // object is no longer active to avoid memory leaks and stale callbacks.
        _globalContext.UiApplication.SelectionChanged -= OnSelectionChanged;
        Closed -= OnClosed;
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
