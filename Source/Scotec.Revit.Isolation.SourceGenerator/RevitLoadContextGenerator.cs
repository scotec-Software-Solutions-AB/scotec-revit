// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Scotec.Revit.Isolation.SourceGenerator;

namespace Scotec.Revit.Isolation.SourceGenerator;

internal sealed record RevitGeneratorOptions
{
    public RevitGeneratorOptions(string sharedAssemblies, string blackListedAssemblies, string preloadedAssemblies
        , string addinRootAssembly, string sharedRootAssembly)
    {
        SharedAssemblies = sharedAssemblies.Split(['|'], StringSplitOptions.RemoveEmptyEntries);
        BlackListedAssemblies = blackListedAssemblies.Split(['|'], StringSplitOptions.RemoveEmptyEntries);
        PreloadedAssemblies = preloadedAssemblies.Split(['|'], StringSplitOptions.RemoveEmptyEntries);
        AddinRootAssembly = addinRootAssembly;
        SharedRootAssembly = sharedRootAssembly;
    }
    public string[] SharedAssemblies { get; set; }
    public string[] BlackListedAssemblies { get; set; }
    public string[] PreloadedAssemblies { get; set; }
    public string AddinRootAssembly { get; set; }
    public string SharedRootAssembly { get; set; }
}


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
    //protected override void OnInitialize()
    //{
    //    Context.RegisterSourceOutput(Context.CompilationProvider, Execute);
    //}
    protected override void OnInitialize()
    {
        var sharedAssemblies = GetBuildProperty("RevitLoadContextSharedAssemblies");
        var blackListedAssemblies = GetBuildProperty("RevitLoadContextBlackListedAssemblies");
        var preloadedAssemblies = GetBuildProperty("RevitLoadContextPreloadedAssemblies");
        var addinRootAssembly = GetBuildProperty("RevitLoadContextAddinRootAssembly");
        var sharedRootAssembly = GetBuildProperty("RevitLoadContextSharedRootAssembly");

        var options =
            sharedAssemblies
                .Combine(blackListedAssemblies)
                .Combine(preloadedAssemblies)
                .Combine(addinRootAssembly)
                .Combine(sharedRootAssembly)
                .Select((x, _) => new RevitGeneratorOptions(
                    sharedAssemblies: x.Left.Left.Left.Left ?? string.Empty,
                    blackListedAssemblies: x.Left.Left.Left.Right ?? string.Empty,
                    preloadedAssemblies: x.Left.Left.Right ?? string.Empty,
                    addinRootAssembly: x.Left.Right ?? string.Empty,
                    sharedRootAssembly: x.Right ?? string.Empty)); var input = Context.CompilationProvider.Combine(options);

        Context.RegisterSourceOutput(input,  (context, source) =>
        {
            var compilation = source.Left;
            var generatorOptions = source.Right;

            Execute(context, compilation, generatorOptions);
        });
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
    private void Execute(SourceProductionContext context, Compilation compilation, RevitGeneratorOptions generatorOptions)
    {
        var @namespace = compilation.Assembly.Name;

        var hasAddinContext = TryGenerateAddinLoadContext(context, compilation, generatorOptions, @namespace, out var contextName, out var useSharedContext);
        var hasSharedContext = TryGenerateAddinSharedLoadContext(context, compilation, @namespace, out var sharedContextName);

        if ((!hasAddinContext || string.IsNullOrEmpty(contextName)) && (!hasSharedContext || string.IsNullOrEmpty(sharedContextName)))
        {
            return;
        }

        GenerateAssemblyLoadContext(context, @namespace);

        contextName ??= string.Empty;
        sharedContextName ??= string.Empty;
        useSharedContext ??= string.Empty;

        GenerateContextInitializer(context, generatorOptions, @namespace, sharedContextName, contextName, useSharedContext, hasAddinContext, hasSharedContext);
    }

    private static void GenerateContextInitializer(SourceProductionContext context, RevitGeneratorOptions generatorOptions, string @namespace, string sharedContextName, string contextName,
                                                   string usedSharedContextName, bool hasAddinContext, bool hasSharedContext)
    {
        var template = LoadTemplate("RevitAssemblyLoadContextInitializer");
        if (!string.IsNullOrEmpty(template))
        {
            var content = string.Format(template, @namespace, sharedContextName, contextName, 
                usedSharedContextName, hasAddinContext.ToString(), hasSharedContext.ToString(),
                generatorOptions.AddinRootAssembly, generatorOptions.SharedRootAssembly);
            context.AddSource("RevitAssemblyLoadContextInitializer.g.cs", content);
        }
    }

    private bool TryGenerateAddinLoadContext(SourceProductionContext context, Compilation compilation, RevitGeneratorOptions generatorOptions, string @namespace, out string? contextName,
                                             out string? sharedContextName)
    {
        if (!TryGetRevitAddinContextName(compilation, out contextName, out sharedContextName) || string.IsNullOrEmpty(contextName))
        {
            return false;
        }

        var template = LoadTemplate("RevitAddinAssemblyLoadContext");
        if (!string.IsNullOrEmpty(template))
        {
            var content = string.Format(template, @namespace, 
                generatorOptions.AddinRootAssembly,
                BuildCollectionExpression(generatorOptions.SharedAssemblies),
                BuildCollectionExpression(generatorOptions.BlackListedAssemblies),
                BuildCollectionExpression(generatorOptions.PreloadedAssemblies));
            context.AddSource("RevitAddinAssemblyLoadContext.g.cs", content);
            return true;
        }

        return false;
    }

    private static string BuildCollectionExpression(string[] values)
    {
        var builder = new StringBuilder();
        builder.Append('[');

        builder.Append(string.Join(",", values.Select(v => $"\"{v}\"")));
        builder.Append(']');

        return builder.ToString();
    }

    private bool TryGenerateAddinSharedLoadContext(SourceProductionContext context, Compilation compilation, string @namespace, out string? contextName)
    {
        if (!TryGetRevitAddinSharedContextName(compilation, out contextName) || string.IsNullOrEmpty(contextName))
        {
            return false;
        }

        var template = LoadTemplate("RevitSharedAssemblyLoadContext");
        if (!string.IsNullOrEmpty(template))
        {
            var content = string.Format(template, @namespace);
            context.AddSource("RevitSharedAssemblyLoadContext.g.cs", content);
            return true;
        }

        return false;
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

        if (!TryGetAttributeData(compilation, "RevitAddinIsolationContextAttribute", out var attributeData) || attributeData is null)
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

        return true;
    }

    private bool TryGetRevitAddinSharedContextName(Compilation compilation, out string? sharedContextName)
    {
        sharedContextName = null;
        if (!TryGetAttributeData(compilation, "RevitSharedIsolationContextAttribute", out var attributeData) || attributeData is null)
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
                                       a.AttributeClass?.Name == attributeName ||
                                       a.AttributeClass?.ToDisplayString().EndsWith($".{attributeName}") == true);
        return attributeData is not null;
    }
}
