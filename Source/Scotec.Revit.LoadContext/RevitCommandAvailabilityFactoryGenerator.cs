// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using Microsoft.CodeAnalysis;

namespace Scotec.Revit.LoadContext;

[Generator]
internal class RevitCommandAvailabilityFactoryGenerator : RevitFactoryGeneratorBase
{
    protected override void OnInitialize()
    {
#if USE_OLD_ISOLATION_ATTRIBUTE
        RegisterSourceOutputForAttribute("Scotec.Revit.RevitCommandAvailabilityIsolationAttribute");
#else
        RegisterSourceOutputForAttribute("Scotec.Revit.Isolation.RevitCommandAvailabilityIsolationAttribute");
#endif
    }

    protected override string GetTemplateName()
    {
        return "RevitCommandAvailabilityFactory";
    }
}
