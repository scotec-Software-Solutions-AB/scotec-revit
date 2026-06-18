---
description: Structured logging, sensitive data handling, and logging context requirements.
applyTo: "**/*"
---


# Logging Instructions

## General Rules

- Use structured logging.
- Prefer named placeholders over interpolated strings.
- Log significant operation start and completion.
- Log not-found situations at warning level.
- Log expected domain errors according to existing project patterns.
- Log unexpected errors at error level.
- Log critical data consistency problems at critical level.

## Structured Logging

Prefer:

```csharp
Logger.LogInformation(
    "Updating object {ObjectId} in repository {RepositoryId}.",
    objectId,
    repositoryId);
```

Avoid:

```csharp
Logger.LogInformation($"Updating object {objectId} in repository {repositoryId}.");
```

## Sensitive Data

Never log:

- Passwords
- Tokens
- Connection strings
- Raw authorization headers
- Sensitive document content
- Personal data unless explicitly required and approved

## Context

Where available, include:

- Repository ID
- Object ID
- Node ID
- Operation name
- User or principal identifier when safe and relevant
