// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Microsoft.CodeAnalysis;

namespace Scotec.Revit.Isolation.SourceGenerator;

/// <summary>
///     Generates source code for Revit database application factories.
/// </summary>
/// <remarks>
///     This class is a sealed implementation of <see cref="RevitFactoryGeneratorBase" /> designed to generate
///     source code for Revit database application factories. It specifies the template name and attributes
///     required for the generation process.
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class RevitDbApplicationFactoryGenerator : RevitFactoryGeneratorBase
{
    /// <summary>
    ///     Retrieves the name of the template associated with the Revit database application factory generator.
    /// </summary>
    /// <returns>
    ///     A <see cref="string" /> representing the name of the template, which is "RevitDbApplicationFactory".
    /// </returns>
    /// <remarks>
    ///     This method overrides the abstract <c>GetTemplateName</c> method from the <see cref="RevitFactoryGeneratorBase" />
    ///     class.
    ///     It provides the specific template name used for generating source code related to Revit database application
    ///     factories.
    /// </remarks>
    protected override string GetTemplateName()
    {
        return "RevitDbApplicationFactory";
    }

    /// <summary>
    ///     Retrieves the attributes associated with the Revit database application factory generator.
    /// </summary>
    /// <returns>
    ///     An array of <see cref="string" /> containing the fully qualified names of the attributes
    ///     used for the generation process.
    /// </returns>
    /// <remarks>
    ///     This method overrides the abstract <c>GetAttributes</c> method from the <see cref="RevitFactoryGeneratorBase" />
    ///     class.
    ///     It provides the specific attributes required for generating source code related to Revit database application
    ///     factories.
    /// </remarks>
    protected override string[] GetAttributes()
    {
        return
        [
            "Scotec.Revit.Isolation.RevitDbApplicationIsolationAttribute"
        ];
    }
}
