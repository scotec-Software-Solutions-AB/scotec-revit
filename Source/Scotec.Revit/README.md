<!--
SPDX-FileCopyrightText: Copyright (c) 2026 Olaf Meyer
SPDX-FileCopyrightText: Copyright (c) 2026 scotec Software Solutions AB
SPDX-License-Identifier: MIT
-->

# Scotec.Revit

Core library for building robust, testable, and maintainable Autodesk Revit add-ins with .NET.

## Overview

Scotec.Revit provides the foundational abstractions and infrastructure for Revit add-in development.
It simplifies command execution, application lifecycle management, and background-to-Revit-context
communication, while promoting clean architecture through dependency injection and service separation.

## Key Features

- RevitCommand: Base class for IExternalCommand with built-in transaction management, DI scope per
  execution, and structured failure handling. Supports transaction modes: None, Transaction,
  TransactionGroup, TransactionWithRollback, TransactionGroupWithRollback, and ReadOnly.
- RevitApp / RevitAppBase / RevitDbApp: Base classes for IExternalApplication and
  IExternalDBApplication that wire up an IHost (Microsoft.Extensions.Hosting) for the add-in
  lifecycle, enabling full IoC container support via Autofac.
- RevitTask: Enables safe execution of Revit API operations from any thread or async context using
  the IExternalEventHandler mechanism. Supports both result-returning and void tasks.
- RevitHostBuilder: Configures the hosted application and registers Revit-specific services.
- RevitUpdater: Base support for IUpdater (dynamic model update) implementations.
- RevitSpatialContainmentResolver: Resolves spatial containment (rooms, spaces, zones).
- RevitLinkGraphBuilder: Builds a graph of Revit link relationships for traversal and analysis.
- RevitBasicFileInfo: Reads Revit file metadata without opening a document.

## Getting Started

Install the NuGet package:

    dotnet add package Scotec.Revit

Your project must target net8.0-windows or net10.0-windows.

## Related Packages

- Scotec.Revit.Isolation  https://www.nuget.org/packages/Scotec.Revit.Isolation/
- Scotec.Revit.Ui         https://www.nuget.org/packages/Scotec.Revit.Ui/
- Scotec.Revit.Wpf        https://www.nuget.org/packages/Scotec.Revit.Wpf/

## Documentation

- https://github.com/scotec-Software-Solutions-AB/scotec-revit/blob/main/Documentation/RevitCommand.md
- https://github.com/scotec-Software-Solutions-AB/scotec-revit/blob/main/Documentation/RevitTask.md
- https://github.com/scotec-Software-Solutions-AB/scotec-revit/blob/main/Documentation/RevitAddinIsolation.md

## License

MIT License  https://licenses.nuget.org/MIT