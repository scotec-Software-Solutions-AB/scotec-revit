using Scotec.Revit.Isolation;
using Scotec.Revit.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace GenerateTest
{
    [RevitAssemblyDependencyResolver]
    internal class MyAssemblyDependencyResolver : IRevitAssemblyDependencyResolver
    {
        private AssemblyDependencyResolver? _resolver;


        public void Initialize(string assemblyPath)
        {
            _resolver = new AssemblyDependencyResolver(assemblyPath);
        }

        public string? ResolveAssemblyToPath(AssemblyName assemblyName)
        {
            return _resolver?.ResolveAssemblyToPath(assemblyName);
        }

        public string? ResolveUnmanagedDllToPath(string unmanagedDllName)
        {
            return _resolver?.ResolveUnmanagedDllToPath(unmanagedDllName);
        }
    }
}
