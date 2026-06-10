---
name: revit-api
description: Practical guidance for implementing and reviewing Autodesk Revit 2025+ C# add-ins.
---

<!--
SPDX-FileCopyrightText: Copyright © 2026 Olaf Meyer
SPDX-FileCopyrightText: Copyright © 2026 scotec Software Solutions AB
SPDX-License-Identifier: MIT
-->

# Revit 2025+ API Skill

Use this skill when the task involves Autodesk Revit 2025 or newer API code, Revit add-in architecture, Revit commands, transactions, element filtering, WPF UI in Revit, add-in packaging, or Revit assembly loading.

---

## When to Use This Skill

Use this skill for tasks involving:

- `Autodesk.Revit.DB`
- `Autodesk.Revit.UI`
- `IExternalCommand`
- `IExternalApplication`
- `ExternalEvent`
- `Transaction`
- `FilteredElementCollector`
- Revit parameters
- Revit units
- Revit selection
- Revit WPF dialogs, dockable panes, or ribbon UI
- `.addin` manifests
- Multi-version Revit builds
- Revit add-in assembly loading or isolation

---

## Core Rules

1. Revit API calls must happen only in a valid Revit API context.
2. Do not access Revit API objects from background threads.
3. Modify documents only inside a valid transaction.
4. Keep external commands and applications thin.
5. Move real logic into services.
6. Use DTOs between Revit API logic and UI logic.
7. Avoid stale `Element` references.
8. Use `UniqueId` for persisted element identity.
9. Avoid unnecessary `Document.Regenerate()`.
10. Do not invent Revit API members.

---

## Implementation Workflow

When implementing a Revit feature:

1. Identify whether the operation is read-only or modifies the document.
2. Validate the Revit context:
   - `UIApplication`
   - `UIDocument`
   - `Document`
   - selected elements, if required
3. Collect elements efficiently.
4. Convert Revit API objects into DTOs if data crosses into UI or non-Revit layers.
5. Open a transaction only for document modifications.
6. Commit or roll back safely.
7. Return useful user-facing messages.
8. Log technical details if logging exists.

---

## External Command Template

Use this pattern for new external commands:

```csharp
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Company.Product.Commands;

[Transaction(TransactionMode.Manual)]
public sealed class SampleCommand : IExternalCommand
{
    public Result Execute(
        ExternalCommandData commandData,
        ref string message,
        ElementSet elements)
    {
        var uiApplication = commandData.Application;
        var uiDocument = uiApplication.ActiveUIDocument;

        if (uiDocument is null)
        {
            message = "No active Revit document is available.";
            return Result.Failed;
        }

        try
        {
            var document = uiDocument.Document;

            // Delegate real work to a service.
            var service = new SampleService();
            service.Execute(document);

            return Result.Succeeded;
        }
        catch (OperationCanceledException)
        {
            return Result.Cancelled;
        }
        catch (Exception exception)
        {
            message = exception.Message;
            return Result.Failed;
        }
    }
}
```

---

## Transaction Template

Use this pattern for write operations:

```csharp
using var transaction = new Transaction(document, "Update Revit data");
transaction.Start();

try
{
    // Modify the document here.

    transaction.Commit();
}
catch
{
    if (transaction.GetStatus() == TransactionStatus.Started)
    {
        transaction.RollBack();
    }

    throw;
}
```

Do not use a transaction for read-only operations.

---

## Efficient Element Collection

Prefer fast filters first:

```csharp
var elements = new FilteredElementCollector(document)
    .OfCategory(BuiltInCategory.OST_Walls)
    .WhereElementIsNotElementType()
    .ToElements();
```

Prefer class filters when possible:

```csharp
var walls = new FilteredElementCollector(document)
    .OfClass(typeof(Wall))
    .WhereElementIsNotElementType()
    .Cast<Wall>()
    .ToList();
```

Avoid:

- Extracting geometry unless necessary.
- Repeated parameter lookup in large loops.
- LINQ expressions that hide expensive Revit API calls.
- Collecting all elements and filtering in memory when Revit filters can be used.

---

## Parameter Access

When reading parameters:

```csharp
var parameter = element.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);

if (parameter is null || parameter.StorageType != StorageType.String)
{
    return;
}

var value = parameter.AsString();
```

When writing parameters:

```csharp
var parameter = element.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);

if (parameter is null || parameter.IsReadOnly || parameter.StorageType != StorageType.String)
{
    return;
}

parameter.Set("New value");
```

Rules:

- Prefer `BuiltInParameter` over name lookup.
- Check for null.
- Check `IsReadOnly`.
- Check `StorageType`.
- Use culture-invariant formatting for persisted values.

---

## Units

Use modern Revit unit APIs:

```csharp
var displayValue = UnitUtils.ConvertFromInternalUnits(internalValue, unitTypeId);
var internalValue = UnitUtils.ConvertToInternalUnits(displayValue, unitTypeId);
```

Rules:

- Use `ForgeTypeId`-based APIs.
- Avoid obsolete `DisplayUnitType`.
- Be explicit about units when converting values.

---

## Modeless UI

For modeless WPF UI:

- Do not call the Revit API directly from the modeless window.
- Use `ExternalEvent`.
- Keep the external event handler small.
- Pass data through DTOs or request objects.
- Avoid storing long-lived `Element` references in view models.

---

## Dockable Panes

For dockable panes:

- Use a WPF `Page` where Revit requires it.
- Keep Revit API access behind services or external events.
- Avoid heavy initialization during registration.
- Carefully dispose resources and event handlers.

---

## Assembly Loading

When working on Revit add-ins:

- Do not accidentally load add-in dependencies into unintended assembly load contexts.
- Be careful with WPF resources, themes, and controls across load contexts.
- Prefer separating entry assemblies from UI assemblies when isolation matters.
- Avoid duplicate loading of shared WPF assemblies unless explicitly intended.
- Do not weaken existing isolation infrastructure.

---

## Multi-Version Revit Code

When the project supports several Revit versions:

```csharp
#if REVIT2027
// Revit 2027-specific code
#elif REVIT2026
// Revit 2026-specific code
#else
// Revit 2025-compatible code
#endif
```

Rules:

- Keep version-specific sections small.
- Prefer adapters over duplicated implementations.
- Do not change build constants unless requested.
- Do not change package version ranges unless requested.

---

## Review Checklist

When reviewing Revit code, check for:

- Missing or unnecessary transactions.
- Revit API access from background threads.
- Long-running work on Revit’s UI thread.
- Inefficient collectors.
- Incorrect parameter access.
- Obsolete Revit API usage.
- Stale element references.
- Incorrect use of `ElementId` for persistence (prefer `UniqueId`).
- Missing active document checks.
- Missing cleanup of events and subscriptions.
- UI code tightly coupled to Revit API objects.
- Assembly loading or WPF resource issues.
- Poor user-facing error messages.

---

## Output Expectations

When generating code:

- Include required `using` statements.
- Make examples compile as far as project context allows.
- Use Revit 2025+ API assumptions.
- Mention assumptions when project context is incomplete.
- Prefer safe, maintainable code over short snippets.
