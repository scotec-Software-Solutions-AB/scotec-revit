// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;
using System.Linq;
    
namespace Scotec.Revit.Isolation.SourceGenerator;

/// <summary>
///     Generates source code for managing Revit add-in load contexts.
/// </summary>
/// <remarks>
///     This sealed class extends <see cref="RevitIncrementalGenerator" /> to implement a source generator
///     that creates a load context for Revit add-ins. It uses the Roslyn incremental generator model
///     to produce source files dynamically during compilation.
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class RevitLoadContextGenerator : RevitIncrementalGenerator
{
    /// <summary>
    ///     Performs initialization logic specific to the <see cref="RevitLoadContextGenerator" /> class.
    /// </summary>
    /// <remarks>
    ///     This method is invoked during the generator's initialization phase to register the source output
    ///     for generating the Revit add-in load context. It utilizes the
    ///     <see cref="IncrementalGeneratorInitializationContext.CompilationProvider" />
    ///     to produce source files dynamically.
    /// </remarks>
    protected override void OnInitialize()
    {
        Context.RegisterSourceOutput(Context.CompilationProvider, Execute);
    }

    /// <summary>
    ///     Executes the source generation process for creating the Revit add-in load context.
    /// </summary>
    /// <param name="context">
    ///     The <see cref="SourceProductionContext" /> used to report diagnostics and add generated source files.
    /// </param>
    /// <param name="compilation">
    ///     The <see cref="Compilation" /> representing the current state of the code being compiled.
    /// </param>
    /// <remarks>
    ///     This method generates a source file for the Revit add-in load context by loading a predefined template
    ///     and formatting it with the namespace of the current assembly. The generated source file is then added
    ///     to the compilation output.
    /// </remarks>
    private void Execute(SourceProductionContext context, Compilation compilation)
    {
        var @namespace = compilation.Assembly.Name;
        GenerateAssemblyLoadContext(context, @namespace);
        
        var (contextName, usedSharedContextName) = GenerateAddinLoadContext(context, compilation, @namespace);
        var sharedContextName = GenerateAddinSharedLoadContext(context, compilation, @namespace);

        GenerateContextInitializer(context, @namespace, sharedContextName, contextName, usedSharedContextName);
    }

    private static void GenerateContextInitializer(SourceProductionContext context, string @namespace, string sharedContextName, string contextName, string usedSharedContextName)
    {
        var template = LoadTemplate("RevitAssemblyLoadContextInitializer");
        if (!string.IsNullOrEmpty(template))
        {
            var content = string.Format(template, @namespace, sharedContextName, contextName, usedSharedContextName);
            context.AddSource("RevitAssemblyLoadContextInitializer.g.cs", content);
        }
    }

    private (string conext, string sharedContext) GenerateAddinLoadContext(SourceProductionContext context, Compilation compilation, string @namespace)
    {
        if (!TryGetRevitAddinContextName(compilation, out var contextName, out var sharedContextName) || string.IsNullOrEmpty(contextName) || string.IsNullOrEmpty(sharedContextName))
        {
            return (string.Empty, string.Empty);
        }

        var template = LoadTemplate("RevitAddinAssemblyLoadContext");
        if (!string.IsNullOrEmpty(template))
        {
            var content = string.Format(template, @namespace);
            context.AddSource("RevitAddinAssemblyLoadContext.g.cs", content);
            return (contextName!, sharedContextName!);
        }

        return (string.Empty, string.Empty);
    }
    private string GenerateAddinSharedLoadContext(SourceProductionContext context, Compilation compilation, string @namespace)
    {
        if (!TryGetRevitAddinSharedContextName(compilation, out var sharedContextName) || string.IsNullOrEmpty(sharedContextName))
        {
            return string.Empty;
        }

        var template = LoadTemplate("RevitAddinSharedAssemblyLoadContext");
        if (!string.IsNullOrEmpty(template))
        {
            var content = string.Format(template, @namespace);
            context.AddSource("RevitAddinSharedAssemblyLoadContext.g.cs", content);
            return sharedContextName!;
        }

        return string.Empty;
    }

    private void GenerateAssemblyLoadContext(SourceProductionContext context, string @namespace)
    {
        var template = LoadTemplate("RevitAssemblyLoadContext");
        if (!string.IsNullOrEmpty(template))
        {
            var content = string.Format(template, @namespace);
            context.AddSource("RevitAssemblyLoadContext.g.cs", content);
        }

    }

    private bool TryGetRevitAddinContextName(Compilation compilation, out string? contextName, out string? sharedContextName)
    {
        contextName = null;
        sharedContextName = null;

        if (!TryGetAttributeData(compilation, "RevitAddinAssemblyAttribute", out var attributeData) || attributeData is null)
        {
            return false;
        }
        

        // Look for a named argument called "Mode"
        foreach (var namedArg in attributeData.NamedArguments)
        {
            if (namedArg.Key == "ContextName")
            {
                contextName = namedArg.Value.Value?.ToString();
            }
            if (namedArg.Key == "SharedContextName")
            {
                sharedContextName = namedArg.Value.Value?.ToString();
            }
        }

        // Use the assembly name if no context name has been specified.
        if (string.IsNullOrEmpty(contextName))
        {
            contextName = compilation.AssemblyName;
        }
        
        sharedContextName ??= string.Empty;

        return true;
    }
    private bool TryGetRevitAddinSharedContextName(Compilation compilation, out string? sharedContextName)
    {
        sharedContextName = null;
        if (!TryGetAttributeData(compilation, "RevitSharedContextAttribute", out var attributeData) || attributeData is null)
        {
            return false;
        }


        // Check ConstructorArguments
        if (attributeData.ConstructorArguments.Length > 0)
        {
            sharedContextName = attributeData.ConstructorArguments[0].Value?.ToString();
        }

        return !string.IsNullOrEmpty(sharedContextName);
    }

    private static bool TryGetAttributeData(Compilation compilation, string attributeName, out AttributeData? attributeData)
    {
        attributeData = compilation.Assembly
                                       .GetAttributes()
                                       .FirstOrDefault(a =>
                                           a.AttributeClass?.Name ==  attributeName ||
                                           a.AttributeClass?.ToDisplayString().EndsWith($".{attributeName}") == true);
        return attributeData is not null;
    }
}

