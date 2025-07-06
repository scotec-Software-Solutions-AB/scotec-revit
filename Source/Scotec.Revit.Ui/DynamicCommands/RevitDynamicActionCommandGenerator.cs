// // Copyright © 2023 - 2025 Olaf Meyer
// // Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using System.Windows.Documents;
using Autodesk.Revit.UI;
using Microsoft.Extensions.Logging;

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
    protected override IEnumerable<string> GetBaseClasses()
    {
        var baseClasses = ExtractEmbeddedResources("Scotec.Revit.Ui.Resources.RevitDynamicActionCommandFactory");

        return base.GetBaseClasses().Concat(baseClasses).ToList();
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
        var commandId = Guid.NewGuid();
        var commandClass = GenerateCommandClass(fullTypeName, "Scotec.Revit.Ui.DynamicCommands.RevitDynamicActionCommandFactory", commandId, Context.Name);

        AddClass(commandClass);
       
        RevitDynamicActionCommand.RegisterAction(commandId, action);
    }
}
