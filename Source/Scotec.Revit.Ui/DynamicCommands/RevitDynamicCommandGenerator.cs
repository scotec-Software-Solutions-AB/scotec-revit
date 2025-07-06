// // Copyright © 2023 - 2025 Olaf Meyer
// // Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows;
using Autodesk.Revit.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;

namespace Scotec.Revit.Ui.DynamicCommands;

/// <summary>
///     Provides an abstract base class for dynamically generating command classes in the Revit environment.
/// </summary>
/// <remarks>
///     This class facilitates the dynamic creation and management of command assemblies, enabling the generation
///     of command classes at runtime. It provides methods for saving and finalizing assemblies, as well as
///     generating derived command types with specific attributes and constructors.
/// </remarks>
public abstract class RevitDynamicCommandGenerator
{
    private readonly List<string> _classes = [];

    protected void AddClass(string @class)
    {
        _classes.Add(@class);
    }
    /// <summary>
    ///     Initializes a new instance of the <see cref="RevitDynamicCommandGenerator" /> class.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly to be dynamically generated.</param>
    /// <param name="context">
    ///     The <see cref="AssemblyLoadContext" /> used for loading and resolving assemblies.
    ///     If <c>null</c>, the default <see cref="AssemblyLoadContext" /> is used.
    /// </param>
    /// <param name="logger">An optional logger instance for logging diagnostic information.</param>
    /// <remarks>
    ///     This constructor sets up the necessary context and assembly definitions for dynamically generating
    ///     command classes. It configures an assembly resolver to locate dependencies and initializes the
    ///     main module for the generated assembly.
    /// </remarks>
    protected RevitDynamicCommandGenerator(string assemblyName, AssemblyLoadContext? context, ILogger<RevitDynamicCommandGenerator>? logger)
    {
        Logger = logger;
        Context = context ?? AssemblyLoadContext.GetLoadContext(GetType().Assembly)!;

    }

    /// <summary>
    ///     Gets the <see cref="AssemblyLoadContext" /> used for loading and resolving assemblies.
    /// </summary>
    /// <value>
    ///     The <see cref="AssemblyLoadContext" /> instance associated with this generator. If no specific context
    ///     was provided during initialization, the default <see cref="AssemblyLoadContext.Default" /> is used.
    /// </value>
    /// <remarks>
    ///     This property provides access to the assembly load context, which is used to manage the loading and
    ///     resolution of assemblies during the dynamic generation of command classes.
    /// </remarks>
    protected AssemblyLoadContext Context { get; }

    /// <summary>
    ///     Gets the logger instance used for logging diagnostic information during the dynamic command generation process.
    /// </summary>
    /// <remarks>
    ///     This property provides an optional <see cref="ILogger{TCategoryName}" /> instance for logging purposes.
    ///     It can be used to log errors, debug information, or other diagnostic messages related to the creation
    ///     and management of dynamic command assemblies.
    /// </remarks>
    protected ILogger<RevitDynamicCommandGenerator>? Logger { get; }


    /// <summary>
    ///     Finalizes the dynamically generated assembly and loads it into the default <see cref="AssemblyLoadContext" />.
    /// </summary>
    /// <returns>
    ///     The <see cref="System.Reflection.Assembly" /> instance representing the dynamically generated and loaded assembly.
    /// </returns>
    /// <exception cref="Exception">
    ///     Thrown if the assembly fails to load from the memory stream.
    /// </exception>
    /// <remarks>
    ///     This method saves the generated assembly to a memory stream, resets the stream position, and attempts to load
    ///     the assembly into the default <see cref="AssemblyLoadContext" />. If an error occurs during the loading process,
    ///     it logs the error (if a logger is available) and rethrows the exception.
    /// </remarks>
    public Assembly FinalizeAssembly()
    {
        using var stream = Compile();
        stream.Seek(0, SeekOrigin.Begin);

        try
        {
            var loadedAssembly = AssemblyLoadContext.Default.LoadFromStream(stream);
            
            return loadedAssembly;
        }
        catch (Exception e)
        {
            Logger?.LogError(e, "Failed to load assembly from stream.");
            throw;
        }
    }

    /// <summary>
    ///     Finalizes the dynamically generated assembly and saves it to the specified file path.
    /// </summary>
    /// <param name="path">The file path where the assembly will be saved.</param>
    /// <returns>
    ///     The <see cref="Assembly" /> instance representing the loaded assembly from the specified file path.
    /// </returns>
    /// <exception cref="Exception">
    ///     Thrown if the assembly fails to load from the specified file path.
    /// </exception>
    /// <remarks>
    ///     This method saves the dynamically generated assembly to the provided file path and attempts to load it
    ///     into the default <see cref="AssemblyLoadContext" />. If the loading process fails, an error is logged
    ///     (if a logger is provided), and the exception is rethrown.
    /// </remarks>
    public Assembly FinalizeAssembly(string path)
    {
        using var stream = Compile();

        SaveAssembly(stream, path);

        try
        {
            var loadedAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            return loadedAssembly;
        }
        catch (Exception e)
        {
            Logger?.LogError(e, $"Failed to load assembly from {path}");
            throw;
        }
    }

    /// <summary>
    ///     Saves the dynamically generated assembly to the specified file path.
    /// </summary>
    /// <param name="outputPath">The file path where the assembly will be saved.</param>
    /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="outputPath" /> is <c>null</c> or empty.</exception>
    /// <exception cref="System.IO.IOException">Thrown if an I/O error occurs during the save operation.</exception>
    /// <exception cref="Exception">Thrown if the assembly cannot be saved due to an unexpected error.</exception>
    /// <remarks>
    ///     This method writes the generated assembly to the specified file path. If the operation fails,
    ///     an error is logged using the provided logger, and the exception is rethrown.
    /// </remarks>
    private void SaveAssembly(Stream assemblyStream, string outputPath)
    {
        try
        {
            assemblyStream.Position = 0;
            
            using var file = File.OpenWrite(outputPath);
            assemblyStream.CopyTo(file);
            file.Flush(true);
            Logger?.LogDebug($"Assembly saved to {outputPath}");
        }
        catch (Exception e)
        {
            Logger?.LogError(e, $"Failed to save assembly to {outputPath}");
            throw;
        }
    }

    protected virtual IEnumerable<string> GetBaseClasses()
    {
        var baseClasses = ExtractEmbeddedResources("Scotec.Revit.Ui.Resources.RevitDynamicCommandFactory");

        return baseClasses;
    }

    public Stream Compile()
    {
        // Extract embedded resources to retrieve all base classes.
        var baseClasses = GetBaseClasses();
        
        // Parse the source code into syntax trees.
        var syntaxTrees = baseClasses.Concat(_classes).Select(code => CSharpSyntaxTree.ParseText(code)).ToList();
        
        // Add references (e.g., mscorlib, System.Runtime, etc.)
        var references = Context.Assemblies
            .Concat(AssemblyLoadContext.Default.Assemblies)
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .ToList();
        
        // Create the compilation
        var compilation = CSharpCompilation.Create(
            "DynamicAssembly",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        // Emit the compiled assembly into a stream.
        var stream = new MemoryStream();
        var result = compilation.Emit(stream);
        if (!result.Success)
        {
            // Handle compilation errors
            var errors = string.Join(Environment.NewLine, result.Diagnostics
                .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                .Select(diagnostic => diagnostic.ToString()));
            throw new Exception($"Compilation failed:\n{errors}");
        }

        stream.Position = 0;
        return stream;
    }


    protected static IEnumerable<string> ExtractEmbeddedResources( string resourceNamespace)
    {
        var assembly = Assembly.GetExecutingAssembly();
        // Get all resource names in the specified namespace
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(resourceNamespace, StringComparison.OrdinalIgnoreCase) && name.EndsWith(".template"));
        foreach (var resourceName in resourceNames)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            yield return reader.ReadToEnd();
        }
    }


    protected string GenerateCommandClass(string fullTypeName, string baseType, Guid commandId, string contextName)
    {
        SplitFullClassName(fullTypeName, out var namespaceName, out var className);
        
        var commandClass = $$"""
                             using System;
                             
                             namespace {{namespaceName}};
                             
                             public class {{className}} : {{baseType}}
                             {
                                 public override Guid Id => new Guid("{{commandId}}");
                                  
                                 protected override string ContextName => "{{contextName}}";
                             }
                             """;
        
        return commandClass;


        static void SplitFullClassName(string fullClassName, out string namespaceName, out string className)
        {
            if (string.IsNullOrWhiteSpace(fullClassName))
            {
                throw new ArgumentException("Full class name cannot be null or empty.", nameof(fullClassName));
            }
            int lastDotIndex = fullClassName.LastIndexOf('.');
            if (lastDotIndex == -1)
            {
                throw new ArgumentException("Full class name must contain a namespace.", nameof(fullClassName));
            }
            namespaceName = fullClassName.Substring(0, lastDotIndex);
            className = fullClassName.Substring(lastDotIndex + 1);
        }
    }

    /// <summary>
    ///     Splits a fully qualified type name into its namespace and class name components.
    /// </summary>
    /// <param name="fullTypeName">
    ///     The fully qualified type name to split. This must include both the namespace and the class name.
    /// </param>
    /// <param name="namespacePart">
    ///     When this method returns, contains the namespace part of the type name, or <c>null</c> if the type name does not
    ///     contain a namespace.
    /// </param>
    /// <param name="classNamePart">
    ///     When this method returns, contains the class name part of the type name.
    /// </param>
    /// <exception cref="System.ArgumentException">
    ///     Thrown when <paramref name="fullTypeName" /> is <c>null</c>, empty, or does not contain a namespace.
    /// </exception>
    /// <remarks>
    ///     This method is used internally to parse type names and separate their namespace and class name components.
    /// </remarks>
    private void SplitTypeName(string fullTypeName, out string? namespacePart, out string classNamePart)
    {
        if (string.IsNullOrWhiteSpace(fullTypeName))
        {
            const string errorMessage = "The full type name cannot be null or empty.";
            Logger?.LogError(errorMessage);
            throw new ArgumentException(errorMessage, nameof(fullTypeName));
        }

        var lastDotIndex = fullTypeName.LastIndexOf('.');
        if (lastDotIndex == -1)
        {
            const string errorMessage = "The full type name does not contain a namespace.";
            Logger?.LogError(errorMessage);
            throw new ArgumentException(errorMessage, nameof(fullTypeName));
        }

        namespacePart = fullTypeName[..lastDotIndex];
        classNamePart = fullTypeName[(lastDotIndex + 1)..];
    }
}
