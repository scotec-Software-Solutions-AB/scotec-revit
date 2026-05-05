# Copilot Revit 2025+ Setup

This archive is intended for a repository or solution where the `.sln` file is located directly inside the `Source` folder.

Copy the included `Source/.github` folder into your existing `Source` folder.

Expected structure:

```text
Source/
  YourSolution.sln
  .github/
    copilot-instructions.md
    agents/
      revit-csharp-expert.agent.md
    instructions/
      revit.instructions.md
    skills/
      revit-2025-api/
        SKILL.md
```

## Files

### `.github/copilot-instructions.md`

Common repository-level Copilot instructions.

### `.github/agents/revit-csharp-expert.agent.md`

A specialized Copilot agent for experienced C# / Autodesk Revit 2025+ development.

### `.github/instructions/revit.instructions.md`

Path-scoped Revit instructions for C#, project, XAML, add-in manifest, props, and targets files.

### `.github/skills/revit-2025-api/SKILL.md`

A Revit 2025+ API skill with implementation patterns, templates, and review checklists.
