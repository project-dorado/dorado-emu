using System.Reflection;
using Dorado.Containers;
using Dorado.Platform;
using Microsoft.Xna.Framework;

namespace Dorado.Runtime;

public sealed class ZuneRunOptions
{
    public required IGraphicsBackend Graphics { get; init; }

    public IInputSource Input { get; init; } = EmptyInputSource.Instance;

    public int FrameLimit { get; init; } = 60;

    public bool RunForever { get; init; }

    /// <summary>Invoked after each presented frame.</summary>
    public Action<int>? OnFrameRendered { get; init; }

    /// <summary>Keep the extracted working directory instead of deleting it.</summary>
    public bool KeepWorkingDirectory { get; init; }
}

public sealed class ZuneRunResult
{
    public required string WorkingDirectory { get; init; }

    public required int FramesRendered { get; init; }

    public required string EntryPoint { get; init; }
}

/// <summary>Extracts and runs a Zune package's managed XNA application.</summary>
public static class ZuneAppRunner
{
    public static ZuneRunResult Run(ZunePackage package, ZuneRunOptions options)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(options);

        if (package.IsEncrypted || package.EntryCount == 0)
        {
            throw new InvalidOperationException("Cannot run an encrypted or empty package without content keys.");
        }

        string directory = ExtractToTempDirectory(package);
        string? originalCwd = Directory.GetCurrentDirectory();

        try
        {
            Directory.SetCurrentDirectory(directory);
            ConfigureHost(options, directory);

            var overrides = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase)
            {
                ["Microsoft.Xna.Framework"] = typeof(Vector2).Assembly,
                ["Microsoft.Xna.Framework.Game"] = typeof(Game).Assembly,
            };

            var context = new ZuneLoadContext(directory, overrides);
            Assembly assembly = context.LoadAppAssembly(Path.Combine(directory, ResolveStartupAssembly(package)));

            string entryPoint = InvokeEntryPoint(assembly, package);

            return new ZuneRunResult
            {
                WorkingDirectory = directory,
                FramesRendered = (int)PlatformHost.FrameCount,
                EntryPoint = entryPoint,
            };
        }
        finally
        {
            Directory.SetCurrentDirectory(originalCwd);
            if (!options.KeepWorkingDirectory)
            {
                TryDelete(directory);
            }
        }
    }

    private static void ConfigureHost(ZuneRunOptions options, string directory)
    {
        PlatformHost.Graphics = options.Graphics;
        PlatformHost.Input = options.Input;
        PlatformHost.FrameLimit = options.FrameLimit;
        PlatformHost.RunForever = options.RunForever;
        PlatformHost.FixedTimeStep = true;
        PlatformHost.FrameTime = TimeSpan.FromSeconds(1.0 / 60.0);
        PlatformHost.OnFrameRendered = options.OnFrameRendered;
        PlatformHost.ResetExit();
    }

    private static string ResolveStartupAssembly(ZunePackage package)
    {
        string? startup = package.Metadata.StartupAssembly;
        if (!string.IsNullOrEmpty(startup))
        {
            string name = Path.GetFileName(startup);
            if (package.Files.Any(f => string.Equals(Path.GetFileName(f.Path), name, StringComparison.OrdinalIgnoreCase)))
            {
                return name;
            }
        }

        PackageFile? executable = package.Files.FirstOrDefault(f =>
            f.Path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
        return executable is null
            ? throw new InvalidOperationException("Package has no startup executable.")
            : Path.GetFileName(executable.Path);
    }

    private static string InvokeEntryPoint(Assembly assembly, ZunePackage package)
    {
        MethodInfo? entry = assembly.EntryPoint;
        if (entry is not null && entry.ReturnType == typeof(void))
        {
            object?[]? args = entry.GetParameters().Length == 0
                ? null
                : new object?[] { Array.Empty<string>() };
            try
            {
                entry.Invoke(null, args);
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                throw new ZuneAppException($"Application entry point threw: {ex.InnerException.Message}", ex.InnerException);
            }

            return $"{assembly.GetName().Name}::{entry.Name}";
        }

        // Fall back to instantiating a Game-derived type directly.
        Type? gameType = assembly.GetTypes().FirstOrDefault(t =>
            !t.IsAbstract && typeof(Game).IsAssignableFrom(t));
        if (gameType is null)
        {
            throw new ZuneAppException(
                $"No entry point or Game-derived type found in '{assembly.GetName().Name}'.");
        }

        if (Activator.CreateInstance(gameType) is not Game game)
        {
            throw new ZuneAppException($"Could not instantiate '{gameType.FullName}'.");
        }

        game.Run();
        return gameType.FullName!;
    }

    private static string ExtractToTempDirectory(ZunePackage package)
    {
        string directory = Path.Combine(Path.GetTempPath(), "dorado", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        foreach (PackageFile file in package.Files)
        {
            if (!package.TryGetEntry(file.Path, out byte[]? payload))
            {
                continue;
            }

            string relative = file.Path.Replace('\\', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
            string destination = Path.GetFullPath(Path.Combine(directory, relative));
            if (!destination.StartsWith(directory, StringComparison.Ordinal))
            {
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.WriteAllBytes(destination, payload);
        }

        return directory;
    }

    private static void TryDelete(string directory)
    {
        try
        {
            Directory.Delete(directory, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}

/// <summary>Raised when a Zune application fails to start.</summary>
public sealed class ZuneAppException : Exception
{
    public ZuneAppException(string message)
        : base(message)
    {
    }

    public ZuneAppException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
