// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using System.Linq;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using RibbonPanel = Autodesk.Revit.UI.RibbonPanel;

namespace Scotec.Revit.Test;

public class TabManager
{
    public static void CreateTab(UIControlledApplication application, string tabName)
    {
        if (HasTab(tabName))
        {
            return;
        }

        application.CreateRibbonTab(tabName);
    }

    public static bool HasTab(string tabName)
    {
        return ComponentManager.Ribbon.Tabs.Any(tab => tab.Name == tabName);
    }

    public static RibbonPanel CreatePanel(UIControlledApplication application, string panelName, string tabName)
    {
        if (!HasTab(tabName))
        {
            throw new ApplicationException($"Missing ribbon tab '{tabName}'.");
        }

        var panel = application.CreateRibbonPanel(tabName, panelName);

        return panel;
    }

    public static RibbonPanel GetPanel(UIControlledApplication application, string panelName, string tabName)
    {
        return HasPanel(application, panelName, tabName)
            ? application.GetRibbonPanels(tabName).FirstOrDefault(item => item.Name == panelName)
            : CreatePanel(application, panelName, tabName);
    }

    public static bool HasPanel(UIControlledApplication application, string panelName, string tabName)
    {
        return application.GetRibbonPanels(tabName).Any(item => item.Name == panelName);
    }
}
