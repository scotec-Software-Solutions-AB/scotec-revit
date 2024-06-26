﻿// Copyright © 2023 - 2024 Olaf Meyer
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
public abstract class RevitCommand : IExternalCommand, IFailuresPreprocessor, IFailuresProcessor
{
    /// <summary>
    ///     The command name that appears in Revit's undo list.
    /// </summary>
    protected abstract string CommandName { get; }

    /// <summary>
    ///     Should be set to true for commands working on application level only.
    ///     You can set this property to true as well if you need the transaction to be handled by user code.
    /// </summary>
    /// <remarks>If <c>NoTransaction</c> is set to true, no failure processing takes place.</remarks>
    protected bool NoTransaction { get; set; }

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
            if (document == null || NoTransaction)
            {
                // No open document or no transaction requested. Therefore we cannot create a transaction.
                return OnExecute(commandData, serviceProvider);
            }

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

    /// <summary>
    ///     This method can be overwritten to handle failures found at the end of a transaction and Revit is about to start
    ///     processing them.
    /// </summary>
    /// <seealso cref="Autodesk.Revit.DB.IFailuresPreprocessor.PreprocessFailures" />
    protected virtual FailureProcessingResult OnPreprocessFailures(FailuresAccessor failuresAccessor)
    {
        return FailureProcessingResult.Continue;
    }

    /// <summary>
    ///     This method can be overwritten to handle failures found at the end of a transaction and Revit is processing them.
    /// </summary>
    /// <seealso cref="Autodesk.Revit.DB.IFailuresProcessor.ProcessFailures" />
    protected virtual FailureProcessingResult OnProcessFailures(FailuresAccessor data)
    {
        return FailureProcessingResult.Continue;
    }

    /// <summary>
    /// </summary>
    /// <param name="commandData"></param>
    /// <param name="services"></param>
    /// <returns></returns>
    protected abstract Result OnExecute(ExternalCommandData commandData, IServiceProvider services);
}
