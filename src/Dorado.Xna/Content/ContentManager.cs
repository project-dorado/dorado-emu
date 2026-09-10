namespace Microsoft.Xna.Framework.Content;

/// <summary>Loads XNB content relative to a root directory.</summary>
public class ContentManager : IDisposable
{
    public ContentManager()
    {
    }

    public ContentManager(IServiceProvider serviceProvider)
    {
    }

    public string RootDirectory { get; set; } = "Content";

    public T Load<T>(string assetName)
    {
        string path = ResolvePath(assetName);
        byte[] data = File.ReadAllBytes(path);
        object value = XnbReader.Read(data);
        if (value is T typed)
        {
            return typed;
        }

        throw new InvalidCastException($"Content '{assetName}' is a {value.GetType().Name}, not {typeof(T).Name}.");
    }

    public void Unload()
    {
    }

    public void Dispose()
    {
    }

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

        throw new FileNotFoundException($"Content asset '{assetName}' was not found.", combined);
    }
}
