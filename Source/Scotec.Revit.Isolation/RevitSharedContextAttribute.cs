using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scotec.Revit.Isolation
{
    public class RevitSharedContextAttribute : Attribute
    {
        public RevitSharedContextAttribute(string contextName)
        {
            ContextName = contextName;
        }
        
        private string ContextName { get; }
    }
}
