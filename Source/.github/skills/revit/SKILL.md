---
name: Skill: Revit Add-In Expert
description: Use when implementing or modifying Revit Add-In behavior, external commands, external events, transactions, family handling, and BIM data workflows.
---


# Skill: Revit Add-In Expert

## Purpose

Use this skill when implementing or modifying Autodesk Revit Add-In functionality.

## Primary Goal

Maintain Revit API compliance above implementation convenience.

## Revit API

Always follow:

- Revit API threading and transaction model
- External command and external event lifecycle
- Document and application event handling
- Host-managed object lifetimes

Reference Revit API documentation sections in comments for non-obvious behavior.

Example:

```csharp
// Revit API: Element modifications require an open transaction
```

## Transactions

Respect:

- Transaction boundaries — never modify elements outside an open transaction
- Transaction names — use descriptive, user-visible names
- Sub-transactions where nested rollback is needed
- Transaction groups for multi-transaction undo grouping

Never simplify transaction handling if doing so changes Revit API semantics.

## Elements and Families

- Access elements through the active document only.
- Use filtered element collectors with explicit filters.
- Prefer built-in parameter access over string-based lookups.
- Respect family and family instance distinctions.
- Handle missing or null elements explicitly.

## External Commands

- Implement `IExternalCommand` correctly.
- Return `Result.Succeeded`, `Result.Failed`, or `Result.Cancelled` appropriately.
- Keep commands thin — delegate logic to services.
- Avoid long-running operations on the Revit API thread.

## External Events

- Use `IExternalEventHandler` for modeless dialog interactions.
- Raise events through `ExternalEvent.Raise()`.
- Handle event execution failures explicitly.
- Keep event handlers short and focused.

## BIM Data

- Preserve element and parameter data integrity.
- Validate data before writing to the model.
- Avoid unnecessary model modifications.
- Treat model data as sensitive to host constraints.

## Error Handling

Map errors to clear, user-facing messages where appropriate.

Avoid generic exceptions for Revit API-level behavior.

## Interoperability

Generated code must remain compatible with:

- Supported Revit versions as defined by the project target
- Existing loader and module registration conventions
