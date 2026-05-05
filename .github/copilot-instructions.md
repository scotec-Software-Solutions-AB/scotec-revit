# GitHub Copilot – Common Agent Instructions

These instructions define the common behavior for GitHub Copilot and Copilot agents in this repository.

They apply to all generated, modified, reviewed, and refactored code unless a more specific agent instruction overrides them.

---

## General Principles

- Prioritize correctness, clarity, maintainability, and long-term readability.
- Prefer simple, explicit solutions over clever or overly abstract designs.
- Follow the existing project structure, naming, and coding style.
- Preserve existing behavior unless a change was explicitly requested.
- Keep changes focused and avoid unrelated refactoring.
- Do not invent APIs, framework behavior, or project conventions.

---

## Code Quality

Always prefer:

- Clear and descriptive names.
- Small methods with a single responsibility.
- Explicit null handling.
- Guard clauses for invalid input.
- Early returns over deeply nested control flow.
- Strong typing over stringly typed code.
- Immutability where practical.
- Production-ready code over prototypes.

Avoid:

- Hidden global state.
- Unnecessary static classes.
- Magic strings or magic numbers.
- Silent exception handling.
- Commented-out code.
- Dead code.
- Over-engineering.
- Reflection unless clearly justified.

---

## Architecture

- Respect the existing architecture.
- Keep dependencies explicit.
- Separate business logic from infrastructure and UI code.
- Prefer testable services over logic embedded in entry points.
- Use interfaces when they improve testability, isolation, or extensibility.
- Avoid tight coupling between unrelated components.

---

## Error Handling

- Never swallow exceptions silently.
- Catch exceptions only when adding value, recovering, rolling back, or translating the error.
- Preserve stack traces when rethrowing.
- Prefer specific exception types.
- Provide useful error messages for users and detailed information for logs.

---

## Performance

- Avoid obvious inefficiencies.
- Avoid repeated expensive operations in loops.
- Avoid unnecessary allocations.
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

---

## Documentation

- Add XML documentation for public APIs when useful.
- Comments should explain why something is done, not simply what the code does.
- Keep documentation aligned with the implementation.
- Do not add noisy or redundant comments.

---

## Security

- Never expose secrets, credentials, tokens, or private keys.
- Validate external input.
- Avoid insecure defaults.
- Be careful with serialization, reflection, file paths, and process execution.
- Do not log sensitive data.

---

## Dependencies

- Prefer the standard library and existing project dependencies.
- Introduce new dependencies only when justified.
- Keep dependency usage minimal and explicit.
- Respect the project target framework and package versions.

---

## Version Awareness

- Respect the repository’s configured target frameworks and language version.
- Do not use APIs unavailable for the target runtime.
- When an API differs between supported versions, isolate the version-specific code.

---

## When Modifying Existing Code

- Minimize the scope of the change.
- Match existing style and patterns.
- Preserve public APIs unless a breaking change is explicitly required.
- Do not reformat unrelated code.
- Do not change behavior silently.

---

## When Generating Code

- Generate complete and compilable code whenever possible.
- Include required using statements.
- Prefer clear names and simple structure.
- Mention assumptions if context is incomplete.
- Highlight risks or edge cases when relevant.

---

## Communication Style

- Be concise and practical.
- Explain important trade-offs.
- Call out assumptions.
- Mention risks and edge cases.
- Prefer concrete code over abstract advice.

---

## Goal

Produce professional, production-quality code that integrates cleanly into the existing codebase.
