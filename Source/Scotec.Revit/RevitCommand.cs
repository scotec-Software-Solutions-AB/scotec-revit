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
    Transaction,

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
    TransactionWithRollback,

    /// <summary>
    ///     Specifies a transaction group mode with rollback behavior for a Revit command.
    /// </summary>
    /// <remarks>
    ///     When this mode is used, all changes made within the transaction group are rolled back
    ///     upon completion of the command. This ensures that no changes are permanently applied
    ///     to the Revit model, providing a safe way to execute commands that may require temporary
    ///     modifications.
    /// </remarks>
    TransactionGroupWithRollback,

    /// <summary>
    ///     Specifies that the command should operate in read-only mode.
    /// </summary>
    /// <remarks>
    ///     When this mode is used, the command is executed without any transaction and is expected
    ///     to perform only read operations on the Revit model. No modifications are allowed.
    ///     This is useful for commands that query or analyze the model without making changes.
    /// </remarks>
    ReadOnly
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
    ///     Initializes a new instance of the <see cref="RevitTransactionModeAttribute" /> class with the default transaction mode.
    /// </summary>
    /// <remarks>
    ///     This constructor sets the <see cref="Mode" /> property to its default value, 
    ///     which is <see cref="RevitTransactionMode.Transaction" />.
    /// </remarks>
    public RevitTransactionModeAttribute()
    {
        
    }
    /// <summary>
    ///     Initializes a new instance of the <see cref="RevitTransactionModeAttribute" /> class with the specified transaction mode.
    /// </summary>
    /// <param name="mode">
    ///     The <see cref="RevitTransactionMode" /> to be associated with the Revit command, specifying how transactions
    ///     should be handled during the execution of the command.
    /// </param>
    public RevitTransactionModeAttribute(RevitTransactionMode mode)
    {
        Mode = mode;
    }
    /// <summary>
    ///     Gets or sets the transaction mode for the associated Revit command.
    /// </summary>
    /// <value>
    ///     A value of type <see cref="RevitTransactionMode" /> that specifies how transactions
    ///     should be handled during the execution of the command. The default value is
    ///     <see cref="RevitTransactionMode.Transaction" />.
    /// </value>
    /// <remarks>
    ///     This property allows configuring the transaction handling mode for a Revit command.
    ///     It can be set to <see cref="RevitTransactionMode.None" />, <see cref="RevitTransactionMode.Transaction" />,
    ///     or <see cref="RevitTransactionMode.TransactionGroup" /> depending on the desired behavior.
    /// </remarks>
    public RevitTransactionMode Mode { get; set; } = RevitTransactionMode.Transaction;
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
[RevitTransactionMode(Mode = RevitTransactionMode.Transaction)]
public abstract class RevitCommand : IExternalCommand, IFailuresPreprocessor, IFailuresProcessor
{
    private static readonly Type[] StandardOnExecuteSignatureWithServiceProvider = [typeof(ExternalCommandData), typeof(IServiceProvider)];
    private static readonly Type[] StandardOnExecuteWithElementSetSignature = [typeof(ExternalCommandData), typeof(ElementSet)];

    /// <summary>
    ///     Gets the name of the Revit command.
    /// </summary>
    /// <remarks>
    ///     This property is used as the transaction name when executing the command within a transaction.
    ///     It should be overridden in derived classes to provide a meaningful name for the command.
    /// </remarks>
    protected abstract string CommandName { get; }

    /// <summary>
    ///     Gets or sets the default transaction mode for the command.
    /// </summary>
    /// <value>
    ///     A <see cref="RevitTransactionMode" /> value specifying how transactions are handled during command execution.
    ///     The default value is <see cref="RevitTransactionMode.Transaction" />.
    /// </value>
    /// <remarks>
    ///     This property is used as the fallback transaction mode when no <see cref="RevitTransactionModeAttribute" />
    ///     is applied to the command class. Override this property in derived classes to set a different default
    ///     without using the attribute.
    /// </remarks>
    protected virtual RevitTransactionMode TransactionMode { get; } = RevitTransactionMode.Transaction;

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
    ///     the transaction lifecycle if a document is available. 
    /// </remarks>
    /// <exception cref="System.Exception">
    ///     Thrown if an unhandled exception occurs during command execution, resulting in a
    ///     <see cref="Autodesk.Revit.UI.Result.Failed" />.
    /// </exception>
    Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiApplication = commandData.Application;
        var application = uiApplication.Application;
        var uiDocument = uiApplication.ActiveUIDocument;
        var document = uiDocument?.Document;
        var view = uiDocument?.ActiveView;

        using var scope = RevitAppBase.GetServiceProvider(commandData.Application.ActiveAddInId.GetGUID())
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
            
        var transactionMode = GetTransactionMode();
        var serviceProvider = scope.Resolve<IServiceProvider>();

        // Call BeforeExecute before any transaction is opened.
        InvokeOptionalMethod("BeforeExecute", commandData, elements, serviceProvider);

        Result result;

        // Skip transaction management if no document is open or transaction is not required.
        if (document is null || transactionMode == RevitTransactionMode.None || transactionMode == RevitTransactionMode.ReadOnly)
        {
            result = InvokeOnExecute(commandData, elements, serviceProvider);
        }
        else
        {
            result = transactionMode switch
            {
                RevitTransactionMode.Transaction or RevitTransactionMode.TransactionWithRollback
                    => ExecuteTransaction(commandData, elements, document, serviceProvider, transactionMode),
                RevitTransactionMode.TransactionGroup or RevitTransactionMode.TransactionGroupWithRollback
                    => ExecuteTransactionGroup(commandData, elements, document, serviceProvider, transactionMode),
                _ => Result.Failed
            };
        }

        // Call AfterExecute after the transaction has been closed.
        InvokeOptionalMethod("AfterExecute", commandData, elements, serviceProvider);

        return result;
    }

    private Result ExecuteTransactionGroup(ExternalCommandData commandData, ElementSet elements, Document document, IServiceProvider serviceProvider,
                                           RevitTransactionMode transactionMode)
    {
        using var transactionGroup = new TransactionGroup(document);
        transactionGroup.Start(CommandName);

        var result = InvokeOnExecute(commandData, elements, serviceProvider);

        // Do not commit on error or in rollback mode.
        if (result == Result.Succeeded && transactionMode == RevitTransactionMode.TransactionGroup)
        {
            transactionGroup.Assimilate();
        }

        return result;
    }

    private Result ExecuteTransaction(ExternalCommandData commandData, ElementSet elements, Document document, IServiceProvider serviceProvider, RevitTransactionMode transactionMode)
    {
        using var transaction = new Transaction(document);
        transaction.Start(CommandName);

        var failureHandlingOptions = transaction.GetFailureHandlingOptions();
        failureHandlingOptions.SetFailuresPreprocessor(this);
        transaction.SetFailureHandlingOptions(failureHandlingOptions);

        var result = InvokeOnExecute(commandData, elements, serviceProvider);

        // Do not commit on error or in rollback mode.
        if (result == Result.Succeeded && transactionMode == RevitTransactionMode.Transaction)
        {
            transaction.Commit();
        }

        return result;
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
    ///     This method is obsolete. Override <see cref="OnExecute(ExternalCommandData, ElementSet)" /> instead,
    ///     or declare a custom <c>OnExecute</c> overload with DI-resolved parameters, which the framework will
    ///     discover and invoke automatically.
    ///     This method will not be called if a custom <c>OnExecute</c> overload is provided in the derived class.
    /// </remarks>
    [Obsolete("This method is obsolete. Override OnExecute(ExternalCommandData, ElementSet) instead, or declare a custom OnExecute overload with DI-resolved parameters.")]
    protected virtual Result OnExecute(ExternalCommandData commandData, IServiceProvider services)
    {
        return Result.Succeeded;
    }

    /// <summary>
    ///     Executes the core logic of the Revit external command.
    /// </summary>
    /// <param name="commandData">
    ///     Provides access to the Revit application and its associated data.
    /// </param>
    /// <param name="elements">
    ///     A set of elements that can be used to highlight or identify problematic elements in case of failure.
    /// </param>
    /// <returns>
    ///     A <see cref="Autodesk.Revit.UI.Result" /> value indicating the outcome of the command execution.
    /// </returns>
    /// <remarks>
    ///     Overriding this method is not recommended. Prefer declaring a custom <c>OnExecute</c> overload
    ///     with DI-resolved parameters, which the framework will discover and invoke automatically.
    ///     This method will not be called if a custom <c>OnExecute</c> overload is provided in the derived class.
    /// </remarks>
    protected virtual Result OnExecute(ExternalCommandData commandData, ElementSet elements)
    {
        return Result.Succeeded;
    }

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
        if (attr is not null) // Since this attribute is assigned to the base class, it should always be present.
        {
            return attr.Mode;
        }

        // Return the default value.
        return TransactionMode;
    }

    /// <summary>
    ///     Looks for a <c>void</c>-returning method with the given <paramref name="methodName"/> on the derived class,
    ///     resolves its parameters from the <paramref name="serviceProvider"/> (passing <see cref="ExternalCommandData"/>
    ///     directly), and invokes it. Does nothing if no such method exists on the derived type.
    /// </summary>
    /// <param name="methodName">The name of the method to find and invoke (e.g. <c>BeforeExecute</c> or <c>AfterExecute</c>).</param>
    /// <param name="commandData">The current <see cref="ExternalCommandData"/> instance.</param>
    /// <param name="elements">The <see cref="ElementSet"/> for the current command execution.</param>
    /// <param name="serviceProvider">The scoped <see cref="IServiceProvider"/> for the current command execution.</param>
    private void InvokeOptionalMethod(string methodName, ExternalCommandData commandData, ElementSet elements, IServiceProvider serviceProvider)
    {
        var method = RevitReflectionHelper.FindMethod(GetType(), typeof(RevitCommand), methodName, typeof(void));
        if (method is null)
        {
            return;
        }

        RevitReflectionHelper.Invoke(this, method, serviceProvider,
            new Dictionary<Type, object>
            {
                [typeof(ExternalCommandData)] = commandData,
                [typeof(ElementSet)] = elements,
                [typeof(IServiceProvider)] = serviceProvider,
            });
    }

    /// <summary>
    ///     Checks whether the derived class declares an <c>OnExecute</c> overload whose parameter types differ from
    ///     the two standard signatures. If such an overload exists, all parameters are resolved from the
    ///     <paramref name="serviceProvider" /> (with <see cref="ExternalCommandData" /> being passed directly) and the
    ///     overload is invoked via reflection. Otherwise <see cref="OnExecute(ExternalCommandData, ElementSet)" /> is
    ///     called if it is overridden in the derived class; if not, the obsolete
    ///     <see cref="OnExecute(ExternalCommandData, IServiceProvider)" /> is called for backward compatibility.
    /// </summary>
    /// <param name="commandData">The current <see cref="ExternalCommandData" /> instance.</param>
    /// <param name="serviceProvider">The scoped <see cref="IServiceProvider" /> for the current command execution.</param>
    /// <param name="elements">The <see cref="ElementSet" /> for the current command execution.</param>
    /// <returns>A <see cref="Result" /> indicating the outcome of the command execution.</returns>
    private Result InvokeOnExecute(ExternalCommandData commandData, ElementSet elements, IServiceProvider serviceProvider)
    {
        // Look for an OnExecute overload whose parameter list differs from both standard signatures.
        var customOnExecute = RevitReflectionHelper.FindMethod(
            GetType(), typeof(RevitCommand), "OnExecute", typeof(Result),
            predicate: m => !m.GetParameters()
                              .Select(p => p.ParameterType)
                              .SequenceEqual(StandardOnExecuteSignatureWithServiceProvider)
                           && !m.GetParameters()
                              .Select(p => p.ParameterType)
                              .SequenceEqual(StandardOnExecuteWithElementSetSignature));

        if (customOnExecute is not null)
        {
            return (Result)RevitReflectionHelper.Invoke(this, customOnExecute, serviceProvider,
                new Dictionary<Type, object>
                {
                    [typeof(ExternalCommandData)] = commandData,
                    [typeof(IServiceProvider)] = serviceProvider,
                    [typeof(ElementSet)] = elements
                })!;
        }

        // Call OnExecute(ExternalCommandData, ElementSet) if the derived class overrides it.
        var elementSetOverride = RevitReflectionHelper.FindMethod(
            GetType(), typeof(RevitCommand), "OnExecute", typeof(Result),
            predicate: m => m.GetParameters()
                             .Select(p => p.ParameterType)
                             .SequenceEqual(StandardOnExecuteWithElementSetSignature));

        if (elementSetOverride is not null)
        {
            return OnExecute(commandData, elements);
        }

        // Fall back to the obsolete overload for backward compatibility.
#pragma warning disable CS0618
        return OnExecute(commandData, serviceProvider);
#pragma warning restore CS0618
    }
}
