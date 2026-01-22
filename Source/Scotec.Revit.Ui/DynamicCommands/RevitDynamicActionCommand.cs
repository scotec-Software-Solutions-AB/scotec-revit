// // Copyright © 2023 - 2025 Olaf Meyer
// // Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using System;
using System.Collections.Generic;
using Autodesk.Revit.UI;
using Microsoft.Extensions.Logging;

namespace Scotec.Revit.Ui.DynamicCommands;

/// <summary>
///     Represents a dynamic Revit command that executes a registered action based on a unique identifier.
/// </summary>
/// <remarks>
///     This class extends the <see cref="RevitDynamicCommand" /> base class and provides functionality
///     for registering and executing actions dynamically within the Revit environment. Each action is associated with a
///     unique
///     <see cref="System.Guid" /> identifier, allowing for flexible and reusable command implementations.
/// </remarks>
public sealed class RevitDynamicActionCommand : RevitDynamicCommand
{
    private static readonly Dictionary<Guid, Action<ExternalCommandData, IServiceProvider>> Actions = new();
    private readonly ILogger<RevitDynamicActionCommand>? _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RevitDynamicActionCommand" /> class.
    /// </summary>
    /// <remarks>
    ///     This constructor creates a new instance of the <see cref="RevitDynamicActionCommand" /> class without initializing
    ///     a logger.
    ///     It is intended for scenarios where logging is not required or will be set up later.
    /// </remarks>
    public RevitDynamicActionCommand()
    {
        _logger = null;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RevitDynamicActionCommand" /> class with a specified logger.
    /// </summary>
    /// <param name="logger">
    ///     An instance of <see cref="Microsoft.Extensions.Logging.ILogger{TCategoryName}" /> used for logging within the
    ///     command.
    /// </param>
    /// <remarks>
    ///     This constructor allows for the initialization of the <see cref="RevitDynamicActionCommand" /> class with logging
    ///     capabilities.
    ///     It is intended for scenarios where detailed logging is required to monitor or debug the execution of dynamic
    ///     actions.
    /// </remarks>
    public RevitDynamicActionCommand(ILogger<RevitDynamicActionCommand> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Gets or sets the unique identifier associated with the dynamic action command.
    /// </summary>
    /// <value>
    ///     A <see cref="System.Guid" /> representing the unique identifier for the action.
    /// </value>
    /// <remarks>
    ///     This property is used to associate a specific action with the command. The identifier is utilized
    ///     to register and execute actions dynamically within the Revit environment.
    /// </remarks>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets the name of the command, which is used to identify and log the execution of this dynamic action command.
    /// </summary>
    /// <remarks>
    ///     This property overrides the <see cref="Scotec.Revit.RevitCommand.CommandName" /> property to provide a specific
    ///     name for the dynamic action command. The name is primarily used for logging and debugging purposes.
    /// </remarks>
    protected override string CommandName => "Action command";

    /// <summary>
    ///     Registers a dynamic action with a unique identifier for execution within the Revit environment.
    /// </summary>
    /// <param name="id">
    ///     A <see cref="System.Guid" /> that uniquely identifies the action to be registered.
    ///     This identifier must not be empty.
    /// </param>
    /// <param name="action">
    ///     The action to be executed, represented as a delegate of type
    ///     <see cref="System.Action{ExternalCommandData, IServiceProvider}" />.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown when the <paramref name="action" /> parameter is <c>null</c>.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    ///     Thrown when the <paramref name="id" /> parameter is an empty <see cref="System.Guid" />.
    /// </exception>
    /// <remarks>
    ///     This method allows dynamic registration of actions that can be executed by the
    ///     <see cref="RevitDynamicActionCommand" /> class.
    ///     The registered actions are stored in an internal dictionary and can be invoked
    ///     using their associated <see cref="Guid" />.
    /// </remarks>
    public static void RegisterAction(Guid id, Action<ExternalCommandData, IServiceProvider> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (id == Guid.Empty)
        {
            throw new ArgumentException("ID cannot be empty.", nameof(id));
        }

        Actions.Add(id, action);
    }

    /// <summary>
    /// Executes the dynamic action command within the Revit environment.
    /// </summary>
    /// <param name="commandData">
    /// An object that provides contextual information about the external command, including access to the Revit
    /// application and active document.
    /// </param>
    /// <param name="services">
    /// A service provider that supplies additional dependencies required for the execution of the command.
    /// </param>
    /// <returns>
    /// A <see cref="Result" /> value indicating the outcome of the command execution.
    /// Returns <see cref="Result.Succeeded" /> if the command executes successfully,
    /// or <see cref="Result.Failed" /> if an exception occurs during execution.
    /// </returns>
    /// <remarks>
    /// This method overrides the <see cref="OnExecute" /> method to provide
    /// specific behavior for executing dynamic actions. It retrieves the registered action associated with the command's
    /// <see cref="Id" /> and invokes it. If the action is not found or an exception occurs, the method handles the error
    /// and logs it appropriately.
    /// </remarks>
    protected override Result OnExecute(ExternalCommandData commandData, IServiceProvider services)
    {
        if (Actions.TryGetValue(Id, out var action))
        {
            try
            {
                action(commandData, services);
            }
            catch (Exception e)
            {
                _logger?.LogError(e, $"Execution of command '{CommandName}' failed.");
                return Result.Failed;
            }
        }

        return Result.Succeeded;
    }
}
