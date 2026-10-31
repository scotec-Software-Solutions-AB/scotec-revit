<!--
SPDX-FileCopyrightText: Copyright © 2026 Olaf Meyer
SPDX-FileCopyrightText: Copyright © 2026 scotec Software Solutions AB
SPDX-License-Identifier: MIT
-->

# GitHub Copilot instructions — Scotec.Revit

Scotec.Revit is a reusable .NET 8 library/framework for Autodesk Revit add-ins, **not** a specific family-management product. Work against the current repository source and documentation, not assumptions from other scotec projects.

## Repository orientation

- Repository: https://github.com/scotec-Software-Solutions-AB/scotec-revit
- Start with `README.md`, `CONTRIBUTING.md`, `Source/`, `Documentation/`, `Build/`, `GenerateTest/`, and the relevant project files. Never assume a solution filename or installed Revit versions without checking.
- Follow the actual target framework, referenced Revit API assemblies, compiler settings, test framework, `NuGet.config`, `GitVersion.yml`, and `Scotec StyleGuide.DotSettings`.
- Public framework contracts are consumed by external add-ins; preserve source/binary compatibility unless the task explicitly calls for a breaking change. Explain changes in public behavior, API signatures, lifetimes, and supported Revit versions.

## Framework-specific principles

- Preserve `RevitApp`/`RevitDbApp` lifecycle behavior and `[RevitStartup]` / `[RevitShutdown]` injection semantics.
- Respect `RevitCommand` transaction modes, failure processing and per-invocation DI scopes.
- For `RevitEventHandler<>`, `RevitUpdater`, and command-availability checks, respect their registration, execution contexts, lifetimes and cleanup behavior.
- Use `RevitTaskDispatcher` / external events for work initiated outside an allowed Revit API context; do not assume UI thread access alone makes Revit document modification legal.
- Distinguish scoped `IRevitContext` / `IRevitUiContext` from long-lived `IGlobalRevitContext` / `IGlobalRevitUiContext`. Do not hold event- or command-scoped objects after their scope ends.
- Treat `AssemblyLoadContext` isolation as a first-class requirement. Avoid accidental type identity conflicts, Default ALC leakage, unintentional shared dependencies, or changes to resolver precedence. Consider Revit's own loading behavior per supported version.
- Cross-add-in requests must establish valid Revit context; inspect `RevitContextTracker` and `IRevitScopeFactory` rather than assuming the caller's context is reusable.
- Do not bypass the host's transaction, event, document, threading, or startup/shutdown restrictions.

## Implementation quality

- Inspect existing implementations and nearby tests first. Follow current naming, nullability, dependency injection, logging, documentation and style conventions.
- Prefer minimal, focused changes; avoid unrelated reformatting, speculative abstractions and new packages unless justified.
- Catch exceptions only to recover, translate or add context; preserve stack traces; never suppress errors or log secrets or sensitive model content.
- Do not invent APIs, behavior, feature support, or benchmark/test results. Verify implementation details in code when uncertain.
- Document public APIs with useful XML summaries, parameter/return/exception contracts where appropriate; keep samples aligned with actual signatures.

## Verification

- Build affected projects and run targeted tests where possible; run broader tests for shared contracts, ALC handling, lifecycle or infrastructure changes.
- Identify Revit-host-dependent behavior requiring manual/integration verification when no Revit runtime is available.
- Report what was actually built/tested, what could not be run, any compatibility risks and remaining assumptions.
