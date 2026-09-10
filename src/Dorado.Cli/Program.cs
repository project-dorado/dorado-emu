using System.Globalization;
using System.Text;
using Dorado.Containers;

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
                "run" => RunStub(args),
                "-h" or "--help" or "help" => PrintUsage(),
                _ => Unknown(args[0]),
            };
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or NotSupportedException or UnauthorizedAccessException)
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

    private static int RunStub(string[] args)
    {
        Console.Error.WriteLine("run: the managed XNA runtime lands in M1; container parsing is available via 'inspect'/'unpack'.");
        return 4;
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
