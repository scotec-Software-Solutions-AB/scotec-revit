---
description: Tests and host-dependent verification for Scotec.Revit framework code.
applyTo: "**/*Test*/**/*.cs"
---

# Scotec.Revit testing

- Check the actual test project's framework, assertions and naming style before writing tests; do not assume a test runner or mocking library.
- Write independent, deterministic Arrange/Act/Assert-style tests; await asynchronous operations and assert specific outcomes, faults and cancellation.
- Cover DI scope lifetime, command results, context tracking, resolver precedence, cancellation and disposal where applicable.
- Separate isolated library tests from behavior that requires Autodesk Revit. A mocked scheduler or API context is not proof of legal host execution.
- For ALC tests, exercise independently loaded assemblies, intentionally shared identity, resolution, resource handling and unloadability when supported.
- For dispatcher tests, cover exceptions, cancellation, reentrancy and stale context with host verification when needed.
- Report which tests actually ran and never imply Revit host integration was tested without executing it.
