using System;
using Autodesk.Revit.UI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Scotec.Revit.Test
{
    public class RevitTestApp : RevitApp
    {
        protected override Result OnShutdown()
        {
            return Result.Succeeded;
        }

        protected override Result OnStartup()
        {
            try
            {
                var config = Services.GetService<IConfiguration>();
            }
            catch (Exception)
            {
                return Result.Failed;
            }

            return Result.Succeeded;
        }

    }
}
