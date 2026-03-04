

using Scotec.Revit.Isolation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: RevitAddinAssembly]


namespace GenerateTest
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RevitAssemblyPreLoaderAttribute : Attribute
    {
    }

    public interface IRevitAssemblyPreLoader
    {
        void PreLoad();
    }


    public class RevitAssemblyPreLoader : IRevitAssemblyPreLoader
    {
        public void PreLoad()
        {
        }
    }
}
