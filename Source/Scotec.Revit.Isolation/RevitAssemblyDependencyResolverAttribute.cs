using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scotec.Revit.Isolation
{
    /// <summary>
    ///     Marks a class as a custom assembly dependency resolver for Revit add-ins.
    /// </summary>
    /// <remarks>
    ///     Classes decorated with this attribute are recognized by the source generator as candidates
    ///     for resolving managed and unmanaged assembly dependencies within a Revit add-in context.
    ///     <b>Note:</b> Classes marked with this attribute must implement the <see cref="Scotec.Revit.Loader.IRevitAssemblyDependencyResolver"/> interface.
    ///     Only one class per assembly should be marked with this attribute.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class RevitAssemblyDependencyResolverAttribute : Attribute
    {
    }
}
