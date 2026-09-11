namespace Microsoft.Xna.Framework.Content;

/// <summary>Loads XNB content relative to a root directory.</summary>
public class ContentManager : IDisposable
{
    public ContentManager()
    {
    }

    public ContentManager(IServiceProvider serviceProvider)
        : this(serviceProvider, "Content")
    {
    }

    public ContentManager(IServiceProvider serviceProvider, string rootDirectory)
    {
        ServiceProvider = serviceProvider;
        RootDirectory = rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory));
    }

    public IServiceProvider? ServiceProvider { get; }

    public string RootDirectory { get; set; } = "Content";

    public virtual T Load<T>(string assetName)
    {
        string path = ResolvePath(assetName);
        byte[] data = File.ReadAllBytes(path);
        object value = XnbReader.Read(data);
        if (value is T typed)
        {
            return typed;
        }

        throw new ContentLoadException(
            $"Content '{assetName}' is a {value.GetType().Name}, not {typeof(T).Name}.");
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

    protected virtual Stream OpenStream(string assetName) => File.OpenRead(ResolvePath(assetName));

    private string ResolvePath(string assetName)
    {
        string name = assetName.Replace('\\', Path.DirectorySeparatorChar);
        string combined = Path.IsPathRooted(name) ? name : Path.Combine(RootDirectory, name);
        if (File.Exists(combined))
        {
            return combined;
        }

        string withExtension = combined + ".xnb";
        if (File.Exists(withExtension))
        {
            return withExtension;
        }

        string rooted = Path.IsPathRooted(name)
            ? name
            : Path.Combine(Directory.GetCurrentDirectory(), combined);
        if (File.Exists(rooted))
        {
            return rooted;
        }

        string rootedExtension = rooted + ".xnb";
        if (File.Exists(rootedExtension))
        {
            return rootedExtension;
        }

        throw new FileNotFoundException($"Content asset '{assetName}' was not found.", combined);
    }
}
