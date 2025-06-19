// // Copyright © 2023 - 2025 Olaf Meyer
// // Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Autodesk.Revit.UI;
using Microsoft.Extensions.Logging;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using PropertyAttributes = Mono.Cecil.PropertyAttributes;

namespace Scotec.Revit.Ui.DynamicCommands;

/// <summary>
///     Provides functionality to dynamically generate and register action-based commands for Autodesk Revit.
/// </summary>
/// <remarks>
///     This class builds upon the <see cref="RevitDynamicCommandGenerator" /> base class, enabling the creation of
///     commands
///     that can be dynamically loaded and executed within the Revit environment. It supports the registration of
///     custom actions and the generation of command types with unique identifiers and context-specific properties.
/// </remarks>
public class RevitDynamicActionCommandGenerator : RevitDynamicCommandGenerator
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RevitDynamicActionCommandGenerator" /> class.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly to be dynamically generated.</param>
    /// <param name="context">
    ///     The <see cref="AssemblyLoadContext" /> used for loading and resolving assemblies.
    ///     If <c>null</c>, the default <see cref="AssemblyLoadContext" /> is used.
    /// </param>
    /// <param name="logger">An optional logger instance for logging diagnostic information.</param>
    /// <remarks>
    ///     This constructor sets up the necessary context and assembly definitions for dynamically generating
    ///     action-based command classes. It leverages the base class functionality to configure an assembly resolver,
    ///     initialize the main module, and provide support for dynamic command generation within the Revit environment.
    /// </remarks>
    public RevitDynamicActionCommandGenerator(string assemblyName, AssemblyLoadContext? context = null,
                                              ILogger<RevitDynamicActionCommandGenerator>? logger = null)
        : base(assemblyName, context, logger)
    {
    }

    /// <summary>
    ///     Dynamically generates and registers a Revit action command type with a specified name and associated action.
    /// </summary>
    /// <param name="fullTypeName">
    ///     The fully qualified name of the command type to be generated, including its namespace.
    /// </param>
    /// <param name="action">
    ///     The action to be executed when the generated command is invoked. This action receives
    ///     <see cref="Autodesk.Revit.UI.ExternalCommandData" /> and an <see cref="IServiceProvider" /> as parameters.
    /// </param>
    /// <remarks>
    ///     This method creates a new command type derived from <see cref="RevitDynamicActionCommandFactory" />, assigns it a
    ///     unique
    ///     identifier, and registers the provided action for execution within the Revit environment.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="fullTypeName" /> or <paramref name="action" /> is <c>null</c>.
    /// </exception>
    public void GenerateActionCommandType(string fullTypeName, Action<ExternalCommandData, IServiceProvider> action)
    {
        var typeDefinition = GenerateCommandClass(fullTypeName, typeof(RevitDynamicActionCommandFactory));
        var commandId = Guid.NewGuid();

        AddIdPropertyOverride(typeDefinition, commandId);

        var contextName = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly())!.Name ?? "";
        AddContextNamePropertyOverride(typeDefinition, contextName);

        RevitDynamicActionCommand.RegisterAction(commandId, action);
    }

    /// <summary>
    ///     Adds an override for the <c>ContextName</c> property in the specified derived type.
    /// </summary>
    /// <param name="derivedType">
    ///     The <see cref="TypeDefinition" /> representing the derived type where the property override will be added.
    /// </param>
    /// <param name="contextName">
    ///     The context name value to be returned by the overridden <c>ContextName</c> property getter.
    /// </param>
    /// <remarks>
    ///     This method dynamically generates a property named <c>ContextName</c> with a getter method that returns
    ///     the specified <paramref name="contextName" />. The generated getter method overrides the corresponding
    ///     abstract or virtual method in the base type.
    /// </remarks>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if the base method <c>get_ContextName</c> cannot be found in the type hierarchy of the base type.
    /// </exception>
    private void AddContextNamePropertyOverride(TypeDefinition derivedType, string contextName)
    {
        // Define the property
        var property = new PropertyDefinition(
            "ContextName",
            PropertyAttributes.None,
            derivedType.Module.ImportReference(typeof(string))
        );
        // Define the getter method
        var getMethod = new MethodDefinition(
            "get_ContextName",
            MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
            derivedType.Module.ImportReference(typeof(string))
        );
        // Generate IL for the getter method
        var ilProcessor = getMethod.Body.GetILProcessor();
        // Load the constant string value onto the stack
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldstr, contextName));

        // Return the value
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));

        // Add the getter method to the property
        property.GetMethod = getMethod;

        // Add the getter method and property to the derived type
        derivedType.Methods.Add(getMethod);
        derivedType.Properties.Add(property);

        // Ensure the method overrides the base class's abstract method
        var baseGetMethod = FindBaseMethod(derivedType.BaseType.Resolve(), "get_ContextName");
        getMethod.Overrides.Add(derivedType.Module.ImportReference(baseGetMethod));
    }

    /// <summary>
    ///     Adds an override for the "Id" property in the specified derived type, assigning it a predefined <see cref="Guid" />
    ///     value.
    /// </summary>
    /// <param name="derivedType">
    ///     The <see cref="TypeDefinition" /> representing the derived type where the property override
    ///     will be added.
    /// </param>
    /// <param name="commandId">The <see cref="Guid" /> value to be assigned to the "Id" property.</param>
    /// <remarks>
    ///     This method dynamically defines a new "Id" property in the specified type, implements its getter method to return
    ///     the provided <paramref name="commandId" />, and ensures that the method overrides the base class's abstract
    ///     "get_Id" method.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="derivedType" /> is <c>null</c>.
    /// </exception>
    private void AddIdPropertyOverride(TypeDefinition derivedType, Guid commandId)
    {
        // Define the property
        var property = new PropertyDefinition("Id", PropertyAttributes.None, derivedType.Module.ImportReference(typeof(Guid)));

        // Define the getter method
        var getMethod = new MethodDefinition(
            "get_Id",
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
            derivedType.Module.ImportReference(typeof(Guid)));

        // Implement the getter method
        var ilProcessor = getMethod.Body.GetILProcessor();

        // Load the predefined Guid onto the stack
        var guidConstructor = typeof(Guid).GetConstructor([typeof(string)]);
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldstr, commandId.ToString()));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Newobj, derivedType.Module.ImportReference(guidConstructor)));
        ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));

        // Add the getter method to the property
        property.GetMethod = getMethod;

        // Add the getter method and property to the derived type
        derivedType.Methods.Add(getMethod);
        derivedType.Properties.Add(property);

        // Ensure the method overrides the base class's abstract method
        var baseGetMethod = FindBaseMethod(derivedType.BaseType.Resolve(), "get_Id");
        getMethod.Overrides.Add(derivedType.Module.ImportReference(baseGetMethod));
    }

    /// <summary>
    ///     Traverses the type hierarchy to locate a method with the specified name in the given type or its base types.
    /// </summary>
    /// <param name="type">The <see cref="TypeDefinition" /> representing the type to start the search from.</param>
    /// <param name="methodName">The name of the method to search for.</param>
    /// <returns>
    ///     A <see cref="Mono.Cecil.MethodReference" /> representing the found method if it exists in the type hierarchy.
    /// </returns>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when a method with the specified name cannot be found in the type hierarchy.
    /// </exception>
    private MethodReference FindBaseMethod(TypeDefinition type, string methodName)
    {
        // Traverse the type hierarchy to find the method
        while (true)
        {
            var method = type.Methods.FirstOrDefault(m => m.Name == methodName);
            if (method != null)
            {
                return method;
            }

            // Move to the base type
            var baseType = type.BaseType.Resolve();
            if (baseType is null)
            {
                // If the method is not found, throw an exception
                var message = $"Method '{methodName}' could not be found in the type hierarchy.";
                Logger?.LogError(message);
                throw new InvalidOperationException(message);
            }

            type = baseType;
        }
    }
}
