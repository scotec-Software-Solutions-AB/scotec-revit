using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Scotec.Revit.Test
{
    [RevitCommand(RevitAppType = typeof(RevitTestApp))]
    [Transaction(TransactionMode.Manual)]

    public class RevitTestCommand : RevitCommand
    {
        protected override string CommandName => "TestCommand";
        protected override Result OnExecute(ExternalCommandData commandData, IServiceProvider services)
        {
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class RevitTestCommandFactory : IExternalCommand
    {
        private readonly IExternalCommand _instance;

        public RevitTestCommandFactory()
        {
            var assembly = RevitTestAppFactory.Context.LoadFromAssemblyPath(typeof(RevitTestCommandFactory).Assembly.Location);
            var types = assembly.GetTypes();
            var t = types.First(type => type.Name == "RevitTestCommand");
            _instance = (IExternalCommand)Activator.CreateInstance(t);
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return _instance.Execute(commandData, ref message, elements);
        }
    }

}
