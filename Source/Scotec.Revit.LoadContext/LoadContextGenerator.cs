// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using Microsoft.CodeAnalysis;

namespace Scotec.Revit.LoadContext;

[Generator]
internal class LoadContextGenerator : IncrementalGenerator
{
    protected override void OnInitialize()
    {
        Context.RegisterSourceOutput(Context.CompilationProvider, Execute);
    }

    private void Execute(SourceProductionContext context, Compilation compilation)
    {
        var template = LoadTemplate("AddinLoadContext");
        if (!string.IsNullOrEmpty(template))
        {
            var @namespace = compilation.Assembly.Name;
            var content = string.Format(template, @namespace);
            context.AddSource("AddinLoadContext.g.cs", content);
        }
    }
}