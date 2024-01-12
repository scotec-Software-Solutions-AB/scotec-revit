using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows;
using Autodesk.Revit.UI;
using System.Runtime.InteropServices;

namespace Scotec.Revit
{
    /// <summary>
    /// 
    /// </summary>
    public class RevitWindow : Window
    {
        /// <summary>
        /// The constructor. Sets the main window as the oxner of this window.
        /// </summary>
        /// 
        public RevitWindow(UIApplication revitApplication)
        {
            var hwndSource = HwndSource.FromHwnd(revitApplication.MainWindowHandle);
            var mainWindow = hwndSource!.RootVisual as Window;
            Owner = mainWindow;
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int value);

        /// <summary>
        /// Hides the minimize and maximize button of the add-in window.
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

        // ReSharper disable InconsistentNaming
        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;

        private const int WS_MINIMIZEBOX = 0x20000;
        // ReSharper restore InconsistentNaming
    }
}
