// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autofac;
using Autofac.Extensions.DependencyInjection;

namespace Scotec.Revit;

/// <summary>
/// </summary>
public abstract class CommandAvailability : IExternalCommandAvailability
{
    /// <inheritdoc />
    bool IExternalCommandAvailability.IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
    {
        try
        {
            var document = applicationData.ActiveUIDocument?.Document;

            using var scope = RevitApp.GetServiceProvider(applicationData.ActiveAddInId.GetGUID())
                                      .GetAutofacRoot()
                                      .BeginLifetimeScope(builder =>
                                      {
                                          if (document != null)
                                          {
                                              builder.RegisterInstance(document).ExternallyOwned();
                                          }

                                          builder.RegisterInstance(applicationData).ExternallyOwned();
                                          builder.RegisterInstance(applicationData.Application).ExternallyOwned();
                                          builder.RegisterInstance(selectedCategories).ExternallyOwned();
                                      });

            var serviceProvider = scope.Resolve<IServiceProvider>();
            var result = IsCommandAvailable(applicationData, selectedCategories, serviceProvider);

            return result;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    ///     Implement this method to provide control over whether your external command is enabled or disabled.
    /// </summary>
    protected abstract bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories, IServiceProvider services);
}
