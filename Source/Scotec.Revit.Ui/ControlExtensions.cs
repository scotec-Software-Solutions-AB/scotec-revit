// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.UI;

namespace Scotec.Revit.Ui;

public static class ControlExtensions
{
    public static PushButton AddPushButton(this RibbonPanel panel, PushButtonData data)
    {
        if (panel == null)
        {
            throw new ArgumentNullException(nameof(panel));
        }

        if (data == null)
        {
            throw new ArgumentNullException(nameof(data));
        }

        var pushButton = panel.AddItem(data) as PushButton;
        if (pushButton == null)
        {
            throw new InvalidOperationException("Failed to add PushButton to the RibbonPanel.");
        }

        return pushButton;
    }
}
