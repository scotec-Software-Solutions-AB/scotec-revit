<!--
SPDX-FileCopyrightText: Copyright (c) 2026 Olaf Meyer
SPDX-FileCopyrightText: Copyright (c) 2026 scotec Software Solutions AB
SPDX-License-Identifier: MIT
-->

# Scotec.Revit.Wpf

WPF base classes for Autodesk Revit add-in user interfaces.

## Overview

Scotec.Revit.Wpf provides WPF-aware base types for Revit add-in windows, pages, and user controls.
These base classes handle correct WPF resource dictionary loading when assemblies are loaded in
isolated AssemblyLoadContext instances, ensuring that styles, templates, and resources resolve
correctly regardless of how the add-in assembly is loaded into the process.

## Key Features

- RevitWindow: Window subclass that resolves merged resource dictionaries safely within an isolated
  AssemblyLoadContext. Use instead of Window directly in Revit add-in modal windows.
- RevitPage: Page subclass with the same resource dictionary safety for page-based navigation UI.
- RevitUserControl: UserControl subclass for custom controls in an isolated add-in assembly.

## Why This Matters

When a Revit add-in is loaded into a custom AssemblyLoadContext (via Scotec.Revit.Isolation),
WPF's standard pack://application URI resolution can fail because it targets the default load
context. These base classes apply the correct resource loading strategy for isolated assemblies,
preventing XamlParseException and missing-style errors at runtime.

## Getting Started

Install the NuGet package:

    dotnet add package Scotec.Revit.Wpf

Your project must target net8.0-windows or net10.0-windows.

Replace Window with RevitWindow in your add-in:

    public partial class MyToolWindow : RevitWindow
    {
        public MyToolWindow(MyViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }

## Related Packages

- Scotec.Revit           https://www.nuget.org/packages/Scotec.Revit/
- Scotec.Revit.Ui        https://www.nuget.org/packages/Scotec.Revit.Ui/
- Scotec.Revit.Isolation https://www.nuget.org/packages/Scotec.Revit.Isolation/

## License

MIT License  https://licenses.nuget.org/MIT