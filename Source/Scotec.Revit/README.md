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

- **RevitCommand** — Base class for `IExternalCommand` with built-in transaction management, a
  per-execution DI scope, and structured failure handling. Supports transaction modes: `None`,
  `Transaction`, `TransactionGroup`, `TransactionWithRollback`, `TransactionGroupWithRollback`,
  and `ReadOnly`.
- **RevitApp / RevitAppBase / RevitDbApp** — Base classes for `IExternalApplication` and
  `IExternalDBApplication` that wire up an `IHost` (Microsoft.Extensions.Hosting) for the add-in
  lifecycle, enabling full IoC container support via Autofac.
- **RevitTask** — Enables safe execution of Revit API operations from any thread or async context
  using the `IExternalEventHandler` mechanism. Supports both result-returning and void tasks, with
  optional DI parameter resolution.
- **RevitEventHandler** — Generic base class for subscribing to Revit application events with a
  per-invocation DI scope. Pre-built concrete handler bases are provided for all common document,
  application, and UI events.
- **RevitCommandAvailability** — Base class for `IExternalCommandAvailability` with DI scope per
  availability check, enabling ribbon button state to be driven by injected services.
- **RevitHostBuilder** — Configures the hosted application and registers Revit-specific services.
- **RevitUpdater** — Base support for `IUpdater` (dynamic model update) implementations.
- **RevitSpatialContainmentResolver** — Resolves spatial containment (rooms, spaces, zones).
- **RevitLinkGraphBuilder** — Builds a graph of Revit link relationships for traversal and analysis.
- **RevitBasicFileInfo** — Reads Revit file metadata without opening the document.

## One Add-in per Assembly Load Context

Each `RevitAppBase`-derived application must run in its own `AssemblyLoadContext`. The framework
stores the root DI service provider in a static field scoped to the load context, which means only
one application instance is supported per context. Attempting to load a second application into the
same context would silently overwrite the service provider, causing unpredictable behavior.

In practice this is not a restriction: every properly isolated add-in already runs in its own
context. Use **Scotec.Revit.Isolation** to generate the required load context infrastructure
automatically at compile time.

## Getting Started

Install the NuGet package:

    dotnet add package Scotec.Revit

Your project must target `net8.0-windows` or `net10.0-windows`.

## Related Packages

- [Scotec.Revit.Isolation](https://www.nuget.org/packages/Scotec.Revit.Isolation/)
- [Scotec.Revit.Ui](https://www.nuget.org/packages/Scotec.Revit.Ui/)
- [Scotec.Revit.Wpf](https://www.nuget.org/packages/Scotec.Revit.Wpf/)

## Documentation

- [RevitCommand](https://github.com/scotec-Software-Solutions-AB/scotec-revit/blob/main/Documentation/RevitCommand.md)
- [RevitEventHandler](https://github.com/scotec-Software-Solutions-AB/scotec-revit/blob/main/Documentation/RevitEventHandler.md)
- [RevitTask](https://github.com/scotec-Software-Solutions-AB/scotec-revit/blob/main/Documentation/RevitTask.md)
- [Revit Add-in Isolation](https://github.com/scotec-Software-Solutions-AB/scotec-revit/blob/main/Documentation/RevitAddinIsolation.md)

## License

MIT License — https://licenses.nuget.org/MIT
