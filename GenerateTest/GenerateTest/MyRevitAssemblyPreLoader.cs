using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Scotec.Revit.Isolation;
using Scotec.Revit.Loader;



namespace GenerateTest
{
    [RevitAssemblyPreLoader]
    internal class MyRevitAssemblyPreLoader : IRevitAssemblyPreLoader
    {
        public void PreLoad()
        {
        }
    }
}


