// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using Microsoft.Extensions.DependencyInjection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Scotec.Revit.Isolation;
using Scotec.Revit.Ui;
using ad = Autodesk.Windows;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using RibbonPanel = Autodesk.Revit.UI.RibbonPanel;
using TypeAttributes = Mono.Cecil.TypeAttributes;
using Scotec.Revit.DynamicCommands;

namespace Scotec.Revit.Test;

[RevitCommandIsolation]
[Transaction(TransactionMode.Manual)]
public class RevitTestCommand : RevitCommand
{
    public RevitTestCommand()
    {
        var context = AssemblyLoadContext.GetLoadContext(GetType().Assembly);
    }

    protected override string CommandName => "TestCommand";

    protected override Result OnExecute(ExternalCommandData commandData, IServiceProvider services)
    {
        var context = AssemblyLoadContext.GetLoadContext(GetType().Assembly);

        //CreateCommandClass();


        var application = services.GetService<UIControlledApplication>();
        var t = ComponentManager.Ribbon.Tabs.ToList();
        var tabs = ComponentManager.Ribbon.Tabs.Select(t => t.Title).ToList();
        //var assemblyPath = Assembly.GetExecutingAssembly().Location;
        var assemblyPath = Path.GetFileName(Assembly.GetExecutingAssembly().Location);

        application.CreateRibbonTab("TEST 1");

        var ribbonPanel = application.CreateRibbonPanel("TEST 1", "CustomPanel");
        //ribbonPanel = false;


        //var x = new ad.RibbonSplitButton();


        //ad.RibbonPanel p = null;
        //var b = new ad.RibbonButton();
        //var s = new RibbonPanelSource();
        //p.Source = s;


        // Step 3: Add a SlideOut
        //ribbonPanel.AddSlideOut();
        //// Step 4: Add controls to the SlideOut
        //var pushButtonData = new PushButtonData(
        //    "SlideOutButton",
        //    "SlideOut Btn",
        //    assemblyPath,
        //    "Namespace.MyCommandClass"
        //);
        //var slideOutButton = ribbonPanel.AddItem(pushButtonData) as PushButton;
        //slideOutButton.ToolTip = "This button is inside a SlideOut section.";

        //var pushButtonData = new PushButtonData(
        //    "PushButton1",
        //    "Command 1",
        //    @"C:\Temp\GeneratedAssembly.dll",
        //    //@"GeneratedAssembly",
        //    "GeneratedAssembly.Command1"
        //);

        var fullCommandTypeName = "Test.TestCommand1";
        var generator = new RevitDynamicActionCommandGenerator("GeneratedAssembly.dll");
        generator.GenerateActionCommandType(fullCommandTypeName, (data, provider) =>
        {

        });

        // Save the assembly to a file
        var outputPath = @"C:\Temp\GeneratedAssembly.dll";
        var assembly = generator.FinalizeAssembly(outputPath);
        
        var commandType = assembly.GetType(fullCommandTypeName)!;
        var c = Activator.CreateInstance(commandType);
        //generator.Save(@"C:\Temp\GeneratedAssembly.dll");
        
        var pushButtonData = RevitControlFactory.CreateButtonData("FamilyManager.Open"
            , "PushButton1"
            , "Command 1"
            , CreateImageSource("Information_16.png")
            , CreateImageSource("Information_16.png")
            , commandType);

        ribbonPanel.AddItem(pushButtonData);


        PulldownButtonData pulldownData1 = new PulldownButtonData("Pulldown1", "Options 1")
        {
            Image = CreateImageSource("Information_16.png"),
            LargeImage = CreateImageSource("Information_32.png"),
        };
        PulldownButtonData pulldownData2 = new PulldownButtonData("Pulldown2", "Options 2")
        {
            Image = CreateImageSource("Information_16.png"),
            LargeImage = CreateImageSource("Information_32.png"),
        };
        PulldownButtonData pulldownData3 = new PulldownButtonData("Pulldown3", "Options 3")
        {
            Image = CreateImageSource("Information_16.png"),
            LargeImage = CreateImageSource("Information_32.png"),
        };
        var stackedItems1 = ribbonPanel.AddStackedItems(pulldownData1, pulldownData2, pulldownData3);
        PulldownButton pulldownButton1 = stackedItems1[0] as PulldownButton;
        var pushButton = pulldownButton1.AddPushButton(new PushButtonData("Command1", "Command 1", assemblyPath,
            "Scotec.Revit.Test.Command1")
        {
            Image = CreateImageSource("Information_16.png"),
            LargeImage = CreateImageSource("Information_16.png"),
            //LargeImage = CreateImageSource("Information_32.png"),
            
        });
        pulldownButton1.AddPushButton(new PushButtonData("Command2", "Command 2", assemblyPath,
            "Scotec.Revit.Test.Command2")
        {
            Image = CreateImageSource("Information_16.png"),
            LargeImage = CreateImageSource("Information_16.png"),
            //LargeImage = CreateImageSource("Information_32.png"),
        });
        PulldownButton pulldownButton2 = stackedItems1[1] as PulldownButton;
        pulldownButton2.AddPushButton(new PushButtonData("Command3", "Command 3", assemblyPath,
            "Scotec.Revit.Test.Command1")
        {
            Image = CreateImageSource("Information_16.png"),
            LargeImage = CreateImageSource("Information_32.png"),
        });
        
        pulldownButton2.AddPushButton(new PushButtonData("Command4", "Command 4", assemblyPath,
            "Scotec.Revit.Test.Command2")
        {
            Image = CreateImageSource("Information_16.png"),
            LargeImage = CreateImageSource("Information_32.png"),
        });
        PulldownButton pulldownButton3 = stackedItems1[2] as PulldownButton;
        pulldownButton3.AddPushButton(new PushButtonData("Command5", "Command 5", assemblyPath,
            "Scotec.Revit.Test.Command1"));
        pulldownButton3.AddPushButton(new PushButtonData("Command6", "Command 6", assemblyPath,
            "Scotec.Revit.Test.Command2")
        {
            Image = CreateImageSource("Information_16.png"),
            LargeImage = CreateImageSource("Information_32.png"),
        });


        SplitButtonData splitButtonData1 = new SplitButtonData("SplitButton1", "Split 1")
        {
            Image = CreateImageSource("Information_16.png"),
            LargeImage = CreateImageSource("Information_32.png"),
        };
        SplitButtonData splitButtonData2 = new SplitButtonData("SplitButton2", "Split 2")
        {
            Image = CreateImageSource("Information_16.png"),
            LargeImage = CreateImageSource("Information_32.png"),
        };
        SplitButtonData splitButtonData3 = new SplitButtonData("SplitButton3", "Split 3")
        {
            Image = CreateImageSource("Information_16.png"),
            LargeImage = CreateImageSource("Information_32.png"),
        };
        // Add SplitButtons as Stacked Items
        var stackedItems2 = ribbonPanel.AddStackedItems(splitButtonData1, splitButtonData2, splitButtonData3);
        // Add commands to each SplitButton
        SplitButton splitButton1 = stackedItems2[0] as SplitButton;
        splitButton1.AddPushButton(new PushButtonData("Command1", "Command 1", assemblyPath,
            "Scotec.Revit.Test.Command1")
        {
            Image = CreateImageSource("Information_16.png"),
            LargeImage = CreateImageSource("Information_32.png"),
        });
        splitButton1.AddPushButton(new PushButtonData("Command2", "Command 2", assemblyPath,
            "Scotec.Revit.Test.Command2"));
        SplitButton splitButton2 = stackedItems2[1] as SplitButton;
        splitButton2.AddPushButton(new PushButtonData("Command3", "Command 3", assemblyPath,
            "Scotec.Revit.Test.Command1")
        {
            Image = CreateImageSource("Information_16.png"),
            LargeImage = CreateImageSource("Information_32.png"),
        });
        splitButton2.AddPushButton(new PushButtonData("Command4", "Command 4", assemblyPath,
            "Scotec.Revit.Test.Command2"));
        SplitButton splitButton3 = stackedItems2[2] as SplitButton;
        splitButton3.AddPushButton(new PushButtonData("Command5", "Command 5", assemblyPath,
            "Scotec.Revit.Test.Command1")
        {
            Image = CreateImageSource("Information_16.png"),
            LargeImage = CreateImageSource("Information_32.png"),
        });
        splitButton3.AddPushButton(new PushButtonData("Command6", "Command 6", assemblyPath,
            "Scotec.Revit.Test.Command2")
        {
            Image = CreateImageSource("Information_16.png"),
            LargeImage = CreateImageSource("Information_32.png"),
        });


        AddSlideOut(ribbonPanel);
        AddPulldownButton(ribbonPanel);
        AddSplitButton(ribbonPanel);

       
        //var tab = ComponentManager.Ribbon.Tabs.First(t => t.Name == "TEST 1");
        //var panel = tab.Panels.First(p => p.Source.Name == "CustomPanel");
        //pannel.Source.Items.Add(new CustomRibbonItem());

        return Result.Succeeded;
    }



    private void AddSlideOut(RibbonPanel panel)
    {
        var assemblyPath = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
        // Add a SlideOut to the panel
        panel.AddSlideOut();
        // Add controls to the SlideOut
        PushButtonData buttonData1 = new PushButtonData(
            "SlideOutButton1",
            "Command 1",
            assemblyPath,
            "Scotec.Revit.Test.Command1");
        PushButtonData buttonData2 = new PushButtonData(
            "SlideOutButton2",
            "Command 2",
            assemblyPath,
            "Scotec.Revit.Test.Command2");
        // Add the buttons to the SlideOut
        panel.AddStackedItems(buttonData1, buttonData2);
    }

    private void AddPulldownButton(RibbonPanel panel)
    {
        var assemblyPath = Assembly.GetExecutingAssembly().Location;

        // Create PulldownButtonData
        PulldownButtonData pulldownData = new PulldownButtonData("PulldownButton", "Options");
        // Add the PulldownButton to the panel
        // Add commands to the PulldownButton
        if (panel.AddItem(pulldownData) is PulldownButton pulldownButton)
        {
            pulldownButton.AddPushButton(new PushButtonData(
                "Option1",
                "Option 1",
                assemblyPath,
                "Namespace.Option1Command"));
            pulldownButton.AddPushButton(new PushButtonData(
                "Option2",
                "Option 2",
                assemblyPath,
                "Namespace.Option2Command"));
        }
    }

    private void AddSplitButton(RibbonPanel panel)
    {
        var assemblyPath = Assembly.GetExecutingAssembly().Location;

        // Create SplitButtonData
        SplitButtonData splitButtonData1 = new SplitButtonData("SplitButton1", "Actions");
        // Add the SplitButton to the panel


        // Add commands to the SplitButton
        if (panel.AddItem(splitButtonData1) is SplitButton splitButton)
        {
            splitButton.AddPushButton(new PushButtonData(
                "Action1",
                "Action 1",
                assemblyPath,
                "Namespace.Action1Command")
            {
                Image = CreateImageSource("Information_16.png"),
                LargeImage = CreateImageSource("Information_32.png"),

            });
            splitButton.AddPushButton(new PushButtonData(
                "Action2",
                "Action 2",
                assemblyPath,
                "Namespace.Action2Command")
                {
                Image = CreateImageSource("Information_16.png"),
                LargeImage = CreateImageSource("Information_32.png"),
            });
        }
    }

    private void HidePushButtonInPulldown(RibbonPanel panel, string pulldownButtonName, string pushButtonName)
    {
        // Iterate through all items in the panel
        foreach (var item in panel.GetItems())
        {
            // Check if the item is a PulldownButton
            if (item is PulldownButton pulldownButton && pulldownButton.Name == pulldownButtonName)
            {
                // Iterate through the child items of the PulldownButton
                foreach (var childItem in pulldownButton.GetItems())
                {
                    // Check if the child item is a PushButton and matches the name
                    if (childItem is PushButton pushButton && pushButton.Name == pushButtonName)
                    {
                        // Hide the PushButton
                        pushButton.Visible = false;
                        return;
                    }
                }
            }
        }
    }

    //private static Assembly s_dynamicAssembly;
    private void CreateCommandClass()
    {
        var context = AssemblyLoadContext.GetLoadContext(GetType().Assembly);

        var d = AssemblyLoadContext.Default;
        using var dc = d.EnterContextualReflection();

        var resolver = new DefaultAssemblyResolver();
        // Add the directory containing RevitAPI.dll to the resolver search paths

        var searchDirectories = d.Assemblies.Select(a => Path.GetDirectoryName(a.Location)).Distinct().ToList();
        searchDirectories.ForEach(d => resolver.AddSearchDirectory(d));
        var moduleParameters = new ModuleParameters()
        {
            AssemblyResolver = resolver,
            Kind = ModuleKind.Dll
        };

        // Create a new assembly and module
        var assemblyName = "GeneratedAssembly";
        var assemblyDefinition = AssemblyDefinition.CreateAssembly(
            new AssemblyNameDefinition(assemblyName, new Version(1, 0, 0, 0)),
            "GeneratedModul", moduleParameters);
        var module = assemblyDefinition.MainModule;
        // Import the base class (replace 'BaseClass' with your actual base class)
        var baseType = module.ImportReference(typeof(CommandBase));
        // Define the derived class
        var derivedClass = new TypeDefinition(
            "GeneratedAssembly", // Namespace
            "Command1", // Class name
            TypeAttributes.Public | TypeAttributes.Class,
            baseType); // Base class
        // Add the derived class to the module
        module.Types.Add(derivedClass);

        // Add the default constructor
        AddDefaultConstructor(module, derivedClass, baseType);

        // Add the [Transaction(TransactionMode.Manual)] attribute
        AddTransactionAttribute(module, derivedClass);

        // Save the assembly to a file
        var outputPath = @"C:\Temp\GeneratedAssembly.dll";
        assemblyDefinition.Write(outputPath);
        Console.WriteLine($"Assembly saved to {outputPath}");
    }

    static void AddDefaultConstructor(ModuleDefinition module, TypeDefinition derivedClass, TypeReference baseType)
    {
        // Define the default constructor
        var constructor = new MethodDefinition(
            ".ctor", // Constructor name
            MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
            module.TypeSystem.Void); // Return type is void
        // Get the base class's default constructor
        var baseConstructor = baseType.Resolve().Methods
            .FirstOrDefault(m => m.IsConstructor && !m.HasParameters);
        if (baseConstructor != null)
        {
            var ilProcessor = constructor.Body.GetILProcessor();
            // Emit IL to call the base class's constructor
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldarg_0)); // Load "this"
            ilProcessor.Append(ilProcessor.Create(OpCodes.Call,
                module.ImportReference(baseConstructor))); // Call base constructor
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ret)); // Return
        }

        // Add the constructor to the derived class
        derivedClass.Methods.Add(constructor);
    }

    static void AddTransactionAttribute(ModuleDefinition module, TypeDefinition type)
    {
        // Import the TransactionAttribute type
        var transactionAttributeType = module.ImportReference(typeof(TransactionAttribute));
        // Import the TransactionMode enum
        var transactionModeType = module.ImportReference(typeof(TransactionMode));
        // Get the constructor of TransactionAttribute that takes a TransactionMode argument
        var constructor = transactionAttributeType.Resolve().Methods
            .First(m => m.IsConstructor && m.Parameters.Count == 1 &&
                        m.Parameters[0].ParameterType.FullName == transactionModeType.FullName);
        var constructorReference = module.ImportReference(constructor);
        // Create the CustomAttribute
        var customAttribute = new CustomAttribute(constructorReference);
        // Set the constructor argument to TransactionMode.Manual
        var transactionModeManual = new CustomAttributeArgument(transactionModeType, (int)TransactionMode.Manual);
        customAttribute.ConstructorArguments.Add(transactionModeManual);
        // Add the attribute to the class
        type.CustomAttributes.Add(customAttribute);
    }

    private static ImageSource CreateImageSource(string image)
    {
        var resourcePath = $"Scotec.Revit.Test.Resources.Images.{image}";

        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourcePath);
        if (stream == null)
        {
            return null;
        }

        var decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

        return decoder.Frames[0];
    }

}

public class CustomRibbonItem : Autodesk.Windows.RibbonItem
{
    public CustomRibbonItem()
    {
        Name = "CustomRibbonItem";
        // Create an ElementHost to host the WPF control
        var elementHost = new ElementHost
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            Child = new MyWpfControl() // Your WPF UserControl
        };
    }
}

//[Transaction(TransactionMode.Manual)]
//[RevitCommandIsolation]
public abstract class CommandBaseFactory : IExternalCommand
{
    private readonly IExternalCommand _instance;

    protected CommandBaseFactory()
    {
        //var assembly = AddinLoadContext.Instance.LoadFromAssemblyPath(GetType().Assembly.Location);
        //using var context = AssemblyLoadContext.EnterContextualReflection(assembly);
        //var types = assembly.GetTypes();

        //// Do not compare types. They might be in different loaod contexts and therefore handled as different types.
        ////var type = types.First(type => type.Name == CommandType.Name);
        //// ReSharper disable once VirtualMemberCallInConstructor
        //_instance = (IExternalCommand)Activator.CreateInstance(CommandType);
    }

    //protected abstract Type CommandType { get; }

    protected IExternalCommand CreateInstance()
    {
        var assembly = GetType().Assembly;
        //var assembly = RevitAddinLoadContext.Instance.LoadFromAssemblyPath(GetType().Assembly.Location);
        using var context = AssemblyLoadContext.EnterContextualReflection(assembly);

        // Get the type assiciated to the assembly load context.
        var type = assembly.GetType(CommandTypeName);
        var instance = (IExternalCommand)Activator.CreateInstance(type!);

        return instance;
    }

    protected abstract void Initialze(IExternalCommand command);

    protected abstract string CommandTypeName { get; }

    Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var instance = CreateInstance();
        Initialze(instance);

        return instance!.Execute(commandData, ref message, elements);
    }
}

[Transaction(TransactionMode.Manual)]
public class DynamoCommandFactory : CommandBaseFactory
{
    protected override void Initialze(IExternalCommand command)
    {
        dynamic dynamoCommand = command;
        dynamoCommand.ScriptPath = "C:\\Temp\\DynamoScript.dyn";
    }

    protected override string CommandTypeName => typeof(DynamoCommand).FullName;
}

public class DynamoCommand : CommandBase
{
    public string ScriptPath { get; set; }
    protected override string CommandName => "DynamoCommand";

    protected override Result OnExecute(ExternalCommandData commandData, IServiceProvider services)
    {
        var context = AssemblyLoadContext.GetLoadContext(GetType().Assembly);
        return Result.Succeeded;
    }
}

//[Transaction(TransactionMode.Manual)]
//[RevitCommandIsolation]
public abstract class CommandBase : RevitCommand
{
    protected CommandBase()
    {
        NoTransaction = true;
    }
    //protected override string CommandName => "CommandBase";
}

[Transaction(TransactionMode.Manual)]
public class Command1 : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var context = AssemblyLoadContext.GetLoadContext(GetType().Assembly);
        return Result.Succeeded;
    }
}

[Transaction(TransactionMode.Manual)]
public class Command2 : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        return Result.Succeeded;
    }
}