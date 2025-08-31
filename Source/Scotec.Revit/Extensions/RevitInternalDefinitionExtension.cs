using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scotec.Revit.Extensions
{
    /// <summary>
    /// Provides extension methods for the <see cref="Autodesk.Revit.DB.InternalDefinition"/> class.
    /// </summary>
    public static class RevitInternalDefinitionExtension
    {
        /// <summary>
        /// Extracts a <see cref="Guid"/> from the specified <see cref="InternalDefinition"/> instance.
        /// </summary>
        /// <param name="definition">
        /// The <see cref="InternalDefinition"/> instance from which to extract the GUID.
        /// </param>
        /// <returns>
        /// A <see cref="Guid"/> if the <see cref="InternalDefinition"/> contains a valid GUID; otherwise, <c>null</c>.
        /// </returns>
        /// <remarks>
        /// This method attempts to parse the GUID from the <see cref="InternalDefinition"/>'s type identifier.
        /// If the type identifier is empty or does not contain a valid GUID, the method returns <c>null</c>.
        /// </remarks>
        public static Guid? ExtractGuid(this InternalDefinition definition)
        {
            const int guidLength = 32;
            
            var forgeTypeId = definition.GetTypeId();
            if (forgeTypeId.Empty())
            {
                return null;
            }

            var typeId = forgeTypeId.TypeId;
            var position = typeId.LastIndexOf(':');
            if (position < 0)
            {
                return null;
            }

            return Guid.TryParseExact(typeId.Substring(position + 1, guidLength), "N", out var guid)
                ? guid
                : null;
        }

        public static bool IsShared(this InternalDefinition definition)
        {
            var ftid = definition.GetTypeId();
            return !ftid.Empty()
                   && ftid.TypeId.StartsWith("revit.local.shared", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsProject(this InternalDefinition definition)
        {
            var ftid = definition.GetTypeId();
            return !ftid.Empty()
                   && ftid.TypeId.StartsWith("revit.local.project", StringComparison.OrdinalIgnoreCase);
        }
    }
}
