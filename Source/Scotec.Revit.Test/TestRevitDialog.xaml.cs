using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit.Test
{
    [SuppressMessage("ReSharper", "AsyncVoidEventHandlerMethod")]
    public partial class TestRevitDialog
    {
        private readonly RevitTask _revitTask;

        public TestRevitDialog(IRevitUiContext context, RevitTask revitTask) : base(context.UiApplication)
        {
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
            await _revitTask.Run((context) =>
            {
                TaskDialog.Show("Revit Task 2", "This is a message from the Revit task!");
            });
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
}
