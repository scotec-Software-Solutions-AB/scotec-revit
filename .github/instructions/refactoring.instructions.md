---
description: Focused behavior-preserving refactoring rules for Scotec.Revit.
applyTo: "**/*.cs"
---

# Safe refactoring

- Inspect the implementation, usages and tests before changing structure; reuse existing abstractions.
- Preserve observable behavior, public APIs, diagnostics, Revit context and transaction semantics, DI lifetimes and assembly resolution order unless the task asks otherwise.
- Prefer small, reviewable extraction, naming, duplication reduction and simplification; avoid unrelated edits or unnecessary dependencies.
- Build affected projects and run relevant tests when possible; call out Revit-host-only verification and any intentional behavior change.
