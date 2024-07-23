// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using Microsoft.CodeAnalysis;

namespace Scotec.Revit.LoadContext;

[Generator(LanguageNames.CSharp)]
public sealed class RevitCommandAvailabilityFactoryGenerator : RevitFactoryGeneratorBase
{
    protected override string GetTemplateName()
    {
        return "RevitCommandAvailabilityFactory";
    }

    protected override string[] GetAttributes()
    {
        return new[]
        {
            "Scotec.Revit.Isolation.RevitCommandAvailabilityIsolationAttribute",
            "Scotec.Revit.RevitCommandAvailabilityIsolationAttribute"
        };
    }
}