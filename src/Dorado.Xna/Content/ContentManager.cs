using Dorado.Platform;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content;

/// <summary>Loads XNB content relative to a root directory.</summary>
public class ContentManager : IDisposable
{
    private string _rootDirectory = string.Empty;

    public ContentManager()
        : this(null!, string.Empty)
    {
    }

    public ContentManager(IServiceProvider serviceProvider)
        : this(serviceProvider, string.Empty)
    {
    }

    public ContentManager(IServiceProvider serviceProvider, string rootDirectory)
    {
        ServiceProvider = serviceProvider;
        RootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
    }

    public IServiceProvider? ServiceProvider { get; }

    /// <summary>
    /// The content root, resolved against the title's install directory exactly
    /// like the device runtime (<c>Path.GetFullPath(TitleLocation + value)</c>).
    /// Titles rely on this being absolute: for example a ZuneGames title builds
    /// <c>RootDirectory + "\\Content\\Language"</c> for its localization file.
    /// </summary>
    public string RootDirectory
    {
        get => _rootDirectory;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            string baseDirectory = string.IsNullOrEmpty(PlatformHost.AppDirectory)
                ? Directory.GetCurrentDirectory()
                : PlatformHost.AppDirectory;
            _rootDirectory = Path.GetFullPath(Path.Combine(baseDirectory, value));
        }
    }

    public virtual T Load<T>(string assetName)
    {
        string? path = ResolvePathOrNull(assetName);
        if (path is null)
        {
            if (typeof(T) == typeof(Texture2D))
            {
                string? image = ResolveImagePath(assetName);
                if (image is not null)
                {
                    return (T)(object)ZuneImageLoader.Load(image);
                }
            }

            throw new FileNotFoundException($"Content asset '{assetName}' was not found.", assetName);
        }

        byte[] data = File.ReadAllBytes(path);
        object? value = XnbReader.Read(data, this, assetName);
        if (value is T typed)
        {
            return typed;
        }

        if (value is null)
        {
            return default!;
        }

        throw new ContentLoadException(
            $"Content '{assetName}' is a {value.GetType().Name}, not {typeof(T).Name}.");
    }

    /// <summary>
    /// Loads an asset and hands any <see cref="IDisposable"/> result to
    /// <paramref name="recordDisposableObject"/> for later unloading, matching
    /// the XNA 3.1 content pipeline contract.
    /// </summary>
    public T ReadAsset<T>(string assetName, Action<IDisposable>? recordDisposableObject)
    {
        T asset = Load<T>(assetName);
        if (recordDisposableObject is not null && asset is IDisposable disposable)
        {
            recordDisposableObject(disposable);
        }

        return asset;
    }

    public virtual void Unload()
    {
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    protected virtual Stream OpenStream(string assetName) =>
        File.OpenRead(ResolvePathOrNull(assetName) ??
            throw new FileNotFoundException($"Content asset '{assetName}' was not found.", assetName));

    private string? ResolvePathOrNull(string assetName)
    {
        string name = assetName
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);

        if (Path.IsPathRooted(name))
        {
            return ResolveExisting(name);
        }

        // Titles sometimes pass paths that already include the root directory
        // (for example "Content\Audio\blank"); accept both resolutions.
        return ResolveExisting(Path.Combine(RootDirectory, name)) ??
               ResolveExisting(name) ??
               ResolveExisting(Path.Combine(Directory.GetCurrentDirectory(), name));
    }

    private string? ResolveImagePath(string assetName)
    {
        string name = assetName
            .Replace('\\', Path.DirectorySeparatorChar)
            .Replace('/', Path.DirectorySeparatorChar);
        string[] extensions = { ".png", ".jpg", ".jpeg", ".bmp" };
        foreach (string extension in extensions)
        {
            string? resolved = Path.IsPathRooted(name)
                ? ResolveExisting(name + extension)
                : ResolveExisting(Path.Combine(RootDirectory, name + extension)) ??
                  ResolveExisting(Path.Combine(RootDirectory, "Content", name + extension)) ??
                  ResolveExisting(name + extension);
            if (resolved is not null)
            {
                return resolved;
            }
        }

        return null;
    }

    /// <summary>
    /// Resolves <paramref name="path"/> exactly, then with an appended
    /// <c>.xnb</c>, then case-insensitively. The official titles were built for
    /// Windows, so their asset names do not always match on-disk casing (for
    /// example <c>"Menu\Leaderboard\Box"</c> for <c>box.xnb</c>).
    /// </summary>
    private static string? ResolveExisting(string path)
    {
        if (File.Exists(path))
        {
            return path;
        }

        string withExtension = path + ".xnb";
        if (File.Exists(withExtension))
        {
            return withExtension;
        }

        return FindCaseInsensitive(path) ?? FindCaseInsensitive(withExtension);
    }

    private static string? FindCaseInsensitive(string path)
    {
        string full;
        try
        {
            full = Path.GetFullPath(path);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return null;
        }

        string root = Path.GetPathRoot(full) ?? string.Empty;
        string[] segments = full[root.Length..].Split(
            Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        string current = root.Length == 0 ? Directory.GetCurrentDirectory() : root;

        foreach (string segment in segments)
        {
            if (!Directory.Exists(current))
            {
                return null;
            }

            string? match = null;
            foreach (string directory in Directory.EnumerateDirectories(current))
            {
                if (string.Equals(Path.GetFileName(directory), segment, StringComparison.OrdinalIgnoreCase))
                {
                    match = directory;
                    break;
                }
            }

            if (match is null)
            {
                foreach (string file in Directory.EnumerateFiles(current))
                {
                    if (string.Equals(Path.GetFileName(file), segment, StringComparison.OrdinalIgnoreCase))
                    {
                        match = file;
                        break;
                    }
                }
            }

            if (match is null)
            {
                return null;
            }

            current = match;
        }

        return File.Exists(current) ? current : null;
    }
}
