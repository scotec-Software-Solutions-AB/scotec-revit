// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Scotec.Revit.Isolation.SourceGenerator;

public abstract class IncrementalGenerator : IIncrementalGenerator
{
    protected IncrementalGeneratorInitializationContext Context { get; private set; }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        Context = context;
        OnInitialize();
    }

    protected static string? LoadTemplate(string templateName)
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

    protected abstract void OnInitialize();
}