// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Scotec.Revit.Isolation.SourceGenerator;

public abstract class RevitFactoryGeneratorBase : IncrementalGenerator
{
    protected override void OnInitialize()
    {
        var attributes = GetAttributes();
        foreach (var attribute in attributes)
        {
            RegisterSourceOutputForAttribute(attribute);
        }
    }

    private void RegisterSourceOutputForAttribute(string attributeTypeName)
    {
        //Debugger.Launch();
        var pipeline = Context.SyntaxProvider.ForAttributeWithMetadataName(
            attributeTypeName,
            static (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax,
            static (context, _) => context);

        Context.RegisterSourceOutput(pipeline, Execute);
    }

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

    protected abstract string GetTemplateName();

    protected abstract string[] GetAttributes();
}