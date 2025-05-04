// // Copyright © 2023 - 2024 Olaf Meyer
// // Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using System;

namespace Scotec.Revit.DynamicCommands;

/// <summary>
/// Represents a factory for creating and initializing instances of <see cref="RevitDynamicActionCommand"/>.
/// </summary>
/// <remarks>
/// This abstract class provides the base implementation for dynamically generating and configuring
/// Revit action commands. It ensures that each command is assigned a unique identifier and is properly
/// initialized with the required context and type information.
/// </remarks>
public abstract class RevitDynamicActionCommandFactory : RevitDynamicCommandFactory<RevitDynamicActionCommand>
{
    /// <summary>
    /// Gets the unique identifier associated with the factory.
    /// </summary>
    /// <value>
    /// A <see cref="System.Guid"/> representing the unique identifier of the factory.
    /// </value>
    /// <remarks>
    /// This identifier is used to associate and initialize instances of <see cref="RevitDynamicActionCommand"/> 
    /// with a specific factory. It ensures that commands created by this factory are uniquely identifiable.
    /// </remarks>
    public abstract Guid Id { get; }

    /// <summary>
    /// This property is declared as abstract in the base class. It should not be implemented here but in the derived class.
    /// However, the derived class and the replacement of this property will be generated with Mono.Cecil.
    /// Since the property is declared in a generic class, it cannot be replaced at runtime, and retrieving the type will fail with the following error:
    /// "System.Reflection.ReflectionTypeLoadException: 'Unable to load one or more of the requested types.
    /// Method override 'get_ContextName' on type 'GENERATED_COMMAND' from assembly 'GENERATED_ASSEMBLY, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' cannot find a method to replace.".
    /// Overriding abstract properties declared in a non-generic class works fine, so it is likely that I did not fully understand
    /// how to use Mono.Cecil to replace the abstract property declared in a generic class.
    /// Including this property here is a workaround to avoid the error. 
    /// </summary>
    protected override string ContextName { get; } = string.Empty;

    /// <summary>
    /// Gets the fully qualified name of the command type associated with this factory.
    /// </summary>
    /// <remarks>
    /// This property returns the <see cref="System.Type.FullName"/> of the <see cref="RevitDynamicActionCommand"/> type.
    /// It is used to identify and initialize commands dynamically within the Revit environment.
    /// </remarks>
    protected override string CommandTypeName => typeof(RevitDynamicActionCommand).FullName!;

    /// <summary>
    /// Initializes the specified <see cref="RevitDynamicActionCommand"/> instance with the required properties.
    /// </summary>
    /// <param name="command">
    /// The <see cref="RevitDynamicActionCommand"/> instance to initialize. This command will be assigned a unique identifier
    /// based on the <see cref="Id"/> property of the factory.
    /// </param>
    /// <remarks>
    /// This method ensures that the <paramref name="command"/> is properly configured with the necessary context
    /// and identifier before execution. It is invoked as part of the command initialization process in the factory.
    /// </remarks>
    protected override void InitializeCommand(RevitDynamicActionCommand command)
    {
        command.Id = Id;
    }
}
