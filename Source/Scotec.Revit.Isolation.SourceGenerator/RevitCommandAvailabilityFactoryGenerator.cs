// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using Microsoft.CodeAnalysis;

namespace Scotec.Revit.Isolation.SourceGenerator;

/// <summary>
/// Generates source code for Revit command availability factories.
/// </summary>
/// <remarks>
/// This generator is responsible for creating factory classes that handle the availability of Revit commands.
/// It derives from <see cref="RevitFactoryGeneratorBase"/> and provides specific template names and attributes
/// required for generating the source code.
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class RevitCommandAvailabilityFactoryGenerator : RevitFactoryGeneratorBase
{
    /// <summary>
    /// Retrieves the name of the template associated with the Revit command availability factory generator.
    /// </summary>
    /// <returns>
    /// A <see cref="string"/> representing the name of the template, which is "RevitCommandAvailabilityFactory".
    /// </returns>
    /// <remarks>
    /// This method overrides the abstract <c>GetTemplateName</c> method from the <see cref="RevitFactoryGeneratorBase"/> class.
    /// It provides the specific template name used for generating source code related to Revit command availability factories.
    /// </remarks>
    protected override string GetTemplateName()
    {
        return "RevitCommandAvailabilityFactory";
    }

    /// <summary>
    /// Retrieves the attributes associated with the Revit command availability factory generator.
    /// </summary>
    /// <returns>
    /// An array of <see cref="string"/> containing the fully qualified names of the attributes 
    /// used for generating source code related to Revit command availability factories.
    /// </returns>
    /// <remarks>
    /// This method overrides the abstract <c>GetAttributes</c> method from the <see cref="RevitFactoryGeneratorBase"/> class.
    /// It provides the specific attributes required for identifying and handling Revit command availability isolation.
    /// </remarks>
    protected override string[] GetAttributes()
    {
        return
        [
            "Scotec.Revit.Isolation.RevitCommandAvailabilityIsolationAttribute",
        ];
    }
}