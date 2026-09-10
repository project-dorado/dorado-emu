using System.Globalization;
using System.Text;
using Dorado.Containers;
using Dorado.Platform.Desktop.Software;
using Dorado.Runtime;

namespace Dorado.Cli;

internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        try
        {
            return args[0].ToLowerInvariant() switch
            {
                "inspect" => Inspect(args),
                "unpack" => Unpack(args),
                "refs" => Refs(args),
                "run" => Run(args),
                "-h" or "--help" or "help" => PrintUsage(),
                _ => Unknown(args[0]),
            };
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or NotSupportedException or UnauthorizedAccessException or ZuneAppException)
        {
            Console.Error.WriteLine($"error: {ex.Message}");
            return 2;
        }
    }

    private static int Inspect(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("usage: dorado inspect <package>");
            return 1;
        }

        ZunePackage package = ZunePackageReader.Read(args[1]);

        Console.WriteLine($"kind           {package.Kind}");
        Console.WriteLine($"title          {Value(package.Metadata.Title)}");
        Console.WriteLine($"executable     {Value(package.Metadata.Executable)}");
        Console.WriteLine($"startup        {Value(package.Metadata.StartupAssembly)}");
        Console.WriteLine($"platform       {Value(package.Metadata.Platform)}");
        Console.WriteLine($"guid           {Value(package.Metadata.GameGuid)}");
        Console.WriteLine($"runtimeProfile {Value(package.Metadata.RuntimeProfile)}");
        Console.WriteLine($"ccgameVersion  {Value(package.Metadata.CcgameVersion)}");
        Console.WriteLine($"encrypted      {package.IsEncrypted}");
        Console.WriteLine($"entries        {package.EntryCount} extracted / {package.Files.Count} declared");

        if (!string.IsNullOrWhiteSpace(package.Metadata.Description))
        {
            Console.WriteLine($"description    {Trim(package.Metadata.Description)}");
        }

        if (package.Files.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("files:");
            foreach (PackageFile file in package.Files)
            {
                string size = file.Size >= 0 ? file.Size.ToString(CultureInfo.InvariantCulture) : "?";
                Console.WriteLine($"  {file.Path,-48} {size,10}  ({file.ContainerName})");
            }
        }

        return 0;
    }

    private static int Unpack(string[] args)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("usage: dorado unpack <package> <outputDir>");
            return 1;
        }

        string outputDir = args[2];
        ZunePackage package = ZunePackageReader.Read(args[1]);

        if (package.IsEncrypted || package.EntryCount == 0)
        {
            Console.Error.WriteLine(
                package.IsEncrypted
                    ? "package payload is DRM-encrypted; no key provider supplied (see docs/drm-key-import.md)."
                    : "package contains no extractable entries.");
            return 3;
        }

        Directory.CreateDirectory(outputDir);
        int written = 0;
        foreach (PackageFile file in package.Files)
        {
            if (!package.TryGetEntry(file.Path, out byte[]? payload))
            {
                continue;
            }

            string relative = Sanitize(file.Path);
            string destination = Path.Combine(outputDir, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination) ?? outputDir);
            File.WriteAllBytes(destination, payload);
            written++;
        }

        Console.WriteLine($"wrote {written} file(s) to {outputDir}");
        return 0;
    }

    private static int Refs(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("usage: dorado refs <assembly>");
            return 1;
        }

        AssemblyReferenceReport report = AssemblyInspector.Inspect(args[1]);

        Console.WriteLine("assembly references:");
        foreach (string reference in report.AssemblyReferences)
        {
            Console.WriteLine($"  {reference}");
        }

        if (report.TypeReferences.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("type references:");
            foreach (string type in report.TypeReferences.Where(t => t.StartsWith("Microsoft.Xna", StringComparison.Ordinal)))
            {
                Console.WriteLine($"  {type}");
            }
        }

        if (report.XnaDerivedTypes.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("types deriving from XNA:");
            foreach (string type in report.XnaDerivedTypes)
            {
                Console.WriteLine($"  {type}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("member references:");
        foreach (var (type, members) in report.MemberReferences)
        {
            Console.WriteLine($"  {type}");
            foreach (string member in members)
            {
                Console.WriteLine($"    {member}");
            }
        }

        return 0;
    }

    private static int Run(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("usage: dorado run <package> [--frames N] [--out frame.png]");
            return 1;
        }

        int frames = 60;
        string? outPath = null;
        bool hash = false;
        for (int i = 2; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--frames" when i + 1 < args.Length:
                    frames = int.Parse(args[++i], CultureInfo.InvariantCulture);
                    break;
                case "--out" when i + 1 < args.Length:
                    outPath = args[++i];
                    break;
                case "--hash":
                    hash = true;
                    break;
            }
        }

        ZunePackage package = ZunePackageReader.Read(args[1]);
        var backend = new SoftwareGraphicsBackend();

        ZuneRunResult result = ZuneAppRunner.Run(package, new ZuneRunOptions
        {
            Graphics = backend,
            Input = ScriptedInputSource.Empty,
            FrameLimit = frames,
            OnFrameRendered = frame =>
            {
                if (outPath is not null && frame == frames - 1)
                {
                    backend.SavePng(outPath);
                }
            },
        });

        Console.WriteLine($"ran {result.FramesRendered} frame(s) of {result.EntryPoint}");
        if (outPath is not null)
        {
            Console.WriteLine($"frame -> {outPath}");
        }

        if (hash)
        {
            Console.WriteLine($"frame-sha256 {FrameHash(backend.SnapshotBackbuffer())}");
        }

        return 0;
    }

    private static string FrameHash(uint[] pixels)
    {
        var bytes = new byte[pixels.Length * 4];
        for (int i = 0; i < pixels.Length; i++)
        {
            System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(i * 4), pixels[i]);
        }

        return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static int Unknown(string command)
    {
        Console.Error.WriteLine($"unknown command '{command}'.");
        PrintUsage();
        return 1;
    }

    private static int PrintUsage()
    {
        Console.WriteLine(
            """
            dorado — Zune HD package tool

            usage:
              dorado inspect <package>            show container metadata and files
              dorado unpack  <package> <outDir>   extract an unencrypted package
              dorado refs    <assembly>           list assembly/member references
              dorado run     <package>            run a package (M1)

            packages: .ccgame (XNA cabinet) and .zcp (NX container)
            """);
        return 0;
    }

    private static string Value(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "-" : value;

    private static string Trim(string value) =>
        value.Length <= 72 ? value : value[..69] + "...";

    /// <summary>Prevents path traversal when extracting package-controlled paths.</summary>
    private static string Sanitize(string path)
    {
        var builder = new StringBuilder(path.Length);
        foreach (char c in path.Replace('\\', '/'))
        {
            if (c is '/' or ':' or '*' or '?' or '"' or '<' or '>' or '|')
            {
                builder.Append('_');
            }
            else
            {
                builder.Append(c);
            }
        }

        string cleaned = builder.ToString().TrimStart('.', '_', '/');
        return cleaned.Length == 0 ? "entry" : cleaned;
    }
}
