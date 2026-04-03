// Copyright © 2023 - 2026 Olaf Meyer
// Copyright © 2023 - 2026 scotec Software Solutions AB, www.scotec.com
// This file is licensed to you under the MIT license.

using System;
using System.Runtime.Loader;
using System.Windows;
using System.Windows.Controls;

namespace Scotec.Revit.Wpf;

/// <summary>
/// Represents a WPF <see cref="UserControl"/> specifically designed for use within the Revit environment.
/// </summary>
/// <remarks>
/// This class provides functionality to manage contextual reflection scopes during the initialization phase,
/// ensuring proper assembly loading and unloading in the Revit environment.
/// </remarks>
public class RevitUserControl : UserControl
{
    private IDisposable? _contextualReflectionScope;

    /// <summary>
    /// Begins the initialization process for the <see cref="RevitUserControl"/>.
    /// </summary>
    /// <remarks>
    /// This method overrides the base <see cref="FrameworkElement.BeginInit"/> method to enter a contextual reflection scope
    /// specific to the assembly of the current <see cref="RevitUserControl"/>. This ensures proper handling of assembly
    /// loading in the Revit environment.
    /// </remarks>
    /// <seealso cref="System.Windows.FrameworkElement.BeginInit"/>
    /// <seealso cref="EndInit"/>
    public override void BeginInit()
    {
        _contextualReflectionScope = AssemblyLoadContext.EnterContextualReflection(GetType().Assembly);
        base.BeginInit();
    }

    /// <summary>
    /// Completes the initialization process for the <see cref="RevitUserControl"/>.
    /// </summary>
    /// <remarks>
    /// This method overrides the base <see cref="FrameworkElement.EndInit"/> method to exit the contextual reflection scope
    /// that was entered during the <see cref="BeginInit"/> method. It ensures proper cleanup of resources
    /// and maintains the integrity of assembly loading in the Revit environment.
    /// </remarks>
    /// <seealso cref="System.Windows.FrameworkElement.EndInit"/>
    /// <seealso cref="BeginInit"/>
    public override void EndInit()
    {
        base.EndInit();
        _contextualReflectionScope?.Dispose();
        _contextualReflectionScope = null;
    }
}
