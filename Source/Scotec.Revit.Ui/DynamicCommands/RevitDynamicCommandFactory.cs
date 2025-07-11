﻿// // Copyright © 2023 - 2025 Olaf Meyer
// // Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Scotec.Revit.Ui.DynamicCommands;

/// <summary>
///     Provides an abstract base class for creating and initializing dynamic Revit command instances.
/// </summary>
/// <remarks>
///     This class serves as a factory for dynamically generating and configuring Revit commands. It defines
///     the core structure and behavior required for initializing commands with specific context and type
///     information. Derived classes are expected to implement the abstract members to provide concrete
///     initialization logic for the commands.
/// </remarks>
public abstract class RevitDynamicCommandFactory : IExternalCommand 
{
    /// <summary>
    ///     Gets the fully qualified name of the command type that this factory is responsible for creating.
    /// </summary>
    /// <value>
    ///     A <see cref="string" /> representing the fully qualified name of the command type.
    /// </value>
    /// <remarks>
    ///     This property is used to dynamically locate and instantiate the command type within the appropriate
    ///     assembly load context. The specified type must implement <see cref="IExternalCommand" />.
    /// </remarks>
    protected abstract string CommandTypeName { get; }

    /// <summary>
    ///     Gets the name of the assembly load context associated with this factory.
    /// </summary>
    /// <remarks>
    ///     This property is used to identify the specific assembly load context where the dynamic Revit command
    ///     instances are created. The context name must match the name of an existing
    ///     <see cref="System.Runtime.Loader.AssemblyLoadContext" />.
    /// </remarks>
    protected abstract string ContextName { get; }

    /// <summary>
    ///     Executes the external command within the Revit environment.
    /// </summary>
    /// <param name="commandData">
    ///     An <see cref="ExternalCommandData" /> object that provides access to the Revit application,
    ///     active document, and other related data required to execute the command.
    /// </param>
    /// <param name="message">
    ///     A string that can be set by the command to provide a message to the user in case of failure.
    /// </param>
    /// <param name="elements">
    ///     An <see cref="ElementSet" /> that can be populated with elements related to the failure,
    ///     if the command does not succeed.
    /// </param>
    /// <returns>
    ///     A <see cref="Result" /> value indicating the outcome of the command execution.
    ///     Possible values include <see cref="Result.Succeeded" />, <see cref="Result.Failed" />,
    ///     and <see cref="Result.Cancelled" />.
    /// </returns>
    /// <remarks>
    ///     This method is part of the <see cref="IExternalCommand" /> implementation and serves as the entry point
    ///     for executing the dynamic Revit command. It delegates the execution to an instance of the command type
    ///     created by the factory, ensuring proper initialization and context handling.
    /// </remarks>
    Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var instance = CreateInstance();
        return instance.Execute(commandData, ref message, elements);
    }

    /// <summary>
    /// Initializes the specified dynamic Revit command instance.
    /// </summary>
    /// <param name="command">
    /// The command instance to initialize. This instance must implement <see cref="IExternalCommand" />.
    /// </param>
    /// <remarks>
    /// This method is intended to be overridden in derived classes to provide specific initialization logic
    /// for the dynamic Revit command instance. It is invoked during the creation of the command instance
    /// to ensure it is properly configured before use.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown if the <paramref name="command" /> parameter is <c>null</c>.
    /// </exception>
    protected abstract void InitializeCommand(IExternalCommand command);

    /// <summary>
    ///     Creates an instance of a dynamic Revit command based on the specified <see cref="CommandTypeName" />
    ///     and <see cref="ContextName" />.
    /// </summary>
    /// <returns>
    ///     An instance of a class implementing <see cref="IExternalCommand" />.
    /// </returns>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown if the assembly load context specified by <see cref="ContextName" /> is not found,
    ///     the type specified by <see cref="CommandTypeName" /> cannot be located, or an instance of the type
    ///     cannot be created.
    /// </exception>
    /// <remarks>
    ///     This method dynamically resolves the assembly load context identified by <see cref="ContextName" />,
    ///     retrieves the type specified by <see cref="CommandTypeName" />, and creates an instance of that type.
    ///     The created instance is then initialized using the <see cref="InitializeCommand(IExternalCommand)" /> method.
    /// </remarks>
    private IExternalCommand CreateInstance()
    {
        var loadContext = AssemblyLoadContext.All.FirstOrDefault(c => c.Name == ContextName);
        if (loadContext is null)
        {
            throw new InvalidOperationException($"Load context '{ContextName}' not found.");
        }

        var result = SplitName(CommandTypeName);

        //var assembly = loadContext.LoadFromAssemblyPath(Assembly.GetExecutingAssembly().Location);
        var assembly = loadContext.LoadFromAssemblyName(new AssemblyName(result.AssemblyName));
        using var context = AssemblyLoadContext.EnterContextualReflection(assembly);
        
        // Get the type associated to the assembly load context.
        var type = assembly.GetType(result.TypeName);

        if (type is null)
        {
            throw new InvalidOperationException($"Could not find type '{result.TypeName}' in assembly '{result.AssemblyName}'.");
        }

        var instance = (IExternalCommand?)Activator.CreateInstance(type);
        if (instance is null)
        {
            throw new InvalidOperationException($"Could not create instance of type '{CommandTypeName}'.");
        }

        InitializeCommand(instance);

        return instance;
    }

    private static (string AssemblyName, string TypeName) SplitName(string commandTypeName)
    {
        var parts = commandTypeName.Split(';', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            throw new InvalidOperationException(
                "The command type name must consist of an assembly name and a type name, separated by a ';'.");
        }
        
        return (parts[0], parts[1]);
    }
}
