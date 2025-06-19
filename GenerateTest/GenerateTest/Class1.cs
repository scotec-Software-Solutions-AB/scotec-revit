
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Scotec.Revit.Isolation;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;

//[assembly: RevitAddinIsolationContext(ContextName = "TestContext")]

namespace GenerateTest
{
    [RevitCommandIsolation()]
    public class Class1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            throw new NotImplementedException();
        }
    }

    [RevitCommandAvailabilityIsolation()]
    public class Class2 : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            throw new NotImplementedException();
        }
    }
    [RevitApplicationIsolation]
    public class Class3: IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            throw new NotImplementedException();
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            throw new NotImplementedException();
        }
    }

    [RevitDbApplicationIsolation]

    public class Class4: IExternalDBApplication
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
}



