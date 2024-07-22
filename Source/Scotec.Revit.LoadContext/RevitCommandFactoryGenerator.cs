// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using Microsoft.CodeAnalysis;

namespace Scotec.Revit.LoadContext;

[Generator]
internal class RevitCommandFactoryGenerator : RevitFactoryGeneratorBase
{
    protected override string GetTemplateName()
    {
        return "RevitCommandFactory";
    }

    protected override string[] GetAttributes()
    {
        return new []
        {
            "Scotec.Revit.Isolation.RevitCommandIsolationAttribute",
            "Scotec.Revit.RevitCommandIsolationAttribute"
        };
    }
}