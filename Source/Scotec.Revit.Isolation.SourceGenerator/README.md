<!--
SPDX-FileCopyrightText: Copyright (c) 2026 Olaf Meyer
SPDX-FileCopyrightText: Copyright (c) 2026 scotec Software Solutions AB
SPDX-License-Identifier: MIT
-->

# Scotec.Revit.Isolation.SourceGenerator

Roslyn incremental source generator that produces assembly load context infrastructure for
isolated Autodesk Revit add-ins.

## Overview

Scotec.Revit.Isolation.SourceGenerator works together with Scotec.Revit.Isolation. It inspects
isolation attributes on Revit entry point classes and automatically generates the factory and
AssemblyLoadContext types required to load each add-in in its own isolated context.

## What Gets Generated

For each annotated Revit entry point the generator produces:

- RevitAssemblyLoadContext: Custom AssemblyLoadContext that resolves private dependencies in isolation.
- RevitAddinAssemblyLoadContext / RevitSharedAssemblyLoadContext: Per-add-in and shared variants.
- RevitAssemblyLoadContextInitializer: Initializes the load context before the first activation.
- Factory classes (RevitApplicationFactory, RevitDbApplicationFactory, RevitCommandFactory,
  RevitCommandAvailabilityFactory): Thin Revit-facing types that activate implementations inside
  the isolated context.

## Usage

This package is consumed automatically when you install Scotec.Revit.Isolation and does not
normally need to be referenced directly.

    dotnet add package Scotec.Revit.Isolation.SourceGenerator

## Related Packages

- Scotec.Revit.Isolation  https://www.nuget.org/packages/Scotec.Revit.Isolation/
- Scotec.Revit             https://www.nuget.org/packages/Scotec.Revit/

## Documentation

- https://github.com/scotec-Software-Solutions-AB/scotec-revit/blob/main/Documentation/RevitAddinIsolation.md

## License

MIT License  https://licenses.nuget.org/MIT