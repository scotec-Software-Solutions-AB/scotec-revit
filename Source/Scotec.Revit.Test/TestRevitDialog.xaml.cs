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

        public TestRevitDialog(UIApplication revitApp, RevitTask revitTask) : base(revitApp)
        {
            _revitTask = revitTask;
            InitializeComponent();
        }

        private async void DialogButton_Click(object sender, RoutedEventArgs e)
        {
            var result = await _revitTask.Run(uiApp =>
            {
                TaskDialog.Show("Revit Task", "This is a message from the Revit task!");
                return true;
            });
            await _revitTask.Run((Document document) =>
            {
                TaskDialog.Show("Revit Task", "This is a message from the Revit task!");
            });
            await _revitTask.Run(MyRevitTask);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Register additional services if needed.
        }

        private void MyRevitTask(Document document)
        {
            TaskDialog.Show("Revit Task", "This is a message from the Revit task!");
        }
    }
}
