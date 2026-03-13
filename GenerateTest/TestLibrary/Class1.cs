
using RevitAssemblyResolver;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.Loader;

namespace Test;

public class Test
{
    public static bool IsDotNetAssembly(string file)
    {
        var path = Path.GetDirectoryName(file);
        var name = Path.GetFileNameWithoutExtension(file);
        using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite);

        using var peReader = new PEReader(stream);

        // Native DLL or other PE without CLI metadata
        if (!peReader.HasMetadata)
            return false;

        var mdReader = peReader.GetMetadataReader();

        // True only for real managed assemblies with an assembly manifest
        return mdReader.IsAssembly;
    }
}
