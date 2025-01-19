// Copyright © 2023 - 2024 Olaf Meyer
// Copyright © 2023 - 2024 scotec Software Solutions AB, www.scotec-software.com
// This file is licensed to you under the MIT license.

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Autodesk.Revit.UI;

namespace Scotec.Revit.Wpf;

/// <summary>
/// Represents a WPF window specifically designed for use within Autodesk Revit add-ins.
/// </summary>
/// <remarks>
/// This class extends the <see cref="Window"/> class and integrates with the Autodesk Revit API.
/// It sets the main Revit window as the owner of this window and provides functionality to 
/// customize the window's behavior, such as hiding minimize and maximize buttons.
/// </remarks>
public class RevitWindow : Window
{
    /// <summary>
    ///     The constructor. Sets the main window as the owner of this window.
    /// </summary>
    public RevitWindow(UIApplication revitApplication)
    {
        var hwndSource = HwndSource.FromHwnd(revitApplication.MainWindowHandle);
        var mainWindow = hwndSource!.RootVisual as Window;
        Owner = mainWindow;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RevitWindow"/> class and sets the main Revit window as its owner.
    /// </summary>
    /// <param name="application">
    /// An instance of <see cref="UIControlledApplication"/> representing the controlled application in Autodesk Revit.
    /// </param>
    /// <remarks>
    /// This constructor retrieves the main Revit window handle from the provided <see cref="UIControlledApplication"/> 
    /// and sets it as the owner of this WPF window. This ensures proper integration and behavior within the Revit environment.
    /// </remarks>
    public RevitWindow(UIControlledApplication application)
    {
        var hwndSource = HwndSource.FromHwnd(application.MainWindowHandle);
        var mainWindow = hwndSource!.RootVisual as Window;
        Owner = mainWindow;

    }

    /// <summary>
    /// Hides the minimize and maximize buttons of the current window.
    /// </summary>
    /// <remarks>
    /// This method modifies the window style to remove the minimize and maximize buttons
    /// by interacting with the Windows API. It is executed when the window's source is initialized.
    /// </remarks>
    public void HideMinimizeAndMaximizeButtons()
    {
        SourceInitialized += (s, e) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var currentStyle = GetWindowLong(hwnd, GWL_STYLE);

            SetWindowLong(hwnd, GWL_STYLE, currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
        };
    }

    /// <summary>
    /// Retrieves information about the specified window.
    /// </summary>
    /// <param name="hwnd">A handle to the window whose information is to be retrieved.</param>
    /// <param name="index">
    /// The zero-based offset to the value to be retrieved. This parameter specifies the 
    /// type of information to retrieve. For example, use <c>GWL_STYLE</c> to retrieve the window's style.
    /// </param>
    /// <returns>
    /// The requested value, which varies depending on the <paramref name="index"/> parameter.
    /// </returns>
    /// <remarks>
    /// This method is a wrapper for the Windows API function <c>GetWindowLong</c>. It is used to retrieve
    /// information about a window, such as its style or extended style.
    /// </remarks>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowlongw">
    /// Windows API documentation for GetWindowLong
    /// </seealso>
    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    /// <summary>
    /// Sets a new value for the specified attribute of a window.
    /// </summary>
    /// <param name="hwnd">A handle to the window whose attribute is to be modified.</param>
    /// <param name="index">
    /// The zero-based offset to the value to be set. This parameter specifies the 
    /// type of attribute to modify. For example, use <c>GWL_STYLE</c> to modify the window's style.
    /// </param>
    /// <param name="value">The new value to be set for the specified attribute.</param>
    /// <returns>
    /// The previous value of the specified attribute, which varies depending on the <paramref name="index"/> parameter.
    /// </returns>
    /// <remarks>
    /// This method is a wrapper for the Windows API function <c>SetWindowLong</c>. It is used to modify
    /// attributes of a window, such as its style or extended style.
    /// </remarks>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowlongw">
    /// Windows API documentation for SetWindowLong
    /// </seealso>
    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int value);

    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Represents the index used to retrieve or modify the window's style information.
    /// </summary>
    /// <remarks>
    /// This constant is used as a parameter in Windows API functions, such as <c>GetWindowLong</c>
    /// and <c>SetWindowLong</c>, to specify that the window's style should be accessed or modified.
    /// </remarks>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getwindowlongw">
    /// Windows API documentation for GetWindowLong
    /// </seealso>
    /// <seealso href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowlongw">
    /// Windows API documentation for SetWindowLong
    /// </seealso>
    private const int GWL_STYLE = -16;
    
    /// <summary>
    /// Represents the window style constant used to enable or disable the maximize button in a window.
    /// </summary>
    /// <remarks>
    /// This constant is used in conjunction with Windows API functions, such as <c>GetWindowLong</c> 
    /// and <c>SetWindowLong</c>, to modify the style of a window and control the visibility of the maximize button.
    /// </remarks>
    private const int WS_MAXIMIZEBOX = 0x10000;

    /// <summary>
    /// Represents the window style constant used to enable or disable the minimize button on a window.
    /// </summary>
    /// <remarks>
    /// This constant is used in conjunction with Windows API functions, such as <c>GetWindowLong</c> 
    /// and <c>SetWindowLong</c>, to modify the style of a window. Specifically, it controls the 
    /// visibility and functionality of the minimize button.
    /// </remarks>
    private const int WS_MINIMIZEBOX = 0x20000;
    // ReSharper restore InconsistentNaming
}
