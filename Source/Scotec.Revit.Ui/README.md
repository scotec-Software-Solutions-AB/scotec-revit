<!--
SPDX-FileCopyrightText: Copyright (c) 2026 Olaf Meyer
SPDX-FileCopyrightText: Copyright (c) 2026 scotec Software Solutions AB
SPDX-License-Identifier: MIT
-->

# Scotec.Revit.Ui

Ribbon UI and dynamic command support for Autodesk Revit add-ins.

## Overview

Scotec.Revit.Ui extends Scotec.Revit with abstractions for building and managing Revit ribbon UI
elements and dynamic commands. It provides a structured approach to registering tabs, panels, and
buttons, and supports runtime-generated commands that can adapt to context without requiring a
separate compiled command class for each action.

## Key Features

- TabManager: Manages Revit ribbon tabs, panels, and buttons. Prevents duplicates across multiple
  OnStartup calls.
- RevitControlFactory: Factory helpers for creating push buttons, split buttons, and pull-down
  buttons with consistent styling and icon binding.
- RevitDynamicCommand / RevitDynamicActionCommand: Base types for commands resolved and executed
  dynamically at runtime, enabling data-driven UI without a fixed set of compiled command classes.
- RevitDynamicCommandFactory / RevitDynamicActionCommandFactory: Source-generator templates that
  produce factory infrastructure for dynamic commands.
- ControlExtensions: Extension methods for working with ribbon controls programmatically.

## Getting Started

Install the NuGet package:

    dotnet add package Scotec.Revit.Ui

Your project must target net8.0-windows or net10.0-windows.

## Related Packages

- Scotec.Revit           https://www.nuget.org/packages/Scotec.Revit/
- Scotec.Revit.Wpf       https://www.nuget.org/packages/Scotec.Revit.Wpf/
- Scotec.Revit.Isolation https://www.nuget.org/packages/Scotec.Revit.Isolation/

## License

MIT License  https://licenses.nuget.org/MIT