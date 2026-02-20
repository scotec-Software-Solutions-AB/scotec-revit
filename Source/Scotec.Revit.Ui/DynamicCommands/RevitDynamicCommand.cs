// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

namespace Scotec.Revit.Ui.DynamicCommands;

/// <summary>
///     Represents an abstract base class for dynamic Revit commands, extending the functionality of the
///     <see cref="Scotec.Revit.RevitCommand" /> class.
/// </summary>
/// <remarks>
///     This class serves as a foundation for creating dynamic Revit commands, providing additional features and behaviors
///     that can be utilized and extended by derived classes.
///     It is designed to simplify the implementation of commands that require dynamic behavior within the Revit environment.
///     The transaction mode is set to RevitTransactionMode.None for this class by default.
///     However, it can be overridden by applying the RevitTransactionModeAttribute to a derived class.
/// </remarks>
[RevitTransactionMode(Mode = RevitTransactionMode.None)]
public abstract class RevitDynamicCommand : RevitCommand;
