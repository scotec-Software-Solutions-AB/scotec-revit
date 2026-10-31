---
description: Dependency injection and scoped Revit context rules for Scotec.Revit.
applyTo: "**/*.cs"
---

# Dependency injection

- Inspect registrations in `RevitApp`, `RevitDbApp` and other framework entry points before changing the DI container.
- Follow the repository's use of `Microsoft.Extensions.Hosting` and dependency injection; do not assume Autofac is used here.
- Keep command, event, updater, task and availability-check scopes aligned with the existing factory/lifecycle behavior.
- Scoped `IRevitContext` / `IRevitUiContext` must not be cached by singletons or used after the executing scope completes.
- Distinguish the lifecycle and capabilities of global context interfaces from scoped context interfaces.
- Dispose scopes and resources correctly, including during failures and Revit shutdown.
- Be careful with registrations/types crossing add-in `AssemblyLoadContext` boundaries; matching full type names do not guarantee matching runtime type identity.
