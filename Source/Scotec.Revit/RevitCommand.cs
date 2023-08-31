// Copyright © 2023 Olaf Meyer
// Copyright © 2023 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autofac;
using Autofac.Extensions.DependencyInjection;

namespace Scotec.Revit;

public abstract class RevitCommand : IExternalCommand, IFailuresPreprocessor, IFailuresProcessor
{
    /// <summary>
    /// The command name that appears in Revit's undo list.
    /// </summary>
    protected abstract string CommandName { get; }

    /// <inheritdoc />
    Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            var document = commandData.Application.ActiveUIDocument?.Document;

            using var scope = RevitApp.GetServiceProvider(commandData.Application.ActiveAddInId.GetGUID())
                                      .GetAutofacRoot()
                                      .BeginLifetimeScope(builder =>
                                      {
                                          if (document != null)
                                          {
                                              builder.RegisterInstance(document).ExternallyOwned();
                                          }

                                          if (commandData.View != null)
                                          {
                                              builder.RegisterInstance(commandData.View).ExternallyOwned();
                                          }

                                          builder.RegisterInstance(commandData.Application).ExternallyOwned();
                                          builder.RegisterInstance(commandData.JournalData).ExternallyOwned();
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
    FailureProcessingResult IFailuresPreprocessor.PreprocessFailures(FailuresAccessor failuresAccessor)
    {
        return OnPreprocessFailures(failuresAccessor);
    }

    /// <inheritdoc />
    FailureProcessingResult IFailuresProcessor.ProcessFailures(FailuresAccessor data)
    {
        return OnProcessFailures(data);
    }

    /// <inheritdoc />
    public virtual void Dismiss(Document document)
    {
        OnDismiss(document);
    }

    /// <summary>
    ///     This method can be overwritten to dismiss any possible pending failure UI that may
    ///     have left on the screen in case of exception or document destruction.
    /// </summary>
    protected virtual void OnDismiss(Document document)
    {
    }

    protected virtual FailureProcessingResult OnPreprocessFailures(FailuresAccessor failuresAccessor)
    {
        return FailureProcessingResult.Continue;
    }

    protected virtual FailureProcessingResult OnProcessFailures(FailuresAccessor data)
    {
        return FailureProcessingResult.Continue;
    }

    protected abstract Result OnExecute(ExternalCommandData commandData, IServiceProvider ervices);
}
