---
name: Skill: .NET 8 / C# Expert
description: Use when generating, reviewing, refactoring, or optimizing .NET 8 and C# code.
---


# Skill: .NET 8 / C# Expert

## Purpose

Use this skill when generating, reviewing, refactoring, or optimizing .NET 8 and C# code.

## Principles

- Generate production-ready code.
- Follow existing project patterns before introducing new approaches.
- Prefer maintainability and readability.
- Use modern C# language features when they improve clarity.

## Language Features

Prefer:

- File-scoped namespaces
- Nullable reference types
- Collection expressions (`[]`)
- Pattern matching
- Primary constructors where appropriate
- Required members
- Global usings only when already used by the project
- `nameof()` instead of string literals

Avoid:

- Legacy coding patterns
- Dynamic typing unless required
- Reflection-based solutions when strongly typed alternatives exist

## Async

- Use async/await for all I/O operations.
- Propagate `CancellationToken`.
- Never use `.Result` or `.Wait()`.
- Avoid `async void` except for event handlers.

## Dependency Injection

- Prefer constructor injection.
- Do not inject `IServiceProvider`.
- Avoid Service Locator patterns.
- Design services for testability.

## Error Handling

- Throw meaningful exceptions.
- Preserve stack traces.
- Never swallow exceptions.
- Log before rethrowing when appropriate.

## Performance

- Avoid premature optimization.
- Minimize allocations in hot paths.
- Prefer `Span<T>`, `Memory<T>`, and pooled resources when profiling indicates benefit.
- Use `ValueTask` only when justified.

## Code Generation Expectations

Before generating code:

1. Reuse existing abstractions.
2. Follow project conventions.
3. Generate complete implementations.
4. Include XML documentation for public APIs when consistent with the project.
