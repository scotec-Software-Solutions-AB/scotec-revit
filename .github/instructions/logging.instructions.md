---
description: Logging and diagnostics for Scotec.Revit framework code.
applyTo: "**/*.cs"
---

# Logging and diagnostics

- Reuse the repository's actual logger and logging conventions; do not introduce a new logging framework just for a change.
- Prefer structured message templates and named placeholders when supported by the logger.
- Use levels based on severity and expected behavior, not blanket rules (for example, a routine missing optional value is not automatically a warning).
- Where available and appropriate, include Revit operation, command/event/updater name, and safe document or add-in context identifiers to aid diagnostics.
- Preserve logging behavior when refactoring. Avoid noisy per-element logs in hot paths unless tracing is explicitly requested.
- Never log credentials, access tokens, secrets or sensitive model/document content.
