// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Scotec.Revit;

namespace Scotec.Revit.Test;

[RevitCommandAvailabilityAttribute]
public class RevitTestCommandAvailability : RevitCommandAvailability
{
    protected override bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories,
                                             IServiceProvider services)
    {
        return true;
    }
}
