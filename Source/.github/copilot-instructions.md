<!--
SPDX-FileCopyrightText: Copyright © 2026 Olaf Meyer
SPDX-FileCopyrightText: Copyright © 2026 scotec Software Solutions AB
SPDX-License-Identifier: MIT
-->

# GitHub Copilot – Common Repository Instructions

These instructions define the common behavior for GitHub Copilot in this repository.

They apply to generated code, code changes, reviews, refactorings, tests, documentation, and architecture suggestions unless a more specific instruction, agent, or skill overrides them.

---

## General Principles

- Prioritize correctness first, then maintainability, then readability.
- When constraints compete, favor the higher-priority item unless the task explicitly requires a different trade-off.
- Prefer simple, explicit solutions over clever or overly abstract designs.
- Follow the existing project structure, naming, formatting, and coding style.
- Preserve existing behavior unless a change was explicitly requested.
- Keep changes focused and avoid unrelated refactoring.
- Do not invent APIs, framework behavior, or project conventions.
- Prefer production-ready code over prototypes.

---

## Code Quality

Always prefer:

- Clear and descriptive names.
- Small methods with a single responsibility.
- Explicit null handling.
- Guard clauses for invalid input.
- Early returns over deeply nested control flow.
- Strong typing over stringly typed code.
- Immutable data structures where practical.
- Explicit dependencies.
- Readable code that is easy to review and maintain.

Avoid:

- Hidden global state.
- Unnecessary static classes.
- Magic strings or magic numbers.
- Silent exception handling.
- Commented-out code.
- Dead code.
- Over-engineering.
- Reflection unless clearly justified.
- Dynamic typing unless required by the framework or interop scenario.

---

## Architecture

- Respect the existing architecture.
- Keep dependencies explicit.
- Separate business logic from infrastructure and UI code.
- Prefer testable services over logic embedded in entry points.
- Use interfaces when they improve testability, isolation, or extensibility.
- Avoid tight coupling between unrelated components.
- Keep public APIs stable unless a breaking change is explicitly requested.
- Isolate version-specific code where supported framework or product versions differ.

---

## Error Handling

- Never swallow exceptions silently.
- Catch exceptions only when adding value, recovering, rolling back, or translating the error.
- Use `throw;` not `throw ex;` when rethrowing to preserve the original stack trace.
- Prefer specific exception types.
- Provide useful user-facing error messages.
- Log technical details when logging infrastructure exists.
- Do not log secrets or sensitive data.

---

## Performance

- Avoid obvious inefficiencies.
- Avoid repeated expensive operations in loops.
- Avoid unnecessary allocations.
- Use efficient data structures.
- Cache reusable data only when it is safe and meaningful.
- Do not optimize prematurely at the cost of readability.

---

## Testing

When generating or modifying logic:

- Keep business logic independently testable.
- Avoid hard dependencies on external systems.
- Use dependency injection where appropriate.
- Add or update tests when behavior changes.
- Prefer deterministic tests.
- Do not make tests depend on execution order.
- Keep unit tests focused and readable.

---

## Documentation

- Add XML documentation for public APIs when useful.
- Comments should explain why something is done, not simply what the code does.
- Keep documentation aligned with the implementation.
- Do not add noisy or redundant comments.
- Update README or developer documentation when behavior, setup, or usage changes.

---

## Security

- Never expose secrets, credentials, tokens, private keys, or connection strings.
- Validate external input.
- Avoid insecure defaults.
- Be careful with serialization, reflection, file paths, and process execution.
- Treat file paths, command-line arguments, environment variables, and network input as untrusted.
- Do not log sensitive data.

---

## Dependencies

- Prefer the standard library and existing project dependencies.
- Introduce new dependencies only when justified.
- Keep dependency usage minimal and explicit.
- Respect the project target framework and package versions.
- Do not upgrade package versions unless the task explicitly requires it.

---

## Version Awareness

- Respect the repository’s configured target frameworks and language version.
- Do not use APIs unavailable for the target runtime.
- This project supports Revit 2025, 2026, and 2027 via named build configurations and conditional compilation symbols (`REVIT2025`, `REVIT2026`, `REVIT2027`).
- When an API differs between supported versions, isolate the version-specific code.
- Prefer conditional compilation only when necessary and already used by the project.

---

## When Modifying Existing Code

- Minimize the scope of the change.
- Match existing style and patterns.
- Preserve public APIs unless a breaking change is explicitly required.
- Do not reformat unrelated code.
- Do not change behavior silently.
- Explain important behavior changes in inline comments, commit messages, or the pull request description.

---

## When Generating Code

- Generate complete and compilable code whenever possible.
- Include required using statements.
- Use `using` declarations or statements for `IDisposable` objects.
- Prefer clear names and simple structure.
- Mention assumptions if context is incomplete.
- Highlight risks or edge cases when relevant.
- Prefer examples that fit the current project structure.

---

## Communication Style

- Be concise and practical.
- Explain important trade-offs.
- Call out assumptions.
- Mention risks and edge cases.
- Prefer concrete code over abstract advice.
- If uncertain, say so instead of inventing details.

---

## Goal

Produce professional, production-quality code that integrates cleanly into the existing codebase.
