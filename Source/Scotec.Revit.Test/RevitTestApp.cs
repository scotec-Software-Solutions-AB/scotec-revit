using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Autodesk.Revit.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit.Test
{
    [RevitApp]
    public class RevitTestApp : RevitApp
    {
        public RevitTestApp()
        {
            
        }
        protected override Result OnShutdown()
        {
            return Result.Succeeded;
        }

        protected override Result OnStartup()
        {
            try
            {
                var config = Services.GetService<IConfiguration>();
                TabManager.CreateTab(Application, "scotec");
                var panel = TabManager.GetPanel(Application, "Test", "scotec");

                //var button = (PushButton)panel.AddItem(CreateButtonData("Revit.Test",
                //    "Test", "Test",
                //    typeof(RevitTestCommandFactory)));

                //button.Enabled = true;

            }
            catch (Exception)
            {
                return Result.Failed;
            }

            return Result.Succeeded;
        }

        private static PushButtonData CreateButtonData(string name, string text, string description, Type commandType)
        {
            return new PushButtonData(name, text,
                Assembly.GetExecutingAssembly().Location, commandType.FullName)
            {
                Image = CreateImageSource("Information_16.png"),
                LargeImage = CreateImageSource("Information_32.png"),
                ToolTip = description,
                //AvailabilityClassName = typeof(TestCommandAvailability).FullName
            };
        }

        private static ImageSource CreateImageSource(string image)
        {
            var resourcePath = $"Revit.Tutorial.Resources.Images.{image}";

            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
            if (stream == null)
            {
                return null;
            }

            var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

            return decoder.Frames[0];
        }

    }

}
