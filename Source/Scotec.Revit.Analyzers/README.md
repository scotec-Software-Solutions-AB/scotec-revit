<!--
SPDX-FileCopyrightText: Copyright (c) 2026 Olaf Meyer
SPDX-FileCopyrightText: Copyright (c) 2026 scotec Software Solutions AB
SPDX-License-Identifier: MIT
-->

# Scotec.Revit.Analyzers

Roslyn analyzers that enforce correct usage patterns for Autodesk Revit add-ins built with
the Scotec.Revit libraries.

## Overview

Scotec.Revit.Analyzers is a compile-time Roslyn diagnostic analyzer package that detects common
mistakes in Revit add-in code. It is automatically included as an analyzer dependency when you
install Scotec.Revit, so you do not normally need to reference it directly.

## Diagnostics

RevitTaskRunAnalyzer: Detects incorrect RevitTask usage that would cause Revit API calls to execute
outside the valid Revit API context. For example, calling Revit API methods directly from background
threads or async continuations instead of dispatching them through the IExternalEventHandler
mechanism.

## Usage

This package is consumed automatically as part of Scotec.Revit. Diagnostics appear in the Visual
Studio Error List and as build warnings or errors according to your project severity settings.

    dotnet add package Scotec.Revit.Analyzers

## Related Packages

- Scotec.Revit  https://www.nuget.org/packages/Scotec.Revit/

## License

MIT License  https://licenses.nuget.org/MIT