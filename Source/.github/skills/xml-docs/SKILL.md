---
name: Skill: C# XML Documentation Expert
description: Use when generating, reviewing, or updating XML documentation comments for public .NET APIs.
---


# Skill: C# XML Documentation Expert

## Purpose

Use this skill when generating, reviewing, or updating XML documentation comments for public .NET APIs.

## Primary Goals

Prioritize:

1. Accuracy
2. Clarity
3. Completeness
4. Consistency
5. Maintainability

Documentation must describe actual behavior, not intended behavior.

## General Rules

Document:

- Public classes
- Public interfaces
- Public records
- Public structs
- Public enums
- Public delegates
- Public methods
- Public properties
- Public events

Do not generate meaningless comments that simply repeat the member name.

## Summary

Every public API should contain a meaningful `<summary>`.

The summary should explain:

- What the member does
- Why it exists
- Important behavioral expectations

## Parameters

Document every parameter using `<param>`.

Describe:

- Purpose
- Expected values
- Special requirements
- Nullability expectations when relevant

## Return Values

Use `<returns>` for all non-void methods.

Describe:

- What is returned
- Nullability
- Special cases

## Exceptions

Use `<exception>` when callers should be aware of thrown exceptions.

Document:

- Domain exceptions
- Validation exceptions
- Important runtime exceptions

## Remarks

Use `<remarks>` for:

- Behavioral details
- Performance considerations
- Thread-safety information
- Specification references
- Usage constraints

## References

Use `<see>` and `<seealso>` where helpful.

Prefer strongly typed references:

```xml
<see cref="Scotec.Revit.IRevitContext"/>
```
