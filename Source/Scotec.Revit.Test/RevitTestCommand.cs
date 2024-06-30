// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

namespace Scotec.Revit.Test;

[RevitCommandIsolation]
[Transaction(TransactionMode.Manual)]
public class RevitTestCommand : RevitCommand
{
    protected override string CommandName => "TestCommand";

    protected override Result OnExecute(ExternalCommandData commandData, IServiceProvider services)
    {
        return Result.Succeeded;
    }
}
