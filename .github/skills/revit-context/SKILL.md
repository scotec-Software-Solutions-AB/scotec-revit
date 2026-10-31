---
name: revit-context
description: Review command, external event, DI scope and cross-add-in Revit execution contexts.
---

# Revit context skill

Review `Documentation/RevitContext.md`, `Documentation/RevitCrossAddinContext.md`, and `Documentation/RevitTaskDispatcher.md` alongside source.

- Do not equate UI thread with a legal Revit API context.
- Identify the current Revit invocation/transaction boundary before executing model operations.
- Use the established dispatcher/external-event pattern for UI-initiated or asynchronous work.
- Track scoped vs global context lifetimes, changes in active document and shutdown.
- When requests cross add-ins, check context tracking and `IRevitScopeFactory` rather than propagating foreign scoped instances.
- Test reentrancy, cancellation, error propagation and stale context behavior, with host verification for actual API interactions.
