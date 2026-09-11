using System.Reflection;
using System.Runtime.InteropServices;
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
            if (Environment.GetEnvironmentVariable("DORADO_ZDK_DEBUG") == "1")
            {
                Console.Error.WriteLine($"[alc] {name} -> {overrideAssembly.Location}");
            }

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
        if (File.Exists(candidate))
        {
            return LoadAppAssembly(candidate);
        }

        string executable = Path.Combine(_directory, name + ".exe");
        return File.Exists(executable) ? LoadAppAssembly(executable) : null;
    }

    /// <summary>
    /// Loads an application-local assembly, clearing the PE <c>32BITREQ</c> flag
    /// first. Zune Compact Framework assemblies are IL-only but carry the flag;
    /// modern .NET refuses to load them unchanged even though the code is
    /// architecture-neutral.
    /// </summary>
    public Assembly LoadAppAssembly(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        CorFlags.TryClear32BitRequired(bytes);
        using var stream = new MemoryStream(bytes, writable: false);
        return LoadFromStream(stream);
    }

    /// <summary>
    /// Resolves the native <c>ZDK</c>/<c>MEDIA</c> libraries the Zune XNA
    /// extension P/Invokes. Dorado ships a compatibility library built from
    /// <c>native/zdk-bridge/</c>; when it is absent the P/Invoke fails normally.
    /// </summary>
    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        if (!unmanagedDllName.Equals("ZDK", StringComparison.OrdinalIgnoreCase) &&
            !unmanagedDllName.Equals("MEDIA", StringComparison.OrdinalIgnoreCase))
        {
            return IntPtr.Zero;
        }

        bool debug = Environment.GetEnvironmentVariable("DORADO_ZDK_DEBUG") == "1";
        foreach (string candidate in ZdkLibraryCandidates())
        {
            if (File.Exists(candidate) && NativeLibrary.TryLoad(candidate, out IntPtr handle))
            {
                if (debug)
                {
                    Console.Error.WriteLine($"[zdk] {unmanagedDllName} -> {candidate}");
                }

                return handle;
            }
        }

        if (debug)
        {
            Console.Error.WriteLine($"[zdk] {unmanagedDllName} unresolved");
        }

        return IntPtr.Zero;
    }

    private IEnumerable<string> ZdkLibraryCandidates()
    {
        string? configured = Environment.GetEnvironmentVariable("DORADO_ZDK_LIB");
        if (!string.IsNullOrWhiteSpace(configured))
        {
            yield return configured;
        }

        foreach (string directory in new[] { AppContext.BaseDirectory, _directory })
        {
            yield return Path.Combine(directory, "libZDK.so");
            yield return Path.Combine(directory, "libZDK.dylib");
            yield return Path.Combine(directory, "ZDK.dll");
        }
    }

    /// <summary>Minimal PE COR20 header writer used to relax the 32BITREQ flag.</summary>
    private static class CorFlags
    {
        public static void TryClear32BitRequired(byte[] image)
        {
            try
            {
                if (image.Length < 0x40)
                {
                    return;
                }

                int peOffset = BitConverter.ToInt32(image, 0x3C);
                if (peOffset <= 0 || peOffset + 24 > image.Length ||
                    image[peOffset] != (byte)'P' || image[peOffset + 1] != (byte)'E' ||
                    image[peOffset + 2] != 0 || image[peOffset + 3] != 0)
                {
                    return;
                }

                int sectionCount = BitConverter.ToUInt16(image, peOffset + 6);
                int optionalSize = BitConverter.ToUInt16(image, peOffset + 20);
                int optional = peOffset + 24;
                int sectionTable = optional + optionalSize;
                if (sectionTable + (sectionCount * 40) > image.Length)
                {
                    return;
                }

                int corDirectory = optional + 96 + (14 * 8);
                if (corDirectory + 8 > image.Length)
                {
                    return;
                }

                uint corRva = BitConverter.ToUInt32(image, corDirectory);
                if (corRva == 0)
                {
                    return;
                }

                for (int i = 0; i < sectionCount; i++)
                {
                    int section = sectionTable + (i * 40);
                    uint virtualSize = BitConverter.ToUInt32(image, section + 8);
                    uint virtualAddress = BitConverter.ToUInt32(image, section + 12);
                    uint rawSize = BitConverter.ToUInt32(image, section + 16);
                    uint rawPointer = BitConverter.ToUInt32(image, section + 20);
                    if (virtualAddress > corRva || corRva >= virtualAddress + Math.Max(virtualSize, rawSize))
                    {
                        continue;
                    }

                    long flagsOffset = (long)rawPointer + (corRva - virtualAddress) + 16;
                    if (flagsOffset + 4 > image.Length)
                    {
                        return;
                    }

                    uint flags = BitConverter.ToUInt32(image, (int)flagsOffset);
                    BitConverter.TryWriteBytes(image.AsSpan((int)flagsOffset, 4), flags & ~0x2u);
                    return;
                }
            }
            catch (Exception ex) when (ex is ArgumentOutOfRangeException or IndexOutOfRangeException)
            {
                // Leave the image untouched; loading will surface the real error.
            }
        }
    }
}
