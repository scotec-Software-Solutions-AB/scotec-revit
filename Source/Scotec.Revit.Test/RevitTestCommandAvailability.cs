// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Scotec.Revit.Isolation;

namespace Scotec.Revit.Test;

<<<<<<< HEAD
[RevitCommandAvailabilityIsolation(ContextName = "Scotec.Revit.Test")]
public class RevitTestCommandAvailability : RevitCommandAvailability
{
    protected override bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
=======
[Isolation.RevitCommandAvailabilityIsolation(ContextName = "Scotec.Revit.Test")]
public class RevitTestCommandAvailability : RevitCommandAvailability
{
    protected bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
>>>>>>> origin/main
    {
        return true;
    }

    protected override bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories,
                                               IServiceProvider services)
    {
        return true;
    }
}
