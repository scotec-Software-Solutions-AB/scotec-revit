// Copyright © 2023 - 2025 Olaf Meyer
// Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autofac;
using Autofac.Extensions.DependencyInjection;

namespace Scotec.Revit;

/// <summary>
///     Represents an abstract base class for Revit commands, implementing the
///     <see cref="Autodesk.Revit.UI.IExternalCommand" />,
///     <see cref="Autodesk.Revit.DB.IFailuresPreprocessor" />, and <see cref="Autodesk.Revit.DB.IFailuresProcessor" />
///     interfaces.
/// </summary>
/// <remarks>
///     This class provides a framework for handling Revit external commands, including failure preprocessing and
///     processing.
///     It includes methods that can be overridden to customize behavior during command execution and failure handling.
/// </remarks>
public abstract class RevitCommand : IExternalCommand, IFailuresPreprocessor, IFailuresProcessor
{
    /// <summary>
    ///     Gets the name of the Revit command.
    /// </summary>
    /// <remarks>
    ///     This property is used as the transaction name when executing the command within a transaction.
    ///     It should be overridden in derived classes to provide a meaningful name for the command.
    /// </remarks>
    protected abstract string CommandName { get; }

    /// <summary>
    ///     Gets or sets a value indicating whether the command should execute without creating a transaction.
    /// </summary>
    /// <remarks>
    ///     When set to <c>true</c>, the command execution bypasses the automatic creation of a Revit transaction.
    ///     This allows the command to handle transactions explicitly, providing greater control over transaction
    ///     management during execution.
    /// </remarks>
    protected bool NoTransaction { get; set; }

    /// <summary>
    ///     Executes the external command within the Revit environment.
    /// </summary>
    /// <param name="commandData">
    ///     An object that provides access to the Revit application and its associated data.
    /// </param>
    /// <param name="message">
    ///     A message that can be set by the command to provide additional information in case of failure.
    /// </param>
    /// <param name="elements">
    ///     A set of elements that can be used to highlight or identify problematic elements in case of failure.
    /// </param>
    /// <returns>
    ///     A <see cref="Autodesk.Revit.UI.Result" /> value indicating the outcome of the command execution.
    /// </returns>
    /// <remarks>
    ///     This method is the entry point for Revit external commands. It initializes the required services and manages
    ///     the transaction lifecycle if a document is available. Override <see cref="OnExecute" /> to implement custom
    ///     command logic.
    /// </remarks>
    /// <exception cref="System.Exception">
    ///     Thrown if an unhandled exception occurs during command execution, resulting in a
    ///     <see cref="Autodesk.Revit.UI.Result.Failed" />.
    /// </exception>
    Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            var document = commandData.Application.ActiveUIDocument?.Document;

            using var scope = RevitAppBase.GetServiceProvider(commandData.Application.ActiveAddInId.GetGUID())
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
                // Skip transaction management if no document is open or transaction is not required.
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

    /// <summary>
    ///     Preprocesses failures encountered during a transaction in Revit.
    /// </summary>
    /// <param name="failuresAccessor">
    ///     An instance of <see cref="Autodesk.Revit.DB.FailuresAccessor" /> that provides access to the failures encountered
    ///     during the transaction.
    /// </param>
    /// <returns>
    ///     A <see cref="Autodesk.Revit.DB.FailureProcessingResult" /> value indicating how Revit should proceed with the
    ///     failures.
    /// </returns>
    /// <remarks>
    ///     This method is invoked by Revit to allow custom handling of failures before they are processed.
    ///     The default implementation delegates to the <see cref="OnPreprocessFailures" /> method, which can be overridden in
    ///     derived classes to customize behavior.
    /// </remarks>
    FailureProcessingResult IFailuresPreprocessor.PreprocessFailures(FailuresAccessor failuresAccessor)
    {
        return OnPreprocessFailures(failuresAccessor);
    }

    /// <summary>
    ///     Processes failures encountered during a transaction in Revit.
    /// </summary>
    /// <param name="data">
    ///     An instance of <see cref="Autodesk.Revit.DB.FailuresAccessor" /> that provides access to the failures encountered.
    /// </param>
    /// <returns>
    ///     A <see cref="Autodesk.Revit.DB.FailureProcessingResult" /> value indicating how the failures should be handled.
    /// </returns>
    /// <remarks>
    ///     This method is invoked when Revit processes failures at the end of a transaction.
    ///     It delegates the handling of failures to the <see cref="OnProcessFailures" /> method, which can be overridden to
    ///     customize behavior.
    /// </remarks>
    FailureProcessingResult IFailuresProcessor.ProcessFailures(FailuresAccessor data)
    {
        return OnProcessFailures(data);
    }

    /// <summary>
    ///     Dismisses any possible pending failure UI that may have been left on the screen
    ///     in case of an exception or document destruction.
    /// </summary>
    /// <param name="document">
    ///     The <see cref="Autodesk.Revit.DB.Document" /> instance representing the Revit document
    ///     associated with the command.
    /// </param>
    /// <remarks>
    ///     This method invokes the <see cref="OnDismiss" /> method, which can be overridden
    ///     in derived classes to provide custom dismissal behavior.
    /// </remarks>
    public void Dismiss(Document document)
    {
        OnDismiss(document);
    }

    /// <summary>
    ///     Dismisses any possible pending failure UI that may have been left on the screen
    ///     in case of an exception or document destruction.
    /// </summary>
    /// <param name="document">
    ///     The <see cref="Autodesk.Revit.DB.Document" /> instance representing the Revit document
    ///     associated with the command.
    /// </param>
    /// <remarks>
    ///     This method can be overridden in derived classes to provide custom behavior for dismissing
    ///     failure UI or other cleanup tasks related to the document.
    /// </remarks>
    protected virtual void OnDismiss(Document document)
    {
    }

    /// <summary>
    ///     Handles failures encountered at the end of a transaction before Revit begins processing them.
    /// </summary>
    /// <param name="failuresAccessor">
    ///     An instance of <see cref="Autodesk.Revit.DB.FailuresAccessor" /> that provides access to the failures encountered
    ///     during the transaction.
    /// </param>
    /// <returns>
    ///     A <see cref="Autodesk.Revit.DB.FailureProcessingResult" /> value indicating how Revit should proceed with the
    ///     failures.
    /// </returns>
    /// <remarks>
    ///     This method can be overridden in derived classes to customize the handling of failures.
    ///     It is invoked by the default implementation of
    ///     <see cref="Autodesk.Revit.DB.IFailuresPreprocessor.PreprocessFailures" />.
    /// </remarks>
    protected virtual FailureProcessingResult OnPreprocessFailures(FailuresAccessor failuresAccessor)
    {
        return FailureProcessingResult.Continue;
    }

    /// <summary>
    ///     Handles failures encountered at the end of a transaction during Revit's failure processing.
    /// </summary>
    /// <param name="data">
    ///     An instance of <see cref="Autodesk.Revit.DB.FailuresAccessor" /> that provides access to the failures encountered.
    /// </param>
    /// <returns>
    ///     A <see cref="Autodesk.Revit.DB.FailureProcessingResult" /> value indicating how the failures should be processed.
    /// </returns>
    /// <remarks>
    ///     This method can be overridden in derived classes to customize the behavior for handling failures.
    ///     It is invoked by the <see cref="Autodesk.Revit.DB.IFailuresProcessor.ProcessFailures" /> implementation.
    /// </remarks>
    protected virtual FailureProcessingResult OnProcessFailures(FailuresAccessor data)
    {
        return FailureProcessingResult.Continue;
    }

    /// <summary>
    ///     Executes the core logic of the Revit external command.
    /// </summary>
    /// <param name="commandData">
    ///     Provides access to the Revit application and its associated data.
    /// </param>
    /// <param name="services">
    ///     A service provider that supplies dependencies required for the command execution.
    /// </param>
    /// <returns>
    ///     A <see cref="Autodesk.Revit.UI.Result" /> value indicating the outcome of the command execution.
    /// </returns>
    /// <remarks>
    ///     This method must be overridden in derived classes to implement the specific functionality of the command.
    ///     It is invoked by the <see cref="IExternalCommand.Execute" /> method, which manages the transaction lifecycle
    ///     and initializes the required services.
    /// </remarks>
    /// <exception cref="System.Exception">
    ///     Thrown if an unhandled exception occurs during the execution of the command logic.
    /// </exception>
    protected abstract Result OnExecute(ExternalCommandData commandData, IServiceProvider services);
}
