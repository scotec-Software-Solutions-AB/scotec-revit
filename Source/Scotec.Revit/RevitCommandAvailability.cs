// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit;

/// <summary>
///     Represents an abstract base class that determines the availability of external commands in Autodesk Revit.
/// </summary>
public abstract class RevitCommandAvailability : IExternalCommandAvailability
{
    private static readonly Type[] StandardIsCommandAvailableSignature =
        [typeof(UIApplication), typeof(CategorySet), typeof(IServiceProvider)];
    private static readonly Type[] StandardIsCommandAvailableWithoutServiceProviderSignature =
        [typeof(UIApplication), typeof(CategorySet)];

    /// <summary>
    ///     Determines whether an external command is available for execution in the current Revit context.
    /// </summary>
    /// <param name="applicationData">
    ///     The <see cref="Autodesk.Revit.UI.UIApplication" /> instance providing access to the current Revit application and its data.
    /// </param>
    /// <param name="selectedCategories">
    ///     A <see cref="CategorySet" /> containing the categories of the selected elements in the Revit document.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the command is available for execution; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This method is part of the <see cref="IExternalCommandAvailability" /> interface and is implemented to
    ///     determine the availability of external commands based on the current Revit environment, selected elements,
    ///     and application state.
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

                                              var view = applicationData.ActiveUIDocument?.ActiveView;
                                              if (view != null)
                                              {
                                                  builder.RegisterInstance(view).ExternallyOwned();
                                              }

                                              builder.RegisterInstance(applicationData).ExternallyOwned();
                                              builder.RegisterInstance(applicationData.Application).ExternallyOwned();
                                              builder.RegisterInstance(selectedCategories).ExternallyOwned();

                                              // Allow derived classes to add services
                                              var services = new ServiceCollection();
                                              ConfigureServices(services);
                                              builder.Populate(services);
                                          });

            var serviceProvider = scope.Resolve<IServiceProvider>();
            return InvokeIsCommandAvailable(applicationData, selectedCategories, serviceProvider);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    ///     Allows derived classes to add services to the DI container for the command's lifetime scope.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which services can be added.</param>
    /// <remarks>
    ///     Override this method in derived classes to register additional services required for the command.
    ///     The base implementation does not add any service to the DI container.
    /// </remarks>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Derived classes can override to add services.
    }

    /// <summary>
    ///     Determines whether the external command is available for execution in the current Revit context.
    /// </summary>
    /// <param name="applicationData">
    ///     The <see cref="UIApplication" /> instance providing access to the current Revit application and its data.
    /// </param>
    /// <param name="selectedCategories">
    ///     A <see cref="CategorySet" /> containing the categories of the selected elements in the Revit document.
    /// </param>
    /// <param name="services">
    ///     An <see cref="IServiceProvider" /> instance that provides access to application services and dependencies.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the command is available for execution; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     This method is obsolete. Override <see cref="IsCommandAvailable(UIApplication, CategorySet)" /> instead,
    ///     or declare a custom <c>IsCommandAvailable</c> overload with DI-resolved parameters, which the framework will
    ///     discover and invoke automatically.
    ///     This method will not be called if a custom <c>IsCommandAvailable</c> overload is provided in the derived class.
    /// </remarks>
    [Obsolete("This method is obsolete. Override IsCommandAvailable(UIApplication, CategorySet) instead, or declare a custom IsCommandAvailable overload with DI-resolved parameters.")]
    protected virtual bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories, IServiceProvider services)
    {
        return true;
    }

    /// <summary>
    ///     Determines whether the external command is available for execution in the current Revit context.
    /// </summary>
    /// <param name="applicationData">
    ///     The <see cref="UIApplication" /> instance providing access to the current Revit application and its data.
    /// </param>
    /// <param name="selectedCategories">
    ///     A <see cref="CategorySet" /> containing the categories of the selected elements in the Revit document.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the command is available for execution; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     Overriding this method is not recommended. Prefer declaring a custom <c>IsCommandAvailable</c> overload
    ///     with DI-resolved parameters, which the framework will discover and invoke automatically.
    ///     This method will not be called if a custom <c>IsCommandAvailable</c> overload is provided in the derived class.
    /// </remarks>
    protected virtual bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
    {
        return true;
    }

    /// <summary>
    ///     Checks whether the derived class declares an <c>IsCommandAvailable</c> overload whose parameter types differ
    ///     from the two standard signatures. If such an overload exists, all parameters are resolved from the
    ///     <paramref name="serviceProvider"/> (with <see cref="UIApplication"/> and <see cref="CategorySet"/> passed
    ///     directly) and the overload is invoked via reflection. Otherwise
    ///     <see cref="IsCommandAvailable(UIApplication, CategorySet)"/> is called if it is overridden in the derived
    ///     class; if not, the obsolete <see cref="IsCommandAvailable(UIApplication, CategorySet, IServiceProvider)"/>
    ///     is called for backward compatibility.
    /// </summary>
    private bool InvokeIsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories,
                                          IServiceProvider serviceProvider)
    {
        // Look for an IsCommandAvailable overload whose parameter list differs from both standard signatures.
        var customMethod = RevitReflectionHelper.FindMethod(
            GetType(), typeof(RevitCommandAvailability), "IsCommandAvailable", typeof(bool),
            predicate: m => !m.GetParameters()
                              .Select(p => p.ParameterType)
                              .SequenceEqual(StandardIsCommandAvailableSignature)
                           && !m.GetParameters()
                              .Select(p => p.ParameterType)
                              .SequenceEqual(StandardIsCommandAvailableWithoutServiceProviderSignature));

        if (customMethod is not null)
        {
            return (bool)RevitReflectionHelper.Invoke(this, customMethod, serviceProvider,
                new Dictionary<Type, object>
                {
                    [typeof(UIApplication)] = applicationData,
                    [typeof(CategorySet)] = selectedCategories,
                    [typeof(IServiceProvider)] = serviceProvider
                })!;
        }

        // Call IsCommandAvailable(UIApplication, CategorySet) if the derived class overrides it.
        var standardOverride = RevitReflectionHelper.FindMethod(
            GetType(), typeof(RevitCommandAvailability), "IsCommandAvailable", typeof(bool),
            predicate: m => m.GetParameters()
                             .Select(p => p.ParameterType)
                             .SequenceEqual(StandardIsCommandAvailableWithoutServiceProviderSignature));

        if (standardOverride is not null)
        {
            return IsCommandAvailable(applicationData, selectedCategories);
        }

        // Fall back to the obsolete overload for backward compatibility.
#pragma warning disable CS0618
        return IsCommandAvailable(applicationData, selectedCategories, serviceProvider);
#pragma warning restore CS0618
    }
}
