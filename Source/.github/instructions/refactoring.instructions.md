---
description: Safe refactoring rules that preserve behavior, APIs, database behavior, and Revit API semantics.
applyTo: "**/*"
---


# Refactoring Guidelines

## Purpose

Use these instructions when improving existing code structure without changing externally observable behavior.

## General Rules

- Refactor incrementally.
- Preserve behavior.
- Preserve public APIs unless explicitly requested.
- Preserve database behavior.
- Preserve Revit API behavior.
- Preserve logging and security behavior.
- Keep changes small and reviewable.
- Do not combine unrelated refactorings.

## Priorities

1. Readability
2. Maintainability
3. Testability
4. Consistency
5. Performance

## Preferred Refactorings

- Extract Method
- Extract Class
- Introduce Interface
- Remove Duplication
- Simplify Conditional Logic
- Replace Magic Values with Constants
- Improve Naming
- Improve Dependency Injection
- Reduce Method Complexity
- Reduce Class Responsibilities

## Avoid

- Large-scale rewrites.
- Changing multiple architectural concepts at once.
- Introducing new frameworks.
- Introducing new libraries.
- Modifying public contracts without approval.
- Reformatting unrelated code.

## Verification

All refactorings must:

- Compile successfully.
- Pass existing affected tests where available.
- Preserve existing behavior.
- Preserve public APIs unless explicitly requested.
