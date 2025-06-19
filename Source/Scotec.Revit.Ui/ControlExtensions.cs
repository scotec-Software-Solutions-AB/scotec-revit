// // Copyright © 2023 - 2025 Olaf Meyer
// // Copyright © 2023 - 2025 scotec Software Solutions AB, www.scotec-software.com
// // This file is licensed to you under the MIT license.

using System;
using Autodesk.Revit.UI;

namespace Scotec.Revit.Ui;

/// <summary>
///     Provides extension methods for working with Revit UI controls, such as adding buttons to ribbon panels.
/// </summary>
public static class ControlExtensions
{
    /// <summary>
    ///     Adds a <see cref="PushButton" /> to the specified <see cref="RibbonPanel" /> using the provided
    ///     <see cref="PushButtonData" />.
    /// </summary>
    /// <param name="panel">The <see cref="RibbonPanel" /> to which the push button will be added.</param>
    /// <param name="data">The <see cref="PushButtonData" /> containing the configuration for the push button.</param>
    /// <returns>The created <see cref="PushButton" /> instance.</returns>
    /// <exception cref="System.ArgumentNullException">
    ///     Thrown when <paramref name="panel" /> or <paramref name="data" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    ///     Thrown when the push button could not be added to the <see cref="RibbonPanel" />.
    /// </exception>
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
