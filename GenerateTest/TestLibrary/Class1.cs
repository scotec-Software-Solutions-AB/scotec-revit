using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Scotec.Revit;
using Scotec.Revit.Isolation;

[assembly: RevitAddinAssembly]

namespace TestLibrary
{
        [RevitCommandIsolation(ContextName = "TestContext3")]
        [RevitTransactionMode(Mode = RevitTransactionMode.TransactionGroup)]
        //[Transaction(TransactionMode.Manual)]

        public class Class1 : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                throw new NotImplementedException();
            }
        }
}
