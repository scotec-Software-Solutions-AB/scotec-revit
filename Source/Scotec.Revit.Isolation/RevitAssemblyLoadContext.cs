// // Copyright © 2023 - 2025 Olaf Meyer
// // Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Scotec.Revit.Isolation;

/// <summary>
///     Represents a custom assembly load context for isolating and resolving assemblies and unmanaged libraries
///     specific to Revit-related operations.
/// </summary>
/// <remarks>
///     This class extends <see cref="System.Runtime.Loader.AssemblyLoadContext" /> to provide a mechanism for
///     resolving assemblies and unmanaged libraries using an
///     <see cref="System.Runtime.Loader.AssemblyDependencyResolver" />.
/// </remarks>
public class RevitAssemblyLoadContext : AssemblyLoadContext
{
    private readonly string _pluginRoot;
    private readonly Dictionary<string, string> _assemblyFullNameMap;
    private readonly Dictionary<string, string> _assemblyNameMap;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RevitAssemblyLoadContext" /> class with the specified context name
    ///     and assembly dependency resolver.
    /// </summary>
    /// <param name="contextName">
    ///     The name of the assembly load context. This name is used for identification purposes.
    /// </param>
    /// <remarks>
    ///     This constructor sets up the custom assembly load context to isolate and resolve dependencies
    ///     specific to Revit-related operations.
    /// </remarks>
    public RevitAssemblyLoadContext(string contextName, string pluginRoot) : base(contextName)
    {
        _pluginRoot = Path.GetFullPath(pluginRoot);
        _assemblyFullNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _assemblyNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        CacheAssemblies();
    }

    /// <summary>
    ///     Resolves and loads the specified assembly by its name within the custom assembly load context.
    /// </summary>
    /// <param name="assemblyName">
    ///     The <see cref="System.Reflection.AssemblyName" /> of the assembly to be loaded.
    /// </param>
    /// <returns>
    ///     The loaded <see cref="System.Reflection.Assembly" /> if the assembly is successfully resolved and loaded;
    ///     otherwise, <see langword="null" />.
    /// </returns>
    /// <remarks>
    ///     This method uses the <see cref="System.Runtime.Loader.AssemblyDependencyResolver" /> to determine the
    ///     file path of the requested assembly. If the assembly is found, it is loaded using
    ///     <see cref="System.Runtime.Loader.AssemblyLoadContext.LoadFromAssemblyPath(string)" />.
    /// </remarks>
    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (_assemblyFullNameMap.TryGetValue(assemblyName.FullName, out var path))
        {
            return LoadFromAssemblyPath(path);
        }

        if (_assemblyNameMap.TryGetValue(assemblyName.Name!, out path))
        {
            return LoadFromAssemblyPath(path);
        }

        // Let framework assemblies resolve normally
        return null;
    }

    /// <summary>
    ///     Resolves and loads the specified unmanaged library by its name within the custom assembly load context.
    /// </summary>
    /// <param name="unmanagedDllName">
    ///     The name of the unmanaged library to be loaded.
    /// </param>
    /// <returns>
    ///     A handle to the loaded unmanaged library if the library is successfully resolved and loaded;
    ///     otherwise, <see cref="System.IntPtr.Zero" />.
    /// </returns>
    /// <remarks>
    ///     This method uses the <see cref="System.Runtime.Loader.AssemblyDependencyResolver" /> to determine the
    ///     file path of the requested unmanaged library. If the library is found, it is loaded using
    ///     <see cref="System.Runtime.Loader.AssemblyLoadContext.LoadUnmanagedDllFromPath(string)" />.
    /// </remarks>
    //protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    //{
    //    var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
    //    if (libraryPath != null)
    //    {
    //        return LoadUnmanagedDllFromPath(libraryPath);
    //    }

    //    return IntPtr.Zero;
    //}

    private void CacheAssemblies()
    {
        foreach (var file in Directory.EnumerateFiles(_pluginRoot, "*.dll", SearchOption.AllDirectories))
        {
            try
            {
                var assemblyName = AssemblyName.GetAssemblyName(file);

                if (!_assemblyFullNameMap.ContainsKey(assemblyName.FullName))
                {
                    _assemblyFullNameMap.Add(assemblyName.FullName, file);
                    if (!_assemblyNameMap.ContainsKey(assemblyName.Name!))
                    {
                        _assemblyNameMap.Add(assemblyName.Name!, file);
                    }
                }
                else
                {
                    Console.WriteLine(
                        $"[Warning] Duplicate assembly identity '{assemblyName.FullName}' found at: {file}. " +
                        $"Using first occurrence: {_assemblyFullNameMap[assemblyName.FullName]}"
                    );
                }
            }
            catch (BadImageFormatException)
            {
                // Not a managed .NET assembly (likely a native DLL) → skip
            }
            catch (FileLoadException)
            {
                // The file exists but is not a valid .NET assembly → skip
            }
        }
    }
}
