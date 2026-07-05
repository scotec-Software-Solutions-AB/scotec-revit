# Scotec.Revit

A modern .NET library for building robust, testable, and maintainable Autodesk® Revit® add-ins. This solution provides advanced abstractions for add-in lifecycle management, command execution, event handling, dynamic model updates, dependency injection, and add-in isolation, targeting .NET 8 for maximum compatibility and performance.

## Features

- **Revit Add-in Isolation:**  
  Provides patterns and guidance for isolating Revit add-ins, improving reliability and testability.
- **Revit Command Framework:**  
  Simplifies the implementation of Revit external commands with built-in transaction management, dependency injection, and failure handling.
- **RevitTask (External Event Integration):**  
  Enables safe, asynchronous, and context-correct execution of Revit API operations from any thread or UI, using the `IExternalEventHandler` mechanism. This allows you to bridge between external UI frameworks (like WPF/WinForms) and the Revit API, ensuring all database operations are performed in the correct Revit context.
- **RevitApp / RevitDbApp (Application Lifecycle):**  
  Base classes for `IExternalApplication` and `IExternalDBApplication` with full DI container setup via `Microsoft.Extensions.Hosting`, and `[RevitStartup]` / `[RevitShutdown]` attributes for lifecycle methods with automatic parameter injection.
- **RevitCommandAvailability:**  
  Controls whether a Revit command is enabled in the ribbon UI. Provides a per-check DI scope and supports the `[RevitCommandAvailabilityCheck]` attribute for clean, injectable availability logic.
- **Revit Context Interfaces:**  
  Provides scoped (`IRevitContext`, `IRevitUiContext`) and singleton (`IGlobalRevitContext`, `IGlobalRevitUiContext`) context interfaces for safe, correct access to `Application`, `Document`, and related Revit objects inside commands, event handlers, and tasks.
- **RevitEventHandler (Application Event Integration):**  
  Generic base class for handling Revit application and UI events with automatic DI scope per invocation. Pre-built handlers cover document lifecycle, view activation, idling, and more.
- **RevitUpdater (Dynamic Model Updater):**  
  Base class for DMU add-ins with automatic registration, disposal, and DI injection per updater execution.
- **Cross-Add-In Context:**  
  `RevitContextTracker` and `IRevitScopeFactory` enable an add-in to obtain the correct Revit document context when responding to requests from other add-ins.

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

### 3. RevitTask

A comprehensive guide to the `RevitTask` class, including:

- How to safely execute operations in the Revit API context using `IExternalEventHandler`
- Usage patterns for both result-returning and void tasks
- Threading, context, and error handling
- Example integration with WPF and async/await

**See:** [RevitTask.md](Documentation/RevitTask.md)

---

### 4. RevitApp and RevitDbApp

A guide to the `RevitApp` and `RevitDbApp` base classes for implementing Revit external applications, including:

- The difference between `RevitApp` (`IExternalApplication`) and `RevitDbApp` (`IExternalDBApplication`)
- Configuring the DI container via `Microsoft.Extensions.Hosting`
- Using `[RevitStartup]` and `[RevitShutdown]` attributes for lifecycle methods with automatic parameter injection
- Assembly resolution helpers for add-in isolation

**See:** [RevitApp.md](Documentation/RevitApp.md)

---

### 5. RevitCommandAvailability

A guide to the `RevitCommandAvailability` base class for controlling ribbon command availability, including:

- How to implement `IExternalCommandAvailability` with DI
- Using the `[RevitCommandAvailabilityCheck]` attribute for injectable availability logic
- Available injectable parameters (`UIApplication`, `CategorySet`, `IRevitContext`, `IRevitUiContext`, and custom services)

**See:** [RevitCommandAvailability.md](Documentation/RevitCommandAvailability.md)

---

### 6. Revit Context Interfaces

A reference guide to all context interfaces provided by the framework, including:

- Scoped interfaces: `IRevitContext` and `IRevitUiContext` — per command execution, event invocation, or `RevitTask.Run` call
- Singleton interfaces: `IGlobalRevitContext` and `IGlobalRevitUiContext` — available for the entire add-in lifetime
- When and how each interface is registered across `RevitCommand`, `RevitEventHandler<>`, and `RevitTask`
- Common mistakes to avoid

**See:** [RevitContext.md](Documentation/RevitContext.md)

---

### 7. Cross-Add-In Revit Context

A guide to obtaining the correct Revit document context when responding to requests from another add-in, including:

- Why cross-add-in context is non-trivial (isolated `AssemblyLoadContext`, no active `RevitCommand` entry point)
- Using `RevitContextTracker` to track the active context
- Using `IRevitScopeFactory` to create a DI scope with the correct context from outside a `RevitCommand`
- Out-of-process channel and shared-context in-process communication patterns

**See:** [RevitCrossAddinContext.md](Documentation/RevitCrossAddinContext.md)

---

### 8. RevitEventHandler

A comprehensive guide to the `RevitEventHandler<TEventArgs>` base class for handling Revit application events, including:

- Pre-built handler base classes for document lifecycle and UI events
- Automatic DI scope per invocation with `IRevitContext` and `IRevitUiContext` registration
- Using the `[RevitEventHandlerExecute]` attribute for injectable handler methods
- Self-subscribing and self-unsubscribing lifecycle

**See:** [RevitEventHandler.md](Documentation/RevitEventHandler.md)

---

### 9. RevitUpdater

A guide to the `RevitUpdater` base class for implementing Revit DMU (Dynamic Model Updater) add-ins, including:

- Required abstract members (`GetUpdaterId`, `GetChangePriority`, `GetUpdaterName`, and others)
- Registering triggers in `OnRegisterUpdater`
- Using the `[RevitUpdaterExecute]` attribute for injectable execute logic
- Automatic registration and disposal

**See:** [RevitUpdater.md](Documentation/RevitUpdater.md)


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
   - [RevitApp.md](Documentation/RevitApp.md): Implement add-in application classes with DI and lifecycle attributes.
   - [RevitCommand.md](Documentation/RevitCommand.md): Learn about the command framework, transaction modes, and DI.
   - [RevitCommandAvailability.md](Documentation/RevitCommandAvailability.md): Control ribbon command availability with DI.
   - [RevitContext.md](Documentation/RevitContext.md): Use scoped and singleton context interfaces to access Revit objects.
   - [RevitCrossAddinContext.md](Documentation/RevitCrossAddinContext.md): Handle cross-add-in context with `RevitContextTracker` and `IRevitScopeFactory`.
   - [RevitEventHandler.md](Documentation/RevitEventHandler.md): Handle Revit application events with automatic DI per invocation.
   - [RevitTask.md](Documentation/RevitTask.md): Safely execute Revit API operations from any thread using `RevitTask`.
   - [RevitUpdater.md](Documentation/RevitUpdater.md): Implement DMU add-ins with automatic registration and DI.

4. **Start developing your Revit add-in**
   - Use the provided abstractions and patterns as described in the documentation files.

## Contributing

Contributions, issues, and feature requests are welcome!  
Please see the [CONTRIBUTING.md](CONTRIBUTING.md) file for guidelines.

## License

This project is licensed under the MIT License.  
See the [LICENSE](license.txt) file for details.
