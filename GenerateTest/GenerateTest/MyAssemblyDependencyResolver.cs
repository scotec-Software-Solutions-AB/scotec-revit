using Scotec.Revit.Isolation;
using Scotec.Revit.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace GenerateTest;

//[RevitAssemblyDependencyResolver]
//internal class MyAssemblyDependencyResolver : IRevitAssemblyDependencyResolver
//{
//    private AssemblyDependencyResolver? _resolver;


//    public void Initialize(string assemblyPath)
//    {
//        _resolver = new AssemblyDependencyResolver(assemblyPath);
//    }

//    public string? ResolveAssemblyToPath(AssemblyName assemblyName)
//    {
//        return _resolver?.ResolveAssemblyToPath(assemblyName);
//    }

//    public string? ResolveUnmanagedDllToPath(string unmanagedDllName)
//    {
//        return _resolver?.ResolveUnmanagedDllToPath(unmanagedDllName);
//    }
//}




[RevitAssemblyDependencyResolver]
internal class MyAssemblyDependencyResolver : RevitAssemblyDependencyResolver
{
    private IList<AssemblyName>? _blackList;
    private string? _assemblyPath;

    public MyAssemblyDependencyResolver()
    {
    }
    
    public override void Initialize(string assemblyPath, AssemblyLoadContext context)
    {
        _blackList = new List<AssemblyName>
        {
            new AssemblyName("System.Runtime.CompilerServices.Unsafe"),
            new AssemblyName("System.Buffers"),
            new AssemblyName("System.Memory"),
            new AssemblyName("System.Numerics.Vectors"),
            new AssemblyName("System.Runtime.CompilerServices.Unsafe"),
            new AssemblyName("System.Threading.Tasks.Extensions")
        };
        _assemblyPath = assemblyPath;
        base.Initialize(assemblyPath, context);
    }

    public override string? ResolveAssemblyToPath(AssemblyName assemblyName)
    {
        return _blackList.Contains(assemblyName) 
            ? null 
            : base.ResolveAssemblyToPath(assemblyName);
    }

    public override string? ResolveUnmanagedDllToPath(string unmanagedDllName)
    {
        return base.ResolveUnmanagedDllToPath(unmanagedDllName);   
    }
}

