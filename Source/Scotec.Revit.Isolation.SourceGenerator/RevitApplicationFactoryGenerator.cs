// // Copyright © 2023 - 2025 Olaf Meyer
// // Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using Microsoft.CodeAnalysis;

namespace Scotec.Revit.Isolation.SourceGenerator;

/// <summary>
///     Represents a source generator for creating Revit application factories.
/// </summary>
/// <remarks>
///     This class is a sealed implementation of <see cref="RevitFactoryGeneratorBase" /> and is marked with the
///     <see cref="GeneratorAttribute" /> to indicate its role as a source generator for C# code.
///     It provides specific functionality for generating source code related to Revit application factories,
///     including the retrieval of template names and associated attributes.
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class RevitApplicationFactoryGenerator : RevitFactoryGeneratorBase
{
    /// <summary>
    ///     Retrieves the name of the template associated with the Revit application factory generator.
    /// </summary>
    /// <returns>
    ///     A <see cref="string" /> representing the name of the template, which is "RevitApplicationFactory".
    /// </returns>
    /// <remarks>
    ///     This method overrides the abstract <c>GetTemplateName</c> method from the <see cref="RevitFactoryGeneratorBase" />
    ///     class.
    ///     It provides the specific template name used for generating source code related to Revit application factories.
    /// </remarks>
    protected override string GetTemplateName()
    {
        return "RevitApplicationFactory";
    }

    /// <summary>
    ///     Retrieves the attributes associated with the Revit application factory generator.
    /// </summary>
    /// <returns>
    ///     An array of <see cref="string" /> representing the fully qualified names of attributes
    ///     used by the Revit application factory generator.
    /// </returns>
    /// <remarks>
    ///     This method overrides the abstract <c>GetAttributes</c> method from the <see cref="RevitFactoryGeneratorBase" />
    ///     class.
    ///     It provides specific attributes that are applied to the generated Revit application factory code.
    /// </remarks>
    protected override string[] GetAttributes()
    {
        return
        [
            "Scotec.Revit.Isolation.RevitApplicationIsolationAttribute"
        ];
    }
}
