---
name: revit-framework
description: Implement or review Autodesk Revit integration in the Scotec.Revit reusable framework.
---

# Revit framework skill

## Context
Read related source and `Documentation/RevitApp.md`, `Documentation/RevitCommand.md`, `Documentation/RevitTaskDispatcher.md`, `Documentation/RevitContext.md`, `Documentation/RevitCrossAddinContext.md`, `Documentation/RevitEventHandler.md`, `Documentation/RevitUpdater.md` or `Documentation/RevitAddinIsolation.md` as appropriate; confirm paths/files exist.

## Rules
- Execute model API work only in permitted Revit contexts; an arbitrary UI/background thread is not sufficient.
- Preserve command transaction modes, failure handling, availability checks and document lifecycle.
- For asynchronous/off-context work, use established `RevitTaskDispatcher` external-event behavior and propagate faults/cancellation correctly.
- Respect scoped vs global context interfaces, and cross-add-in scope reconstruction when appropriate.
- Preserve independent add-in `AssemblyLoadContext`s, resolver precedence and shared type identity; account for Revit's version-dependent loader behavior.
- Handle event handler/updater subscription and disposal consistently with existing framework lifecycle.
- Check API availability and target frameworks in the real project files; do not assume a single Revit version.
- Add tests for framework semantics; explicitly identify work requiring a real Revit host.
