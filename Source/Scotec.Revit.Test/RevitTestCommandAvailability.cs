// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using System.Reflection;
using System.Runtime.Loader;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Scotec.Revit.Test;

[Isolation.RevitCommandAvailabilityIsolation]
public class RevitTestCommandAvailability : RevitCommandAvailability
{
    protected override bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories,
                                               IServiceProvider services)
    {
        var context = AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly());

        return true;
    }
}
