// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Autodesk.Revit.UI;

namespace Scotec.Revit;

/// <summary>
/// </summary>
public class RevitWindow : Window
{
    /// <summary>
    ///     The constructor. Sets the main window as the oxner of this window.
    /// </summary>
    public RevitWindow(UIApplication revitApplication)
    {
        var hwndSource = HwndSource.FromHwnd(revitApplication.MainWindowHandle);
        var mainWindow = hwndSource!.RootVisual as Window;
        Owner = mainWindow;
    }

    /// <summary>
    ///     Hides the minimize and maximize button of the add-in window.
    /// </summary>
    public void HideMinimizeAndMaximizeButtons()
    {
        SourceInitialized += (s, e) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var currentStyle = GetWindowLong(hwnd, GWL_STYLE);

            SetWindowLong(hwnd, GWL_STYLE, currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
        };
    }

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int value);

    // ReSharper disable InconsistentNaming
    private const int GWL_STYLE = -16;
    private const int WS_MAXIMIZEBOX = 0x10000;

    private const int WS_MINIMIZEBOX = 0x20000;
    // ReSharper restore InconsistentNaming
}
