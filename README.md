# Scotec.Revit

A modern .NET library for building robust, testable, and maintainable Autodesk Revit add-ins. This solution provides advanced abstractions for Revit command execution, dependency injection, and add-in isolation, targeting .NET 8 and .NET Standard 2.0 for maximum compatibility and performance.

---

## Features

- **Revit Add-in Isolation:**  
  Provides patterns and guidance for isolating Revit add-ins, improving reliability and testability.
- **Revit Command Framework:**  
  Simplifies the implementation of Revit external commands with built-in transaction management, dependency injection, and failure handling.

---

## Documentation

### 1. Revit Add-in Isolation

Explains best practices and patterns for isolating Revit add-ins, including:

- Why isolation is important for stability and testability
- How to structure your add-in for isolation
- Example approaches and recommended techniques

**See:** [RevitAddinIsolation.md](Documentation/RevitAddinIsolation.md)

---

### 2. Revit Command Framework

A detailed guide to the `RevitCommand` base class, including:

- How to implement your own Revit commands
- Supported transaction modes (`None`, `SingleTransaction`, `TransactionGroup`, and rollback variants)
- How to use and override dependency injection scopes
- How to register additional services for your command

**See:** [RevitCommand.md](Documentation/RevitCommand.md)

---

## Getting Started

1. **Install the NuGet package**
   - Add the [Scotec.Revit](https://www.nuget.org/packages/Scotec.Revit/) NuGet package to your Revit add-in project using Visual Studio's NuGet Package Manager or the CLI:
     ```
     dotnet add package Scotec.Revit
     ```
   - For other related packages, see the [NuGet Gallery](https://www.nuget.org/packages?q=Scotec.Revit).

2. **Reference the library in your project**
   - Ensure your project targets `net8.0-windows`.

3. **Explore the documentation**
   - [RevitAddinIsolation.md](Documentation/RevitAddinIsolation.md): Understand add-in isolation patterns and best practices.
   - [RevitCommand.md](Documentation/RevitCommand.md): Learn about the command framework, transaction modes, and DI.

4. **Start developing your Revit add-in**
   - Use the provided abstractions and patterns as described in the documentation files.

## Contributing

Contributions, issues, and feature requests are welcome!  
Please see the [CONTRIBUTING.md](CONTRIBUTING.md) file for guidelines.

---

## License

This project is licensed under the MIT License.  
See the [LICENSE](license.txt) file for details.

---