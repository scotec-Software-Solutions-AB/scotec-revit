<!--
SPDX-FileCopyrightText: Copyright (c) 2026 Olaf Meyer
SPDX-FileCopyrightText: Copyright (c) 2026 scotec Software Solutions AB
SPDX-License-Identifier: MIT
-->

# Scotec.Revit.Isolation

Assembly load context isolation support for Autodesk Revit add-ins.

## Overview

Scotec.Revit.Isolation enables Revit add-ins to load their dependencies into isolated
AssemblyLoadContext instances. This prevents version conflicts between add-ins and with Revit's
own assemblies, which is essential when multiple add-ins are loaded in the same Revit process.

## Key Features

- RevitApplicationIsolation: Marks IExternalApplication for isolated loading.
- RevitDbApplicationIsolationAttribute: Marks IExternalDBApplication for isolated loading.
- RevitCommandIsolation: Marks IExternalCommand for isolated loading.
- RevitCommandAvailabilityIsolation: Marks IExternalCommandAvailability for isolated loading.
- RevitAddinIsolationContext: Defines a named isolation context grouping entry points that share
  a single AssemblyLoadContext.
- RevitSharedIsolationContext: Marks assemblies or types shared across isolation boundaries.
- Factory and load context infrastructure is generated automatically at compile time by
  Scotec.Revit.Isolation.SourceGenerator. No hand-written boilerplate required.
- Includes build-transitive MSBuild props that propagate configuration to consuming projects.

## Getting Started

Install the NuGet package:

    dotnet add package Scotec.Revit.Isolation

Annotate your external application:

    [RevitAddinIsolationContext("MyAddin")]
    [RevitApplicationIsolation]
    public sealed class MyApplication : RevitApp { }

Reference the generated factory type in your .addin manifest instead of the application class.

## Related Packages

- Scotec.Revit                          https://www.nuget.org/packages/Scotec.Revit/
- Scotec.Revit.Isolation.SourceGenerator https://www.nuget.org/packages/Scotec.Revit.Isolation.SourceGenerator/

## Documentation

- https://github.com/scotec-Software-Solutions-AB/scotec-revit/blob/main/Documentation/RevitAddinIsolation.md

## License

MIT License  https://licenses.nuget.org/MIT