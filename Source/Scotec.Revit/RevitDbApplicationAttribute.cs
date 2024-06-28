// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

namespace Scotec.Revit;

/// <summary>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RevitDbApplicationAttribute : Attribute
{
}

public class test : IExternalDBApplication
{
    public ExternalDBApplicationResult OnStartup(ControlledApplication application)
    {
        throw new NotImplementedException();
    }

    public ExternalDBApplicationResult OnShutdown(ControlledApplication application)
    {
        throw new NotImplementedException();
    }
}