---
description: Developer documentation, XML documentation, and user-facing documentation rules.
applyTo: "**/*"
---


# Documentation Instructions

## General Rules

- Keep documentation accurate and concise.
- Describe actual behavior, not intended behavior.
- Do not generate empty or repetitive comments.
- Use consistent terminology.
- Prefer concrete examples over vague descriptions.

## XML Documentation

Public APIs should have meaningful XML documentation when the project requires it.

Document:

- Public classes
- Public interfaces
- Public methods
- Public properties
- Public records
- Public enums

Include:

- `summary`
- `param`
- `returns`
- `exception`
- `remarks` where useful

## User Documentation

When writing user-facing documentation:

- Start with the purpose.
- Explain the workflow.
- Include examples.
- Avoid implementation details unless relevant.
- Keep wording clear and professional.
