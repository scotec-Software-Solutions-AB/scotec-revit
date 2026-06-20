// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Autodesk.Revit.UI;
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

    private void ConfigureServices(IServiceCollection services)
    {
        // Register additional services if needed.
    }

    private void MyRevitTask(IRevitUiContext context)
    {
        TaskDialog.Show("Revit Task 3", "This is a message from the Revit task!");
    }
}
