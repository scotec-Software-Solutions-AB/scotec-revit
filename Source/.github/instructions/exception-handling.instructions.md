---
description: Domain exception hierarchy, wrapping, logging, and Try-method rules.
applyTo: "**/*"
---


# Exception Handling Instructions

## General Rules

- Use the project domain exception hierarchy.
- Catch known domain exceptions first and rethrow them.
- Wrap unexpected exceptions in the project runtime exception type.
- Never swallow exceptions silently.
- Log before rethrowing where existing patterns require it.

## Domain Exceptions

Prefer specific domain exceptions for expected protocol and business failures.

Examples:

- Object not found
- Invalid argument
- Constraint violation
- Content already exists
- Runtime failure

Do not use generic exceptions for protocol-level behavior.

## Logging Exceptions

When logging exceptions:

- Pass the exception as the first argument.
- Use structured logging placeholders.
- Include repository and object identifiers where available.
- Do not log secrets or sensitive payload data.

## Try Methods

Only suppress exceptions inside clearly named `TryXxx` methods when the behavior is documented and expected.
