// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit;

/// <summary>
///     Specifies the transaction mode for a Revit command.
/// </summary>
/// <remarks>
///     This enumeration defines the different modes of transaction handling that can be used
///     when executing a Revit command. It allows specifying whether no transaction, a single transaction,
///     or a transaction group should be used.
/// </remarks>
public enum RevitTransactionMode
{
    /// <summary>
    ///     Indicates that no transaction is required for the Revit command.
    /// </summary>
    /// <remarks>
    ///     This mode specifies that the command does not require any transaction handling by the framework.
    ///     It is typically used for commands where the user intends to manage transactions manually
    ///     or when no modifications to the Revit document are necessary.
    /// </remarks>
    None,

    /// <summary>
    ///     Indicates that a single transaction is required for the execution of a Revit command.
    /// </summary>
    /// <remarks>
    ///     This mode ensures that all operations performed during the execution of the command
    ///     are encapsulated within a single transaction. This is useful for commands that require
    ///     atomicity and consistency in their operations.
    /// </remarks>
    SingleTransaction,

    /// <summary>
    ///     Indicates that a transaction group is required.
    /// </summary>
    /// <remarks>
    ///     A transaction group allows multiple transactions to be grouped together, enabling them to be committed or rolled
    ///     back as a single unit.
    ///     This mode is useful for commands that involve multiple operations that need to be treated as a single logical
    ///     transaction.
    /// </remarks>
    TransactionGroup,


    /// <summary>
    ///     Specifies that the transaction should be rolled back after execution.
    /// </summary>
    /// <remarks>
    ///     When this mode is used, all changes made within the transaction are rolled back
    ///     upon completion of the command. This ensures that no changes are permanently applied
    ///     to the Revit model, providing a safe way to execute commands that may require temporary
    ///     modifications.
    /// </remarks>
    SingleTransactionRollback,

    /// <summary>
    ///     Specifies a transaction group mode with rollback behavior for a Revit command.
    /// </summary>
    /// <remarks>
    ///     When this mode is used, all changes made within the transaction group are rolled back
    ///     upon completion of the command. This ensures that no changes are permanently applied
    ///     to the Revit model, providing a safe way to execute commands that may require temporary
    ///     modifications.
    /// </remarks>
    TransactionGroupRollback
}

/// <summary>
///     An attribute used to specify the transaction mode for a Revit command.
/// </summary>
/// <remarks>
///     This attribute allows associating a specific <see cref="RevitTransactionMode" /> with a Revit command,
///     indicating how transactions should be handled during the execution of the command.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class RevitTransactionModeAttribute : Attribute
{
    /// <summary>
    ///     Gets or sets the transaction mode for the associated Revit command.
    /// </summary>
    /// <value>
    ///     A value of type <see cref="RevitTransactionMode" /> that specifies how transactions
    ///     should be handled during the execution of the command. The default value is
    ///     <see cref="RevitTransactionMode.SingleTransaction" />.
    /// </value>
    /// <remarks>
    ///     This property allows configuring the transaction handling mode for a Revit command.
    ///     It can be set to <see cref="RevitTransactionMode.None" />, <see cref="RevitTransactionMode.SingleTransaction" />,
    ///     or <see cref="RevitTransactionMode.TransactionGroup" /> depending on the desired behavior.
    /// </remarks>
    public RevitTransactionMode Mode { get; set; } = RevitTransactionMode.SingleTransaction;
}

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
///     During execution, this class creates a new scope for the DI container and adds the current <see cref="Document"/>
///     if available, the current <see cref="View"/> if available, the <see cref="Autodesk.Revit.UI.UIApplication"/>,
///     and the JournalData.
///     Override <see cref="ConfigureServices"/> to apply custom services to the current scope.
/// </remarks>
[RevitTransactionMode(Mode = RevitTransactionMode.SingleTransaction)]
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
    ///     This property is deprecated. Use the RevitTransactionModeAttribute to specify the transaction mode for your command class instead.
    /// </remarks>
    [Obsolete("This property is deprecated. Use the RevitTransactionModeAttribute to specify the transaction mode for your command class instead.")]
    protected bool NoTransaction { get; set; }

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

                                              // Allow derived classes to add services
                                              var services = new ServiceCollection();
                                              ConfigureServices(services);
                                              builder.Populate(services);
                                          });

            var transactionMode = GetTransactionMode();
            var serviceProvider = scope.Resolve<IServiceProvider>();
            if (document == null || transactionMode == RevitTransactionMode.None)
            {
                // Skip transaction management if no document is open or transaction is not required.
                return OnExecute(commandData, serviceProvider);
            }

            switch (transactionMode)
            {
                case RevitTransactionMode.SingleTransaction:
                case RevitTransactionMode.SingleTransactionRollback:
                {
                    using var transaction = new Transaction(document);
                    transaction.Start(CommandName);

                    var failureHandlingOptions = transaction.GetFailureHandlingOptions();
                    failureHandlingOptions.SetFailuresPreprocessor(this);
                    transaction.SetFailureHandlingOptions(failureHandlingOptions);

                    var result = OnExecute(commandData, serviceProvider);
                    
                    // Do not commit on error or in rollback mode.
                    if(result == Result.Succeeded && transactionMode == RevitTransactionMode.SingleTransaction)
                    {
                        transaction.Commit();
                    }

                    return result;
                }
                case RevitTransactionMode.TransactionGroup:
                case RevitTransactionMode.TransactionGroupRollback:
                {
                    using var transactionGroup = new TransactionGroup(document);
                    transactionGroup.Start(CommandName);

                    var result = OnExecute(commandData, serviceProvider);

                    // Do not commit on error or in rollback mode.
                    if (result == Result.Succeeded && transactionMode == RevitTransactionMode.TransactionGroup)
                    {
                        transactionGroup.Assimilate();
                    }

                    return result;
                }
            }

            return Result.Failed;
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
    void IFailuresProcessor.Dismiss(Document document)
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

    private RevitTransactionMode GetTransactionMode()
    {
#pragma warning disable CS0618 // NoTransaction is obsolete
        // We do not want to alter the behavior of legacy code.
        // Therefore, if NoTransaction is set to true, we ignore the attribute and return RevitTransactionMode.None.
        if (NoTransaction)
        {
            return RevitTransactionMode.None;
        }
#pragma warning restore CS0618
        // Try to get the RevitTransactionModeAttribute
        var type = GetType();
        var attr = (RevitTransactionModeAttribute?)Attribute.GetCustomAttribute(type, typeof(RevitTransactionModeAttribute));
        if (attr != null) // Since this attribute is assigned to the base class, it should always be present.
        {
            return attr.Mode;
        }

        // Return the default value.
        return RevitTransactionMode.SingleTransaction;
    }
}
