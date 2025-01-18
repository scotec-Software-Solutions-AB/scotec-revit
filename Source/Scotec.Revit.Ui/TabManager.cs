// // Copyright © 2023 - 2024 Olaf Meyer
// // Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using System;
using System.Linq;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using RibbonPanel = Autodesk.Revit.UI.RibbonPanel;

namespace Scotec.Revit.Ui;

/// <summary>
///     Provides functionality for managing tabs and panels within the Autodesk Revit ribbon interface.
/// </summary>
/// <remarks>
///     This class includes methods to create, check, and retrieve tabs and panels in the Revit ribbon.
///     It is designed to streamline the process of customizing the Revit user interface.
/// </remarks>
public static class RevitTabManager
{
    /// <summary>
    ///     Creates a new tab in the Autodesk Revit ribbon interface.
    /// </summary>
    /// <param name="application">
    ///     The <see cref="UIControlledApplication" /> instance used to interact with the Revit application.
    /// </param>
    /// <param name="tabName">
    ///     The name of the tab to be created. If a tab with the specified name already exists, no action is taken.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown when the <paramref name="application" /> parameter is <c>null</c>.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    ///     Thrown when the <paramref name="tabName" /> parameter is <c>null</c>, empty, or consists only of whitespace.
    /// </exception>
    /// <remarks>
    ///     This method checks if a tab with the specified name already exists in the Revit ribbon.
    ///     If the tab does not exist, it creates a new tab with the given name.
    /// </remarks>
    public static void CreateTab(UIControlledApplication application, string tabName)
    {
        if (application == null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        if (string.IsNullOrWhiteSpace(tabName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(tabName));
        }

        if (HasTab(tabName))
        {
            return;
        }

        application.CreateRibbonTab(tabName);
    }

    /// <summary>
    ///     Determines whether a tab with the specified name exists in the Autodesk Revit ribbon interface.
    /// </summary>
    /// <param name="tabName">
    ///     The name of the tab to check for existence. Must not be <c>null</c>, empty, or consist only of whitespace.
    /// </param>
    /// <returns>
    ///     <c>true</c> if a tab with the specified name exists; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="System.ArgumentException">
    ///     Thrown when the <paramref name="tabName" /> parameter is <c>null</c>, empty, or consists only of whitespace.
    /// </exception>
    /// <remarks>
    ///     This method searches through the existing tabs in the Revit ribbon to determine if a tab with the given name is
    ///     present.
    /// </remarks>
    public static bool HasTab(string tabName)
    {
        if (string.IsNullOrWhiteSpace(tabName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(tabName));
        }

        return ComponentManager.Ribbon.Tabs.Any(tab => tab.Name == tabName);
    }

    /// <summary>
    ///     Creates a new panel within a specified tab in the Autodesk Revit ribbon interface.
    /// </summary>
    /// <param name="application">
    ///     The <see cref="UIControlledApplication" /> instance used to interact with the Revit application.
    /// </param>
    /// <param name="panelName">
    ///     The name of the panel to be created.
    /// </param>
    /// <param name="tabName">
    ///     The name of the tab in which the panel will be created. The tab must already exist.
    /// </param>
    /// <returns>
    ///     The newly created <see cref="Autodesk.Revit.UI.RibbonPanel" /> instance.
    /// </returns>
    /// <exception cref="System.ApplicationException">
    ///     Thrown when the specified tab does not exist in the Revit ribbon.
    /// </exception>
    /// <remarks>
    ///     This method ensures that the specified tab exists before creating a new panel within it.
    ///     If the tab is missing, an exception is thrown.
    /// </remarks>
    public static RibbonPanel CreatePanel(UIControlledApplication application, string panelName, string tabName)
    {
        if (!HasTab(tabName))
        {
            throw new Exception($"Missing ribbon tab '{tabName}'.");
        }

        var panel = application.CreateRibbonPanel(tabName, panelName);

        return panel!;
    }

    /// <summary>
    ///     Retrieves a ribbon panel from the specified tab in the Autodesk Revit ribbon interface.
    /// </summary>
    /// <param name="application">The <see cref="UIControlledApplication" /> instance representing the Revit application.</param>
    /// <param name="panelName">The name of the panel to retrieve.</param>
    /// <param name="tabName">The name of the tab containing the panel.</param>
    /// <returns>
    ///     The <see cref="RibbonPanel" /> instance if the panel exists; otherwise, creates and returns a new panel.
    /// </returns>
    /// <remarks>
    ///     If the specified panel does not exist within the given tab, this method creates the panel and returns it.
    /// </remarks>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown if <paramref name="application" />, <paramref name="panelName" />, or <paramref name="tabName" /> is
    ///     <c>null</c>.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    ///     Thrown if <paramref name="panelName" /> or <paramref name="tabName" /> is an empty string or consists only of
    ///     whitespace.
    /// </exception>
    public static RibbonPanel GetPanel(UIControlledApplication application, string panelName, string tabName)
    {
        if (application == null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        if (string.IsNullOrWhiteSpace(panelName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(panelName));
        }

        if (string.IsNullOrWhiteSpace(tabName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(tabName));
        }

        return (HasPanel(application, panelName, tabName)
            ? application.GetRibbonPanels(tabName).FirstOrDefault(item => item.Name == panelName)
            : CreatePanel(application, panelName, tabName))!;
    }

    /// <summary>
    ///     Determines whether a panel with the specified name exists within a given tab in the Autodesk Revit ribbon
    ///     interface.
    /// </summary>
    /// <param name="application">
    ///     The <see cref="UIControlledApplication" /> instance used to interact with the Revit application.
    /// </param>
    /// <param name="panelName">
    ///     The name of the panel to check for existence.
    /// </param>
    /// <param name="tabName">
    ///     The name of the tab in which to search for the panel.
    /// </param>
    /// <returns>
    ///     <c>true</c> if a panel with the specified name exists within the given tab; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown when the <paramref name="application" /> parameter is <c>null</c>.
    /// </exception>
    /// <exception cref="System.ArgumentException">
    ///     Thrown when the <paramref name="panelName" /> or <paramref name="tabName" /> parameter is <c>null</c>,
    ///     empty, or consists only of white-space characters.
    /// </exception>
    /// <remarks>
    ///     This method searches through the panels of the specified tab in the Revit ribbon to determine if a panel with the
    ///     given name is present.
    /// </remarks>
    public static bool HasPanel(UIControlledApplication application, string panelName, string tabName)
    {
        if (application == null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        if (string.IsNullOrWhiteSpace(panelName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(panelName));
        }

        if (string.IsNullOrWhiteSpace(tabName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(tabName));
        }

        return application.GetRibbonPanels(tabName).Any(item => item.Name == panelName);
    }
}
