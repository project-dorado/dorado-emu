using System.Reflection;
using System.Runtime.Loader;

namespace Dorado.Runtime;

/// <summary>
/// Loads Zune app assemblies and maps their Compact Framework / XNA assembly
/// references onto the running framework and the Dorado shims.
/// </summary>
internal sealed class ZuneLoadContext : AssemblyLoadContext
{
    private static readonly HashSet<string> FrameworkFacades = new(StringComparer.OrdinalIgnoreCase)
    {
        "mscorlib",
        "System",
        "System.Core",
        "System.Runtime",
        "System.Runtime.Extensions",
        "System.Collections",
        "System.Linq",
        "System.Xml",
        "System.Xml.Linq",
        "netstandard",
    };

    private readonly string _directory;
    private readonly IReadOnlyDictionary<string, Assembly> _overrides;

    public ZuneLoadContext(string directory, IReadOnlyDictionary<string, Assembly> overrides)
        : base("Dorado.Zune")
    {
        _directory = directory;
        _overrides = overrides;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string? name = assemblyName.Name;
        if (name is null)
        {
            return null;
        }

        if (_overrides.TryGetValue(name, out Assembly? overrideAssembly))
        {
            return overrideAssembly;
        }

        if (FrameworkFacades.Contains(name))
        {
            try
            {
                return Assembly.Load(name);
            }
            catch (FileNotFoundException)
            {
                // Fall through to the app-local lookup.
            }
        }

        string candidate = Path.Combine(_directory, name + ".dll");
        return File.Exists(candidate) ? LoadFromAssemblyPath(candidate) : null;
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName) => IntPtr.Zero;
}
