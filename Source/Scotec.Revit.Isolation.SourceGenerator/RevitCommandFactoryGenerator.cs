// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System.Diagnostics;
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
    private const string TransactionModeManual = "TransactionMode.Manual";
    private const string TransactionModeReadOnly = "TransactionMode.ReadOnly";
    
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

    protected override object?[] OnExecute(SourceProductionContext sourceContext, GeneratorAttributeSyntaxContext syntaxContext)
    {
        //Debugger.Launch();
        // Find the RevitTransactionModeAttribute type in the compilation
        var compilation = syntaxContext.SemanticModel.Compilation;
        var transactionMode = GetTransactionMode(syntaxContext, compilation);

        return [transactionMode];
    }

    private static string GetTransactionMode(GeneratorAttributeSyntaxContext syntaxContext, Compilation compilation)
    {
        // Get the attribute from Scotec.Revit command.
        if (TryGetRevitTransactionMode(syntaxContext, compilation, out var revitTransactionMode))
        {
            return revitTransactionMode!;
        }

        // The command may not be derrived from Scotec.Revit.RevitCommand. Try to get the Autodesk.Revit.Attributes.TransactionMode.
        if (TryGetTransactionMode(syntaxContext, compilation, out var transactionMode))
        {
            return transactionMode!;
        }
        
        return TransactionModeManual;
    }

    private static bool TryGetRevitTransactionMode(GeneratorAttributeSyntaxContext syntaxContext, Compilation compilation, out string? transactionMode)
    {
        transactionMode = null;

        const string attributeType = "Scotec.Revit.RevitTransactionModeAttribute";
        const string attributePropertyName = "Mode";

        var value = GetAttributeValue(syntaxContext, compilation, attributeType, attributePropertyName);
        if (!string.IsNullOrEmpty(value))
        {
            // RevitTransactionMode.None = 0
            // RevitTransactionMode.Transaction = 1
            // RevitTransactionMode.TransactionGroup = 2
            // RevitTransactionMode.TransactionWithRollback = 3
            // RevitTransactionMode.TransactionGroupWithRollback = 4
            // RevitTransactionMode.ReadOnly = 5
            transactionMode = value == "5" ? TransactionModeReadOnly : TransactionModeManual;
            return true;
        }

        return false;
    }

    private static bool TryGetTransactionMode(GeneratorAttributeSyntaxContext syntaxContext, Compilation compilation, out string? transactionMode)
    {
        transactionMode = null;

        const string attributeType = "Autodesk.Revit.Attributes.TransactionAttribute";
        const string attributePropertyName = "Mode";

        var value = GetAttributeValue(syntaxContext, compilation, attributeType, attributePropertyName);
        if (!string.IsNullOrEmpty(value))
        {
            // TransactionMode.Manual = 1
            // TransactionMode.ReadOnly = 2
            transactionMode = value == "2" ? TransactionModeReadOnly : TransactionModeManual;
            return true;
        }

        return false;
    }

    private static string? GetAttributeValue(GeneratorAttributeSyntaxContext syntaxContext, Compilation compilation, string attributeType,
                                             string attributePropertyName)
    {
        var transactionModeAttributeType = compilation.GetTypeByMetadataName(attributeType);

        string? transactionModeValue = null;

        if (transactionModeAttributeType != null)
        {
            // Get the attribute from the target symbol
            var attributeData = syntaxContext.TargetSymbol.GetAttributes()
                                             .FirstOrDefault(ad => SymbolEqualityComparer.Default.Equals(ad.AttributeClass, transactionModeAttributeType));

            if (attributeData != null)
            {
                // Try to get the "Mode" named argument
                var modeArg = attributeData.NamedArguments.FirstOrDefault(kv => kv.Key == attributePropertyName).Value;
                if (modeArg.Value != null)
                {
                    // Mode is an enum value, get its name as string
                    transactionModeValue = modeArg.Value.ToString();
                }
                else if (attributeData.ConstructorArguments.Length > 0)
                {
                    var ctorArg = attributeData.ConstructorArguments[0];
                    if (ctorArg.Value != null)
                        transactionModeValue = ctorArg.Value.ToString();
                }
            }
        }

        return transactionModeValue;
    }
}
