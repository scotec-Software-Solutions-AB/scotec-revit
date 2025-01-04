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
/// Represents an abstract base class that determines the availability of external commands in Autodesk Revit.
/// </summary>
public abstract class RevitCommandAvailability : IExternalCommandAvailability
{
    /// <summary>
    /// Determines whether an external command is available for execution in the current Revit context.
    /// </summary>
    /// <param name="applicationData">
    /// The <see cref="UIApplication"/> instance providing access to the current Revit application and its data.
    /// </param>
    /// <param name="selectedCategories">
    /// A <see cref="CategorySet"/> containing the categories of the selected elements in the Revit document.
    /// </param>
    /// <returns>
    /// <c>true</c> if the command is available for execution; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method is part of the <see cref="IExternalCommandAvailability"/> interface and is implemented to
    /// determine the availability of external commands based on the current Revit environment, selected elements,
    /// and application state.
    /// </remarks>
    bool IExternalCommandAvailability.IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
    {
        try
        {
            var document = applicationData.ActiveUIDocument?.Document;

            using var scope = RevitAppBase.GetServiceProvider(applicationData.ActiveAddInId.GetGUID())
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
    /// Determines whether the external command is available for execution in the current Revit context.
    /// </summary>
    /// <param name="applicationData">
    /// The <see cref="UIApplication"/> instance providing access to the current Revit application and its data.
    /// </param>
    /// <param name="selectedCategories">
    /// A <see cref="CategorySet"/> containing the categories of the selected elements in the Revit document.
    /// </param>
    /// <param name="services">
    /// An <see cref="IServiceProvider"/> instance that provides access to application services and dependencies.
    /// </param>
    /// <returns>
    /// <c>true</c> if the command is available for execution; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method must be implemented in derived classes to define the specific conditions under which the external
    /// command is available. It provides a mechanism to enable or disable commands based on the current Revit environment,
    /// selected elements, and application state.
    /// </remarks>
    protected abstract bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories, IServiceProvider services);
}
