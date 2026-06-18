---
description: xUnit testing rules, unit and integration testing strategy, and naming conventions.
applyTo: "**/*"
---


# Testing Instructions

## General Rules

- Use xUnit.
- Follow Arrange / Act / Assert.
- Test behavior, not implementation details.
- Add regression tests for bug fixes.
- Prefer descriptive test names.
- Keep tests independent and repeatable.

## Naming

Use:

```text
MethodName_StateUnderTest_ExpectedBehavior
```

Examples:

```text
GetObjectAsync_ObjectExists_ReturnsObject
GetObjectAsync_ObjectDoesNotExist_ThrowsObjectNotFoundException
ExportFamiliesAsync_PartialFailure_ReturnsUnexportedElements
```

## Unit Tests

Use unit tests for:

- Business rules
- Validation
- Mapping logic
- Error handling
- Protocol-specific decisions

## Integration Tests

Use integration tests for:

- EF Core query behavior
- Transactions
- Concurrency
- Database mappings
- Repository behavior
- Revit API adapter behavior
- BIM data export workflows

## Test Data

Prefer builders or fixtures.

Avoid large duplicated setup blocks.

Each test should make relevant data obvious.
