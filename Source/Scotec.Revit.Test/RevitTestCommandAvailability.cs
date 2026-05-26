// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using System.Reflection;
using System.Runtime.Loader;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Scotec.Revit.Isolation;

namespace Scotec.Revit.Test;

[RevitCommandAvailabilityIsolation(ContextName = "Scotec.Revit.Test")]
public class RevitTestCommandAvailability : RevitCommandAvailability
{
    protected bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
    {
        return true;
    }

    protected override bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories,
                                               IServiceProvider services)
    {
        return true;
    }
}
