// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scotec.Revit.Isolation;
using Scotec.Revit.Ui;
using Scotec.Revit.Ui.DynamicCommands;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Runtime.Loader;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JetBrains.Annotations;
<<<<<<< HEAD
using Microsoft.Extensions.Hosting;
=======
>>>>>>> origin/main

[assembly: RevitAddinIsolationContext(ContextName = "Scotec.Revit.Test")]


namespace Scotec.Revit.Test;

[RevitDbApplicationIsolation(ContextName = "Scotec.Revit.Test")]
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

[RevitApplicationIsolation(ContextName = "Scotec.Revit.Test")]
public class RevitTestApp : RevitApp
{
    protected override bool OnStartup()
    {
        //Assembly assembly = null;
        //try
        //{
        //    var loadContext = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
        //    var generator = new RevitDynamicActionCommandGenerator("TestCommands", loadContext);
        //    generator.GenerateActionCommandType("TestCommands.TestCommand1", (data, provider) =>
        //    {
        //        Debugger.Launch();
        //    });
        //    assembly = generator.FinalizeAssembly(@"C:\Temp\TestCommands.dll");
        //}
        //catch (Exception e)
        //{
        //    Debugger.Launch();
        //    throw;
        //}

        try
        {
            var config = Services.GetService<IConfiguration>();
            RevitTabManager.CreateTab(Application, "scotec");
            var panel = RevitTabManager.GetPanel(Application, "Test", "scotec");

            panel.AddItem(CreateTestButtonData());
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
<<<<<<< HEAD

    protected override bool OnStartup(UIControlledApplication application)
    {
        try
        {
            var config = Services.GetService<IConfiguration>();
            RevitTabManager.CreateTab(Application, "scotec");
            var panel = RevitTabManager.GetPanel(Application, "Test", "scotec");

            panel.AddItem(CreateTestButtonData());
        }
        catch (Exception)
        {
            Debugger.Launch();
            return false;
        }

        return base.OnStartup(application);
    }

    [RevitApplicationStartup]
    [UsedImplicitly]
    private bool OnStartup(IConfiguration configuration)
    {
        try
        {
            RevitTabManager.CreateTab(Application, "scotec");
            var panel = RevitTabManager.GetPanel(Application, "Test", "scotec");

            panel.AddItem(CreateTestButtonData());
            panel.AddItem(CreateShowDialogButtonData());

=======

    protected override bool OnStartup(UIControlledApplication application)
    {
        try
        {
            var config = Services.GetService<IConfiguration>();
            RevitTabManager.CreateTab(Application, "scotec");
            var panel = RevitTabManager.GetPanel(Application, "Test", "scotec");

            panel.AddItem(CreateTestButtonData());
        }
        catch (Exception)
        {
            Debugger.Launch();
            return false;
        }

        return base.OnStartup(application);
    }

    [RevitStartup]
    [UsedImplicitly]
    private bool OnStartup(IConfiguration configuration)
    {
        try
        {
            RevitTabManager.CreateTab(Application, "scotec");
            var panel = RevitTabManager.GetPanel(Application, "Test", "scotec");

            panel.AddItem(CreateTestButtonData());
>>>>>>> origin/main
        }
        catch (Exception)
        {
            Debugger.Launch();
            return false;
        }

        return true;
    }

<<<<<<< HEAD
    protected override void OnConfigure(IHostBuilder builder)
    {
        base.OnConfigure(builder);

        builder.ConfigureServices((context, services) =>
        {
            services.AddScoped<TestRevitDialog>();
            services.AddSingleton(new RevitTask());
        });
    }

=======
>>>>>>> origin/main
    private static PushButtonData CreateTestButtonData()
    {
        var smallImageSource = BuildImageResourcePath("Information_16.png");
        var largeImageSource = BuildImageResourcePath("Information_32.png");

        var pushButtonData = RevitControlFactory.CreateButtonData("Test"
            , "Test"
            , "Test"
            , smallImageSource
            , largeImageSource
            , typeof(RevitTestCommandFactory)
            , typeof(RevitTestCommandAvailabilityFactory));

        return pushButtonData;
    }

<<<<<<< HEAD
    private static PushButtonData CreateShowDialogButtonData()
    {
        var smallImageSource = BuildImageResourcePath("Information_16.png");
        var largeImageSource = BuildImageResourcePath("Information_32.png");
        return RevitControlFactory.CreateButtonData(
            "ShowTestDialog",
            "Show Dialog",
            "Open the test dialog window.",
            smallImageSource,
            largeImageSource,
            typeof(ShowTestDialogCommandFactory),
            typeof(ShowTestDialogCommandAvailabilityFactory));
    }

=======
>>>>>>> origin/main
    private static Uri BuildImageResourcePath(string imageFileName)
    {
        return new Uri($"pack://application:,,,/Scotec.Revit.Test;component/Resources/Images/{imageFileName}");
    }



    //private static PushButtonData CreateButtonData(string name, string text, string description, Type commandType)
    //{
    //    return new PushButtonData(name, text,
    //        Assembly.GetExecutingAssembly().Location, commandType.FullName)
    //    {
    //        Image = CreateImageSource("Information_16.png"),
    //        LargeImage = CreateImageSource("Information_32.png"),
    //        ToolTip = description,
    //        ClassName = typeof(RevitTestCommandFactory).FullName,
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
