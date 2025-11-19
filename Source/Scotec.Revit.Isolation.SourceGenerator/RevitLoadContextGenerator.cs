// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Scotec.Revit.Isolation.SourceGenerator;

/// <summary>
/// Generates source code for managing Revit add-in load contexts.
/// </summary>
/// <remarks>
/// This sealed class extends <see cref="RevitIncrementalGenerator"/> to implement a source generator
/// that creates a load context for Revit add-ins. It uses the Roslyn incremental generator model
/// to produce source files dynamically during compilation.
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class RevitLoadContextGenerator : RevitIncrementalGenerator
{
    /// <summary>
    /// Performs initialization logic specific to the <see cref="RevitLoadContextGenerator"/> class.
    /// </summary>
    /// <remarks>
    /// This method is invoked during the generator's initialization phase to register the source output
    /// for generating the Revit add-in load context. It utilizes the <see cref="IncrementalGeneratorInitializationContext.CompilationProvider"/>
    /// to produce source files dynamically.
    /// </remarks>
    protected override void OnInitialize()
    {
        //Debugger.Launch();
        Context.RegisterSourceOutput(Context.CompilationProvider, Execute);
    }

    /// <summary>
    /// Executes the source generation process for creating the Revit add-in load context.
    /// </summary>
    /// <param name="context">
    /// The <see cref="SourceProductionContext"/> used to report diagnostics and add generated source files.
    /// </param>
    /// <param name="compilation">
    /// The <see cref="Compilation"/> representing the current state of the code being compiled.
    /// </param>
    /// <remarks>
    /// This method generates a source file for the Revit add-in load context by loading a predefined template
    /// and formatting it with the namespace of the current assembly. The generated source file is then added
    /// to the compilation output.
    /// </remarks>
    private void Execute(SourceProductionContext context, Compilation compilation)
    {
        var template = LoadTemplate("RevitAssemblyLoadContext");
        if (!string.IsNullOrEmpty(template))
        {
            var @namespace = compilation.Assembly.Name;
            var content = string.Format(template, @namespace);
            context.AddSource("RevitAssemblyLoadContext.g.cs", content);
        }
    }
}