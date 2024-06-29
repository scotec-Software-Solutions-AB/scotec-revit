// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit.Test;

[RevitDbApplicationIsolation]
public class DbApp : IExternalDBApplication
{
    public ExternalDBApplicationResult OnStartup(ControlledApplication application)
    {
        throw new NotImplementedException();
    }

    public ExternalDBApplicationResult OnShutdown(ControlledApplication application)
    {
        throw new NotImplementedException();
    }
}

[RevitApplicationIsolation]
public class RevitTestApp : RevitApp
{
    public RevitTestApp()
    {
        var context = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
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

            var button = (PushButton)panel.AddItem(CreateButtonData("Revit.Test",
                "Test", "Test",
                typeof(RevitTestCommandFactory)));

            button.Enabled = true;
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
            AvailabilityClassName = typeof(RevitTestCommandAvailabilityFactory).FullName
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
