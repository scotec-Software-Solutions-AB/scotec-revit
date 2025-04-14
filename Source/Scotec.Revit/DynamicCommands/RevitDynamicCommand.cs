// // Copyright © 2023 - 2024 Olaf Meyer
// // Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

namespace Scotec.Revit.DynamicCommands;

/// <summary>
/// Represents an abstract base class for dynamic Revit commands, extending the functionality of the <see cref="Scotec.Revit.RevitCommand"/> class.
/// </summary>
/// <remarks>
/// This class serves as a foundation for creating dynamic Revit commands, providing additional features and behaviors
/// that can be utilized and extended by derived classes. It is designed to simplify the implementation of commands
/// that require dynamic behavior within the Revit environment.
/// </remarks>
public abstract class RevitDynamicCommand : RevitCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RevitDynamicCommand"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor sets the <see cref="RevitCommand.NoTransaction"/> property to <c>true</c>, 
    /// indicating that the command does not require a transaction by default. 
    /// Derived classes can override this behavior if needed.
    /// </remarks>
    protected RevitDynamicCommand()
    {
        NoTransaction = true;
    }
}
