// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit;

/// <summary>
///     Marks a method as the availability-check entry point for a <see cref="RevitCommandAvailability" />.
/// </summary>
/// <remarks>
///     Apply this attribute to a method that should be called by the framework to determine whether the associated
///     ribbon button is enabled. The method must return <c>bool</c>. Its parameters are resolved from the
///     availability check's DI scope; <see cref="UIApplication" /> and <see cref="CategorySet" /> are passed through
///     directly. Only one method per type hierarchy may carry this attribute.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class RevitCommandAvailabilityCheckAttribute : Attribute
{
}

/// <summary>
///     Represents an abstract base class that determines the availability of external commands in Autodesk Revit.
/// </summary>
public abstract class RevitCommandAvailability : IExternalCommandAvailability
{
    private static readonly Type[] StandardIsCommandAvailableWithoutServiceProviderSignature =
        [typeof(UIApplication), typeof(CategorySet)];

    /// <summary>
    ///     Determines whether an external command is available for execution in the current Revit context.
    /// </summary>
    /// <param name="uiApplication">
    ///     The <see cref="Autodesk.Revit.UI.UIApplication" /> instance providing access to the current Revit application and
    ///     its data.
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
    bool IExternalCommandAvailability.IsCommandAvailable(UIApplication uiApplication, CategorySet selectedCategories)
    {
        try
        {
            var application = uiApplication.Application;
            var uiDocument = uiApplication.ActiveUIDocument;
            var document = uiDocument?.Document;
            var view = uiDocument?.ActiveView;

            using var scope = RevitAppBase.GetServiceProvider(uiApplication.ActiveAddInId.GetGUID())
                                          .GetAutofacRoot()
                                          .BeginLifetimeScope(builder =>
                                          {
                                              builder.RegisterInstance(uiApplication).ExternallyOwned();

                                              if (application is not null)
                                              {
                                                  builder.RegisterInstance(application).ExternallyOwned();
                                              }

                                              if (uiDocument is not null)
                                              {
                                                  builder.RegisterInstance(uiDocument).ExternallyOwned();
                                              }

                                              if (document is not null)
                                              {
                                                  builder.RegisterInstance(document).ExternallyOwned();
                                              }

                                              if (view is not null)
                                              {
                                                  builder.RegisterInstance(view).ExternallyOwned();
                                              }

                                              // Allow derived classes to add services
                                              var services = new ServiceCollection();
                                              ConfigureServices(services);
                                              builder.Populate(services);
                                          });

            var serviceProvider = scope.Resolve<IServiceProvider>();
            return InvokeIsCommandAvailable(uiApplication, selectedCategories, serviceProvider);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    ///     Allows derived classes to add services to the DI container for the command's lifetime scope.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to which services can be added.</param>
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
    [Obsolete(
        "This method is obsolete. Override IsCommandAvailable(UIApplication, CategorySet) instead, or declare a custom IsCommandAvailable overload with DI-resolved parameters.")]
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
    ///     Invokes the availability-check entry point. If a method marked with
    ///     <see cref="RevitCommandAvailabilityCheckAttribute" /> exists in the type hierarchy it is invoked with
    ///     DI-resolved parameters. Otherwise falls back to the standard
    ///     <see cref="IsCommandAvailable(UIApplication, CategorySet)" /> override, and finally to the obsolete
    ///     <see cref="IsCommandAvailable(UIApplication, CategorySet, IServiceProvider)" /> for backward compatibility.
    ///     Throws <see cref="InvalidOperationException" /> if more than one method carries the attribute.
    /// </summary>
    private bool InvokeIsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories,
                                          IServiceProvider serviceProvider)
    {
        // Prefer a method explicitly marked with [RevitCommandAvailabilityCheck].
        var attributedCheck = RevitReflectionHelper.FindSingleAttributedMethod<RevitCommandAvailabilityCheckAttribute>(GetType(), typeof(RevitCommandAvailability), typeof(bool));
        if (attributedCheck is not null)
        {
            return (bool)RevitReflectionHelper.Invoke(this, attributedCheck, serviceProvider,
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
            m => m.GetParameters()
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
