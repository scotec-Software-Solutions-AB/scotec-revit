---
applyTo: "**/*.{cs,csproj,xaml,addin,props,targets}"
description: Revit 2025+ C# add-in coding instructions
---

# Revit 2025+ Project Instructions

Apply these instructions when working on Revit add-in code, project files, XAML UI, add-in manifests, build files, and packaging files.

---

## Revit Version

Assume Revit 2025 or newer unless the project indicates otherwise.

Use modern Revit API patterns and avoid obsolete APIs.

---

## C# and .NET

- Use modern C# that is supported by the project.
- Respect the configured target framework.
- Keep nullable reference type warnings meaningful.
- Do not introduce unsupported runtime APIs.
- Preserve existing multi-targeting or Revit-year-specific build logic.

---

## Revit API Constraints

- Revit API access must stay on Revit’s valid API context.
- Do not call Revit API from background threads.
- Use `ExternalEvent` for modeless UI.
- Use transactions for document modifications.
- Do not keep long-lived references to Revit `Element` objects.
- Prefer DTOs for UI and service boundaries.

---

## WPF

- Use MVVM.
- Keep views simple.
- Avoid direct Revit API access from XAML code-behind unless unavoidable.
- Do not block the UI thread.
- Dispose event subscriptions.

---

## Build and Packaging

- Do not change Revit version constants without an explicit request.
- Do not change package version ranges without an explicit request.
- Keep `.addin` manifests consistent with assembly names and paths.
- Respect existing build output and installer conventions.
