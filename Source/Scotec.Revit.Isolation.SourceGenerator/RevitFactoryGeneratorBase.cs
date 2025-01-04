// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Scotec.Revit.Isolation.SourceGenerator;

public abstract class RevitFactoryGeneratorBase : RevitIncrementalGenerator
{
    /// <summary>
    /// Performs initialization logic specific to the derived generator.
    /// </summary>
    /// <remarks>
    /// This method is invoked during the initialization phase of the generator. It retrieves a list of attributes
    /// using the <see cref="GetAttributes"/> method and registers source outputs for each attribute by calling
    /// <see cref="RegisterSourceOutputForAttribute"/>.
    /// </remarks>
    /// <seealso cref="GetAttributes"/>
    /// <seealso cref="RegisterSourceOutputForAttribute"/>
    protected override void OnInitialize()
    {
        var attributes = GetAttributes();
        foreach (var attribute in attributes)
        {
            RegisterSourceOutputForAttribute(attribute);
        }
    }

    /// <summary>
    /// Registers a source output pipeline for the specified attribute type.
    /// </summary>
    /// <param name="attributeTypeName">
    /// The fully qualified name of the attribute type for which the source output pipeline is to be registered.
    /// </param>
    /// <remarks>
    /// This method creates a syntax provider pipeline that identifies classes annotated with the specified attribute.
    /// It then registers the pipeline to generate source code by invoking the <see cref="Execute"/> method.
    /// </remarks>
    /// <seealso cref="Execute"/>
    private void RegisterSourceOutputForAttribute(string attributeTypeName)
    {
        //Debugger.Launch();
        var pipeline = Context.SyntaxProvider.ForAttributeWithMetadataName(
            attributeTypeName,
            static (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax,
            static (context, _) => context);

        Context.RegisterSourceOutput(pipeline, Execute);
    }

    /// <summary>
    /// Executes the source generation process for a specific class annotated with a target attribute.
    /// </summary>
    /// <param name="sourceContext">
    /// The context used to add generated source code during the source generation process.
    /// </param>
    /// <param name="syntaxContext">
    /// The context containing information about the syntax and semantic model of the target class.
    /// </param>
    /// <remarks>
    /// This method generates source code for a class annotated with a specific attribute by:
    /// <list type="bullet">
    /// <item>Extracting the target class's symbol, name, namespace, and global namespace.</item>
    /// <item>Loading a template using the <see cref="GetTemplateName"/> method.</item>
    /// <item>Formatting the template with the extracted information.</item>
    /// <item>Adding the generated source code to the source context.</item>
    /// </list>
    /// If the template is empty or null, no source code is generated.
    /// </remarks>
    /// <seealso cref="RegisterSourceOutputForAttribute"/>
    private void Execute(SourceProductionContext sourceContext, GeneratorAttributeSyntaxContext syntaxContext)
    {
        //Debugger.Launch();
        var symbol = syntaxContext.TargetSymbol;
        var className = syntaxContext.TargetSymbol.Name;
        var @namespace = symbol.ContainingNamespace.ToDisplayString();
        var globalNamespace = syntaxContext.SemanticModel.Compilation.Assembly.Name;

        var template = LoadTemplate(GetTemplateName());
        if (!string.IsNullOrEmpty(template))
        {
            var content = string.Format(template, @namespace, className, globalNamespace);
            sourceContext.AddSource($"{className}Factory.g.cs", content);
        }
    }

    /// <summary>
    /// Retrieves the name of the template to be used for source generation.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> representing the name of the template. 
    /// This value is used to load and format the template during the source generation process.
    /// </returns>
    /// <remarks>
    /// Derived classes must override this method to provide the specific template name 
    /// required for their respective source generation logic.
    /// </remarks>
    protected abstract string GetTemplateName();

    /// <summary>
    /// Retrieves the attributes required by the derived factory generator.
    /// </summary>
    /// <returns>
    /// An array of <see cref="string"/> containing the fully qualified names of the attributes
    /// used by the specific factory generator.
    /// </returns>
    /// <remarks>
    /// This method is abstract and must be implemented by derived classes to specify the attributes
    /// relevant to their source generation logic. These attributes are typically used to identify
    /// and process specific elements in the source code.
    /// </remarks>
    /// <seealso cref="RevitCommandFactoryGenerator"/>
    /// <seealso cref="RevitCommandAvailabilityFactoryGenerator"/>
    /// <seealso cref="RevitDbApplicationFactoryGenerator"/>
    /// <seealso cref="RevitApplicationFactoryGenerator"/>
    protected abstract string[] GetAttributes();
}