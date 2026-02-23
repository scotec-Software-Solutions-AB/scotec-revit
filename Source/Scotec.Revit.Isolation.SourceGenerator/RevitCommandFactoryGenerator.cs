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

    /// <summary>
    /// Executes the source generation process for Revit commands.
    /// </summary>
    /// <param name="sourceContext">
    /// The <see cref="SourceProductionContext" /> that provides context for the source generation process,
    /// including mechanisms for reporting diagnostics and adding generated source.
    /// </param>
    /// <param name="syntaxContext">
    /// The <see cref="GeneratorAttributeSyntaxContext" /> that provides information about the syntax and semantic
    /// model of the code being analyzed during the source generation process.
    /// </param>
    /// <returns>
    /// An array of objects representing the results of the source generation process. This typically includes
    /// information such as the transaction mode required for the generated Revit commands.
    /// </returns>
    /// <remarks>
    /// This method overrides the <c>OnExecute</c> method from the <see cref="RevitFactoryGeneratorBase" /> class.
    /// It performs specific logic to analyze the provided syntax and semantic model, determine the transaction mode,
    /// and return the results required for generating Revit command factories.
    /// </remarks>
    protected override object?[] OnExecute(SourceProductionContext sourceContext, GeneratorAttributeSyntaxContext syntaxContext)
    {
        //Debugger.Launch();
        // Find the RevitTransactionModeAttribute type in the compilation
        var compilation = syntaxContext.SemanticModel.Compilation;
        var transactionMode = GetTransactionMode(syntaxContext, compilation);

        return [transactionMode];
    }

    /// <summary>
    /// Determines the transaction mode for a Revit command based on the provided syntax context and compilation.
    /// </summary>
    /// <param name="syntaxContext">
    /// The context containing syntax information for the generator attribute.
    /// </param>
    /// <param name="compilation">
    /// The current compilation instance used to analyze symbols and attributes.
    /// </param>
    /// <returns>
    /// A string representing the transaction mode. If no specific transaction mode is found, 
    /// the default value <c>TransactionMode.Manual</c> is returned.
    /// </returns>
    /// <remarks>
    /// This method attempts to retrieve the transaction mode by first checking for the 
    /// <c>Scotec.Revit.RevitTransactionModeAttribute</c>. If not found, it falls back to 
    /// checking the <c>Autodesk.Revit.Attributes.TransactionAttribute</c>.
    /// </remarks>
    private static string GetTransactionMode(GeneratorAttributeSyntaxContext syntaxContext, Compilation compilation)
    {
        // Get the attribute from Scotec.Revit command.
        if (TryGetRevitTransactionMode(syntaxContext, compilation, out var revitTransactionMode))
        {
            return revitTransactionMode!;
        }

        // The command may not be derived from Scotec.Revit.RevitCommand. Try to get the Autodesk.Revit.Attributes.TransactionMode.
        if (TryGetTransactionMode(syntaxContext, compilation, out var transactionMode))
        {
            return transactionMode!;
        }
        
        return TransactionModeManual;
    }

    /// <summary>
    /// Attempts to retrieve the Revit transaction mode from the specified syntax context and compilation.
    /// </summary>
    /// <param name="syntaxContext">
    /// The <see cref="GeneratorAttributeSyntaxContext"/> providing the context for the generator attribute.
    /// </param>
    /// <param name="compilation">
    /// The <see cref="Compilation"/> representing the current compilation context.
    /// </param>
    /// <param name="transactionMode">
    /// When this method returns, contains the transaction mode as a string if the operation is successful; otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the Revit transaction mode was successfully retrieved; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method checks for the presence of the <c>Scotec.Revit.RevitTransactionModeAttribute</c>
    /// and extracts its <c>Mode</c> property value. If the attribute is not found or the value is invalid,
    /// the method returns <c>false</c>.
    /// </remarks>
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

    /// <summary>
    /// Attempts to retrieve the transaction mode from the specified syntax context and compilation.
    /// </summary>
    /// <param name="syntaxContext">
    /// The <see cref="GeneratorAttributeSyntaxContext"/> containing the syntax node and semantic model information.
    /// </param>
    /// <param name="compilation">
    /// The <see cref="Compilation"/> instance used to resolve type and attribute information.
    /// </param>
    /// <param name="transactionMode">
    /// When this method returns, contains the transaction mode as a string if the operation succeeds; otherwise, <c>null</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the transaction mode was successfully retrieved; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method checks for the presence of the <c>Autodesk.Revit.Attributes.TransactionAttribute</c> on the provided syntax context.
    /// If the attribute is found, it extracts the value of its <c>Mode</c> property and maps it to a corresponding transaction mode.
    /// </remarks>
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

    /// <summary>
    /// Retrieves the value of a specified attribute property from a symbol's attributes.
    /// </summary>
    /// <param name="syntaxContext">
    /// The context containing the target symbol and its associated syntax.
    /// </param>
    /// <param name="compilation">
    /// The compilation instance used to resolve the attribute type.
    /// </param>
    /// <param name="attributeType">
    /// The fully qualified name of the attribute type to search for.
    /// </param>
    /// <param name="attributePropertyName">
    /// The name of the property within the attribute whose value is to be retrieved.
    /// </param>
    /// <returns>
    /// The value of the specified attribute property as a string, or <c>null</c> if the attribute or property is not found.
    /// </returns>
    /// <remarks>
    /// This method attempts to locate the specified attribute on the target symbol and retrieve the value of the specified property.
    /// If the property is not explicitly set, it will attempt to retrieve the value from the attribute's constructor arguments.
    /// </remarks>
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
