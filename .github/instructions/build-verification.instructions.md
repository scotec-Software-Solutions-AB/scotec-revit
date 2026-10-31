---
description: Build and verification workflow for Scotec.Revit changes.
applyTo: "**/*"
---

# Build verification

- Discover actual `.sln`/`.slnx` and project files under `Source/` before choosing commands; use build scripts under `Build/` if the repository requires them.
- Confirm available .NET SDK, Revit reference dependencies and target framework(s).
- Build affected projects first, then related tests; for shared public contracts, isolation or DI infrastructure changes, attempt broader validation.
- Check compatibility with the repository's supported Revit versions and build configurations as documented by the project files.
- Distinguish test failures from environment limitations (e.g., Revit unavailable); report exact commands and results, never inferred success.
