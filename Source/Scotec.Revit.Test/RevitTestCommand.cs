// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Scotec.Revit.Isolation;

namespace Scotec.Revit.Test;

public class Test
{

}


[RevitCommandIsolation(ContextName = "Scotec.Revit.Test")]
[RevitTransactionMode(RevitTransactionMode.None)]
public class RevitTestCommand : RevitCommand
{
    protected override RevitTransactionMode TransactionMode { get; } = RevitTransactionMode.TransactionGroup;
    protected override string CommandName => "TestCommand";

    protected virtual void BeforeExecute(ExternalCommandData commandData, Test? test, ElementSet elements)
    {
    }

    protected virtual void AfterExecute(ExternalCommandData commandData, ElementSet elements)
    {
    }

    protected Result OnExecute(Document document)
    {
        return Result.Succeeded;
    }

    protected override Result OnExecute(ExternalCommandData commandData, IServiceProvider services)
    {
        return Result.Succeeded;
    }
}
