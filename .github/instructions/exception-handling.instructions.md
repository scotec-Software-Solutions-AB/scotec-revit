---
description: Exception and failure handling across Scotec.Revit host integrations.
applyTo: "**/*.cs"
---

# Exceptions and failures

- Preserve current exception and failure handling semantics; inspect existing exception types before referring to a "domain" or "runtime" exception hierarchy.
- Respect Revit command result and transaction rollback behavior, `IExternalEventHandler` restrictions, updater failure handling and lifecycle cleanup.
- Catch exceptions only where recovery, translation, logging or cleanup is justified; preserve stacks with `throw;`.
- Avoid hiding errors in asynchronous external-event execution; surface faults/cancellation to callers using existing conventions.
- Do not log credentials, tokens or sensitive model data.
