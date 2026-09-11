namespace Microsoft.Xna.Framework.Content;

/// <summary>Raised when content cannot be loaded or does not match the requested type.</summary>
[Serializable]
public class ContentLoadException : Exception
{
    public ContentLoadException()
        : base("Content could not be loaded.")
    {
    }

    public ContentLoadException(string message)
        : base(message)
    {
    }

    public ContentLoadException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
