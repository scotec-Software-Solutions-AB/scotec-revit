// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using Microsoft.CodeAnalysis;

namespace Scotec.Revit.LoadContext;

[Generator(LanguageNames.CSharp)]
public sealed class RevitApplicationFactoryGenerator : RevitFactoryGeneratorBase
{
    protected override string GetTemplateName()
    {
        return "RevitApplicationFactory";
    }

    protected override string[] GetAttributes()
    {
        return new[]
        {
            "Scotec.Revit.Isolation.RevitApplicationIsolationAttribute", 
            "Scotec.Revit.RevitApplicationIsolationAttribute"
        };
    }
}