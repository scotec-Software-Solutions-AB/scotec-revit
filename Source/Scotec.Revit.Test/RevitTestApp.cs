// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scotec.Revit.DynamicCommands;
using Scotec.Revit.Isolation;
using Scotec.Revit.Ui;

namespace Scotec.Revit.Test;

[Isolation.RevitDbApplicationIsolation]
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

[Isolation.RevitApplicationIsolation]
public class RevitTestApp : RevitApp
{
    public RevitTestApp()
    {
        var context = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
    }

    protected override bool OnShutdown()
    {
        return true;
    }

    protected override bool OnStartup()
    {

        Assembly assembly = null;
        try
        {
            var loadContext = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
            var generator = new RevitDynamicActionCommandGenerator("TestCommands", loadContext);
            generator.GenerateActionCommandType("TestCommands.TestCommand1", (data, provider) =>
            {
                Debugger.Launch();
            });
            assembly = generator.FinalizeAssembly(@"C:\Temp\TestCommands.dll");
        }
        catch (Exception e)
        {
            Debugger.Launch();
            throw;
        }

        try
        {
           //var types = assembly.GetTypes();
        }
        catch (Exception e)
        {
            Debugger.Launch();
            throw;
        }
        try
        {
            var config = Services.GetService<IConfiguration>();
            TabManager.CreateTab(Application, "scotec");
            var panel = TabManager.GetPanel(Application, "Test", "scotec");

            //var button = (PushButton)panel.AddItem(CreateButtonData("Revit.Test",
            //    "Test", "Test",
            //    typeof(RevitTestCommandFactory)));

            //button.Enabled = true;
            var pushButtonData = new PushButtonData("Test", "Test",
                @"C:\Temp\TestCommands.dll",
                "TestCommands.TestCommand1")
            {
                Image = CreateImageSource("Information_16.png"),
                LargeImage = CreateImageSource("Information_32.png"),
                ToolTip = "Tooltip"
                //, AvailabilityClassName = typeof(RevitTestCommandAvailabilityFactory).FullName
            };

            panel.AddItem(pushButtonData);
        }
        catch (Exception)
        {
            Debugger.Launch();
            return false;
        }

        return true;
    }

    //private static PushButtonData CreateButtonData(string name, string text, string description, Type commandType)
    //{
    //    return new PushButtonData(name, text,
    //        Assembly.GetExecutingAssembly().Location, commandType.FullName)
    //    {
    //        Image = CreateImageSource("Information_16.png"),
    //        LargeImage = CreateImageSource("Information_32.png"),
    //        ToolTip = description,
    //        AvailabilityClassName = typeof(RevitTestCommandAvailabilityFactory).FullName
    //    };
    //}

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
