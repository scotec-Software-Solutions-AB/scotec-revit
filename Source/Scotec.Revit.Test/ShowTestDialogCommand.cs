// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Scotec.Revit.Isolation;

namespace Scotec.Revit.Test;

[RevitCommandIsolation(ContextName = "Scotec.Revit.Test")]
[RevitTransactionMode(RevitTransactionMode.None)]
public class ShowTestDialogCommand : RevitCommand
{
    protected override RevitTransactionMode TransactionMode { get; } = RevitTransactionMode.TransactionGroup;
    protected override string CommandName => "ShowTestDialog";

    [RevitCommandBeforeExecute]
    protected virtual void Initialize(ExternalCommandData commandData, object? data, ElementSet elements)
    {
    }

    [RevitCommandAfterExecute]
    protected virtual void Cleanup(ExternalCommandData commandData, ElementSet elements)
    {
    }

    [RevitCommandExecute]
    protected Result Run(IRevitUiContext context, TestRevitDialog dialog) 
    {
        dialog.Show();
        return Result.Succeeded;
    }
}
