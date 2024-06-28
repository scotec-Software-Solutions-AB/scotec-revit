// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Scotec.Revit.LoadContext;

[Generator]
internal class RevitCommandFactoryGenerator : IncrementalGeneratorBase
{
    public override void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var pipeline = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Scotec.Revit.RevitCommandContextAttribute",
            static (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax,
            static (context, _) => context);

        context.RegisterSourceOutput(pipeline, Execute);
    }

    private static void Execute(SourceProductionContext sourceContext, GeneratorAttributeSyntaxContext syntaxContext)
    {
        var symbol = syntaxContext.TargetSymbol;
        var className = syntaxContext.TargetSymbol.Name;
        var @namespace = symbol.ContainingNamespace.ToDisplayString();
        var globalNamespace = syntaxContext.SemanticModel.Compilation.Assembly.Name;

        var template = LoadTemplate("RevitCommandFactory");
        if (!string.IsNullOrEmpty(template))
        {
            var content = string.Format(template, @namespace, className, globalNamespace);
            sourceContext.AddSource($"{className}Factory.g.cs", content);
        }
    }
}
