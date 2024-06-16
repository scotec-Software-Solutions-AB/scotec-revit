using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.UI;
using Scotec.Revit.LoadContext;

namespace {@namespace}
{
    public class {{className}}Factory : IExternalApplication
    {
        public static AddinLoadContext Context { get; }
        private IExternalApplication _instance;
        private static Assembly s_assembly;
                                    
        static {className}Factory()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            var path = Path.GetDirectoryName(location)!;
                                    
            Context = new AddinLoadContext(path);
            s_assembly = Context.LoadFromAssemblyPath(location);
        }
                                        
        public {{className}}Factory()
        {
            var types = s_assembly.GetTypes();
            var t = types.First(type => type.Name == "{className}");
            _instance = (IExternalApplication)Activator.CreateInstance(t);
        }
                                    
        public Result OnStartup(UIControlledApplication application)
        {
            return _instance.OnStartup(application);
        }
                                    
        public Result OnShutdown(UIControlledApplication application)
        {
            return _instance.OnShutdown(application);
        }
    }
}
