// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Reflection;
using System.Runtime.Loader;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Scotec.Revit.Test;

[Isolation.RevitCommandAvailabilityIsolation(ContextName = "Scotec.Revit.Test")]
public class ShowTestDialogCommandAvailability : RevitCommandAvailability
{
    protected bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
    {
        return true;
    }

    protected override bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories,
                                               IServiceProvider services)
    {
        var context = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());
        return true;
    }
}
