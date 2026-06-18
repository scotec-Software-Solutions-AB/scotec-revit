---
description: Build, rebuild, test, and reporting requirements after code changes.
applyTo: "**/*"
---


# Build Verification Instructions

## Purpose

Use these instructions after any code change.

## Required Verification

After modifying source code:

1. Restore packages if required.
2. Build the affected project.
3. Build the full solution when shared components are modified.
4. Run affected tests when available.
5. Fix all compilation errors.
6. Report remaining warnings.
7. Do not mark a task as completed if the build fails.

## Full Solution Build Required

Build the full solution when changes affect:

- Shared libraries
- Abstractions
- Public APIs
- Dependency injection registrations
- Source generators
- Package references
- Common data models
- EF Core mappings
- Build props or targets
- Cross-project contracts

## Reporting

After verification, report:

- What was built
- Whether the build succeeded
- Which tests were run
- Remaining warnings or failures
