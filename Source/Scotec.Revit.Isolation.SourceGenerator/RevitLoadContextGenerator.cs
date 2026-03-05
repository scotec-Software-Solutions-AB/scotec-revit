// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
    
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
        //Debugger.Launch();
        if (!TryGetRevitAddinContextName(compilation, out var contextName))
        {
            return;
        }

        // Look for class with RevitAssemblyDependencyResolverAttribute
        string resolverClassName = resolverClassName = GetResolverClassName(compilation);
        string preLoaderClassName = GetPreLoaderClassName(compilation);

        // If not found, search for a class derived from AssemblyDependencyResolver
        //if (resolverClassName == null)
        //{
        //    foreach (var cls in compilation.SyntaxTrees
        //                                   .SelectMany(tree => tree.GetRoot()
        //                                                           .DescendantNodes()
        //                                                           .OfType<ClassDeclarationSyntax>()))
        //    {
        //        var model = compilation.GetSemanticModel(cls.SyntaxTree);
        //        if (model.GetDeclaredSymbol(cls) is INamedTypeSymbol { BaseType: not null } symbol &&
        //            symbol.BaseType.ToDisplayString() == "System.Runtime.Loader.AssemblyDependencyResolver")
        //        {
        //            resolverClassName = symbol.ToDisplayString();
        //            break;
        //        }
        //    }
        //}

        var template = LoadTemplate("RevitAssemblyLoadContext");
        if (!string.IsNullOrEmpty(template))
        {
            var @namespace = compilation.Assembly.Name;
            var content = string.Format(template, @namespace, contextName, resolverClassName, preLoaderClassName);
            context.AddSource("RevitAssemblyLoadContext.g.cs", content);
        }
    }

    private static string GetResolverClassName(Compilation compilation)
    {
        foreach (var cls in compilation.SyntaxTrees
                                       .SelectMany(tree => tree.GetRoot()
                                                               .DescendantNodes()
                                                               .OfType<ClassDeclarationSyntax>()))
        {
            var model = compilation.GetSemanticModel(cls.SyntaxTree);
            if (model.GetDeclaredSymbol(cls) is INamedTypeSymbol symbol &&
                symbol.GetAttributes().Any(attr =>
                    attr.AttributeClass?.Name == "RevitAssemblyDependencyResolverAttribute" ||
                    attr.AttributeClass?.ToDisplayString().EndsWith(".RevitAssemblyDependencyResolverAttribute") == true))
            {
                return symbol.ToDisplayString();
            }
        }

        return "RevitAssemblyDependencyResolver";
    }

    private static string GetPreLoaderClassName(Compilation compilation)
    {
        foreach (var cls in compilation.SyntaxTrees
                                       .SelectMany(tree => tree.GetRoot()
                                                               .DescendantNodes()
                                                               .OfType<ClassDeclarationSyntax>()))
        {
            var model = compilation.GetSemanticModel(cls.SyntaxTree);
            if (model.GetDeclaredSymbol(cls) is INamedTypeSymbol symbol &&
                symbol.GetAttributes().Any(attr =>
                    attr.AttributeClass?.Name == "RevitAssemblyPreLoaderAttribute" ||
                    attr.AttributeClass?.ToDisplayString().EndsWith(".RevitAssemblyPreLoaderAttribute") == true))
            {
                return symbol.ToDisplayString();
            }
        }

        return "RevitAssemblyPreLoader";
    }

    private bool TryGetRevitAddinContextName(Compilation compilation, out string? contextName)
    {
        contextName = null;
        var attr = compilation.Assembly
                              .GetAttributes()
                              .FirstOrDefault(a =>
                                  a.AttributeClass?.Name == "RevitAddinAssemblyAttribute" ||
                                  a.AttributeClass?.ToDisplayString().EndsWith(".RevitAddinAssemblyAttribute") == true);

        if (attr == null)
        {
            // Not a Revit add-in assembly.
            return false;
        }

        // Look for a named argument called "Mode"
        foreach (var namedArg in attr.NamedArguments)
        {
            if (namedArg.Key == "Mode")
            {
                contextName = namedArg.Value.Value?.ToString();
                break;
            }
        }

        // If Mode is a constructor argument (positional), check ConstructorArguments
        // (Uncomment if Mode is a constructor parameter)
        // if (attr.ConstructorArguments.Length > 0)
        // {
        //     return attr.ConstructorArguments[0].Value?.ToString();
        // }

        // Use the assembly name if no context name has been specified.
        if (string.IsNullOrEmpty(contextName))
        {
            contextName = compilation.AssemblyName;
        }

        return true;
    }
}

