using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using Scotec.Extensions.Utilities;

namespace Scotec.Revit.LoadContext
{
    [Generator]
    public class RevitSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx => GenerateLoadContext(ctx));

            var provider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
                .Where(m => m.AttributeLists
                             .Any(list => list.Attributes
                                              .Any(a => a.ToString() is "RevitApp" or "RevitCommand")));

            var compilation = context.CompilationProvider.Combine(provider.Collect());

            context.RegisterSourceOutput(compilation, Execute);

        }

        private void GenerateLoadContext(IncrementalGeneratorPostInitializationContext context)
        {
            var template = LoadTemplate("AddinLoadContext");
            if (!string.IsNullOrEmpty(template))
            {
                context.AddSource("AddinLoadContext.g.cs", template);
            }
        }

        private static string? LoadTemplate(string templateName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourcePath = assembly
                               .GetManifestResourceNames()
                               .FirstOrDefault(name => name.Contains(templateName));

            if (resourcePath == null)
            {
                return null;
            }

            using var stream = assembly.GetManifestResourceStream(resourcePath)!;
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private void Execute(SourceProductionContext context,
                             (Compilation Left, ImmutableArray<ClassDeclarationSyntax> Right) tuple)
        {
            //Debugger.Launch();
            var (compilation, syntaxList) = tuple;

            //var treesWithClassWithAttributes = compilation.SyntaxTrees
            //    .Where(st => st.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>()
            //    .Any(p => p.DescendantNodes().OfType<AttributeSyntax>().Any())).ToList();
            
            //foreach (var tree in treesWithClassWithAttributes)
            //{
            //    if (!tree.GetRoot().DescendantNodes()
            //            .OfType<AttributeSyntax>()
            //            .Any(a => a.DescendantTokens().Any(t => t.Text == "RevitApp")))
            //    {
            //        continue;
            //    }
            //}
            
            foreach (var syntax in syntaxList)
            {
                var symbol = compilation.GetSemanticModel(syntax.SyntaxTree)
                    .GetDeclaredSymbol(syntax) as INamedTypeSymbol;

                var from = symbol.ConstructedFrom;
                var className = symbol.Name;
                var @namespace = from.ContainingNamespace.ToDisplayString();


                var template = LoadTemplate("RevitAppFactory");
                if (!string.IsNullOrEmpty(template))
                {
                    var content = template.Format(NAMESPACE => @namespace, CLASSNAME => className);
                    context.AddSource($"className.g.cs", content);
                }
            }
        }
    }
}
