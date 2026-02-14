// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using Microsoft.CodeAnalysis;

namespace Scotec.Revit.Isolation.SourceGenerator;

/// <summary>
///     Represents a source generator for creating Revit command factories.
/// </summary>
/// <remarks>
///     This generator is responsible for producing source code for Revit command factories
///     by utilizing specific attributes and templates. It extends the functionality of
///     <see cref="RevitFactoryGeneratorBase" /> to provide Revit-specific command generation logic.
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class RevitCommandFactoryGenerator : RevitFactoryGeneratorBase
{
    /// <summary>
    ///     Retrieves the name of the template associated with the Revit command factory generator.
    /// </summary>
    /// <returns>
    ///     A <see cref="string" /> representing the name of the template, which is "RevitCommandFactory".
    /// </returns>
    /// <remarks>
    ///     This method overrides the abstract <c>GetTemplateName</c> method from the <see cref="RevitFactoryGeneratorBase" />
    ///     class.
    ///     It provides the specific template name used for generating source code related to Revit command factories.
    /// </remarks>
    protected override string GetTemplateName()
    {
        return "RevitCommandFactory";
    }

    /// <summary>
    ///     Retrieves the attributes associated with the Revit command factory generator.
    /// </summary>
    /// <returns>
    ///     An array of <see cref="string" /> containing the fully qualified names of the attributes
    ///     used by the Revit command factory generator.
    /// </returns>
    /// <remarks>
    ///     This method overrides the abstract <c>GetAttributes</c> method from the
    ///     <see cref="RevitFactoryGeneratorBase" /> class. It provides the specific attributes
    ///     required for generating source code related to Revit command factories.
    /// </remarks>
    protected override string[] GetAttributes()
    {
        return
        [
            "Scotec.Revit.Isolation.RevitCommandIsolationAttribute"
        ];
    }
}
