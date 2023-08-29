using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autofac;
using Autofac.Extensions.DependencyInjection;

namespace Scotec.Revit;

public abstract class CommandAvailability : IExternalCommandAvailability
{
    public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
    {
        try
        {
            var document = applicationData.ActiveUIDocument.Document;

            using var scope = RevitApp.GetServiceProvider(applicationData.ActiveAddInId.GetGUID())
                                      .GetAutofacRoot()
                                      .BeginLifetimeScope(builder =>
                                      {
                                          builder.RegisterInstance(document).ExternallyOwned();
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

    protected abstract bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories, IServiceProvider services);
}
