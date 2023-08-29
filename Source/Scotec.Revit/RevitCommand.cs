using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autofac;
using Autofac.Extensions.DependencyInjection;

namespace Scotec.Revit;

public abstract class RevitCommand : IExternalCommand, IFailuresPreprocessor, IFailuresProcessor
{
    protected abstract string CommandName { get; }

    /// <inheritdoc />
    /// >
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            var document = commandData.Application.ActiveUIDocument.Document;

            using var scope = RevitApp.GetServiceProvider(commandData.Application.ActiveAddInId.GetGUID())
                                      .GetAutofacRoot()
                                      .BeginLifetimeScope(builder =>
                                      {
                                          builder.RegisterInstance(document).ExternallyOwned();
                                          builder.RegisterInstance(commandData.Application).ExternallyOwned();
                                      });

            var serviceProvider = scope.Resolve<IServiceProvider>();
            using var transaction = new Transaction(document);
            transaction.Start(CommandName);

            var failureHandlingOptions = transaction.GetFailureHandlingOptions();
            failureHandlingOptions.SetFailuresPreprocessor(this);
            transaction.SetFailureHandlingOptions(failureHandlingOptions);

            var result = OnExecute(commandData, serviceProvider);
            if (result == Result.Succeeded)
            {
                transaction.Commit();
            }

            return result;
        }
        catch (Exception)
        {
            return Result.Failed;
        }
    }

    /// <inheritdoc />
    /// >
    public virtual FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
    {
        return FailureProcessingResult.Continue;
    }

    /// <inheritdoc />
    /// >
    public virtual FailureProcessingResult ProcessFailures(FailuresAccessor data)
    {
        return FailureProcessingResult.Continue;
    }

    /// <inheritdoc />
    /// >
    public virtual void Dismiss(Document document)
    {
    }

    protected abstract Result OnExecute(ExternalCommandData commandData, IServiceProvider ervices);
}
