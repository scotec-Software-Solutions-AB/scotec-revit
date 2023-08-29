using Microsoft.Extensions.Hosting;

namespace Scotec.Revit;

public static class RevitAppExtensions
{
    public static IHostBuilder CreateRevitHostBuilder(this RevitApp app)
    {
        return new RevitHostBuilder(app);
    }
}
