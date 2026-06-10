<!--
SPDX-FileCopyrightText: Copyright © 2026 Olaf Meyer
SPDX-FileCopyrightText: Copyright © 2026 scotec Software Solutions AB
SPDX-License-Identifier: MIT
-->

# GitHub Copilot Instructions

These instructions define the default behavior for GitHub Copilot and Copilot agents in this repository.

They apply to generated, modified, reviewed, and refactored code unless a more specific instruction overrides them.

---

## Primary Goal

Produce production-quality changes that integrate cleanly into this repository, preserve existing behavior, and remain compatible with the repository's .NET and Revit constraints.

---

## Instruction Precedence

When instructions conflict, apply this order:

1. Explicit user request.
2. More specific repository or folder-level instruction.
3. Existing local code patterns in the touched area.
4. These common instructions.

When two valid approaches are possible, prefer the one that is simpler, more local, and more consistent with nearby code.

---

## Repository Expectations

- Work from the code that already exists in the touched project, not from generic framework examples.
- Match the naming, file layout, nullability style, dependency style, and test style used nearby.
- Preserve public APIs and serialized shapes unless a breaking change is explicitly requested.
- Keep edits narrow. Do not combine feature work with cleanup or unrelated refactoring.
- Do not invent project conventions, helper abstractions, or infrastructure that are not already supported by the repository.

---

## Implementation Rules

Prefer:

- Clear, explicit code over clever or heavily abstracted code.
- Small focused methods and straightforward control flow.
- Strong typing, explicit null handling, and guard clauses.
- Dependency injection or existing seams when they improve testability.
- Business logic in testable units instead of entry points, UI glue, or infrastructure wrappers.

Avoid:

- Hidden global state.
- Unnecessary new abstractions, interfaces, or base classes.
- Silent exception handling.
- Reflection unless there is an established need in the codebase.
- Magic strings, magic numbers, dead code, and commented-out code.

Add a new abstraction only when it clearly reduces duplication, isolates a boundary already present in the design, or is required for testability.

---

## Revit and Host-Specific Rules

When working with Revit-facing code:

- Respect Revit API version boundaries and avoid assuming newer API surface is available.
- Keep version-specific behavior isolated when support differs across target versions.
- Do not move Revit API calls across transaction, document, threading, or host-lifecycle boundaries without clear evidence that the change is safe.
- Treat document state, external events, file paths, and model data as sensitive to host constraints and failure handling.
- Prefer small, explicit adapters around Revit-specific APIs rather than leaking host concerns through unrelated layers.

If the safety of a Revit API change cannot be verified from nearby code, keep the change conservative and state the assumption.

---

## Error Handling and Diagnostics

- Never swallow exceptions silently.
- Catch exceptions only to add context, recover, translate, or clean up.
- Preserve stack traces when rethrowing.
- Prefer specific exception types and actionable messages.
- Log useful diagnostics, but never log secrets, credentials, tokens, private keys, or sensitive model data.

---

## Dependencies and Compatibility

- Prefer the standard library and dependencies already used in the repository.
- Introduce new packages only when justified and only if they fit the target framework and repository conventions.
- Respect configured target frameworks, language versions, analyzers, and nullable settings.
- Do not use APIs that are unavailable in the targeted runtime or package version.

---

## Testing and Validation

For behavior changes, bug fixes, and non-trivial refactors:

- Add or update tests when there is an existing test location and a reasonable seam for the behavior.
- Prefer deterministic tests that exercise the changed behavior directly.
- Validate the touched slice with the narrowest relevant check available, such as a focused test, build, or type check.
- If validation cannot be run, say so explicitly and do not imply the change was verified.

Do not stop at code generation alone when a local validation step is available.

---

## Documentation and Comments

- Keep documentation aligned with the implementation.
- Add XML documentation for public APIs when the surrounding codebase uses it or when the contract is non-obvious.
- Comments should explain intent, constraints, or non-obvious decisions, not restate the code.
- Do not add noisy comments or tutorial-style explanations inside production code.

---

## Communication Style

- Be concise, concrete, and practical.
- Call out assumptions, risks, and edge cases when they matter.
- Prefer concrete code changes over abstract advice.
- When context is incomplete, state the assumption instead of inventing missing behavior.

---

## Definition of Done

A change is complete only when all of the following are true:

- The requested behavior is implemented or the requested analysis is complete.
- The change is limited to the necessary scope.
- Existing local patterns and compatibility constraints were respected.
- Relevant tests or validation were updated and run when feasible, or the lack of validation was stated explicitly.
- The final explanation includes important assumptions, risks, and any unverified areas.
