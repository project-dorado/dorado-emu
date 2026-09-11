using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content;

/// <summary>Base class for XNB type readers; mirrors the XNA 3.1 surface.</summary>
public abstract class ContentTypeReader
{
    protected ContentTypeReader(Type targetType)
    {
        TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
    }

    public virtual bool CanDeserializeIntoExistingObject => false;

    public Type TargetType { get; }

    public virtual int TypeVersion => 0;

    internal bool TargetIsValueType => TargetType.IsValueType;

    protected internal virtual void Initialize(ContentTypeReaderManager manager)
    {
    }

    protected internal abstract object? Read(ContentReader input, object? existingInstance);
}

/// <summary>A strongly typed XNB type reader.</summary>
public abstract class ContentTypeReader<T> : ContentTypeReader
{
    protected ContentTypeReader()
        : base(typeof(T))
    {
    }

    protected internal override object? Read(ContentReader input, object? existingInstance) =>
        Read(input, existingInstance is T typed ? typed : default!);

    protected internal abstract T Read(ContentReader input, T? existingInstance);
}

/// <summary>Resolves the type readers an XNB asset declares; app-local readers are instantiated reflectively.</summary>
public sealed class ContentTypeReaderManager
{
    private readonly Dictionary<Type, ContentTypeReader> _byType = new();

    public ContentTypeReader GetTypeReader(Type targetType)
    {
        ArgumentNullException.ThrowIfNull(targetType);
        if (_byType.TryGetValue(targetType, out ContentTypeReader? reader))
        {
            return reader;
        }

        reader = BuiltInReaders.CreateForType(targetType);
        _byType[targetType] = reader;
        return reader;
    }

    internal void Register(ContentTypeReader reader) => _byType[reader.TargetType] = reader;

    internal static ContentTypeReader Create(string typeName, ContentTypeReaderManager manager)
    {
        ContentTypeReader reader = BuiltInReaders.Create(typeName) ?? ReflectiveReaders.Create(typeName);
        reader.Initialize(manager);
        return reader;
    }
}

/// <summary>Streams values from an XNB asset; the API apps bind against.</summary>
public sealed class ContentReader : BinaryReader
{
    private readonly IReadOnlyList<ContentTypeReader> _readers;
    private readonly ContentTypeReaderManager _manager;
    private readonly ContentManager? _contentManager;

    internal ContentReader(
        Stream stream,
        ContentTypeReaderManager manager,
        IReadOnlyList<ContentTypeReader> readers,
        ContentManager? contentManager)
        : base(stream, Encoding.UTF8, leaveOpen: true)
    {
        _manager = manager;
        _readers = readers;
        _contentManager = contentManager;
    }

    public string AssetName { get; internal set; } = string.Empty;

    public ContentManager ContentManager =>
        _contentManager ?? throw new InvalidOperationException("This reader has no content manager.");

    public T ReadObject<T>() => ReadObjectInternal<T>(null);

    public T ReadObject<T>(T? existingInstance) => ReadObjectInternal<T>(existingInstance);

    public T ReadObject<T>(ContentTypeReader typeReader) => ReadObjectInternal<T>(typeReader, null);

    public T ReadObject<T>(ContentTypeReader typeReader, T? existingInstance) =>
        ReadObjectInternal<T>(typeReader, existingInstance);

    public T ReadRawObject<T>() => Invoke<T>(_manager.GetTypeReader(typeof(T)), null);

    public T ReadRawObject<T>(T? existingInstance) =>
        Invoke<T>(_manager.GetTypeReader(typeof(T)), existingInstance);

    public T ReadRawObject<T>(ContentTypeReader typeReader)
    {
        ArgumentNullException.ThrowIfNull(typeReader);
        return Invoke<T>(typeReader, null);
    }

    public T ReadRawObject<T>(ContentTypeReader typeReader, T? existingInstance)
    {
        ArgumentNullException.ThrowIfNull(typeReader);
        return Invoke<T>(typeReader, existingInstance);
    }

    private T ReadObjectInternal<T>(object? existingInstance)
    {
        int index = Read7Bit();
        if (index == 0)
        {
            return default!;
        }

        index--;
        if ((uint)index >= _readers.Count - 1)
        {
            throw new ContentLoadException($"Invalid XNB type-reader index {index + 1}.");
        }

        return Invoke<T>(_readers[index + 1], existingInstance);
    }

    private T ReadObjectInternal<T>(ContentTypeReader typeReader, object? existingInstance)
    {
        ArgumentNullException.ThrowIfNull(typeReader);
        if (typeReader.TargetIsValueType)
        {
            return Invoke<T>(typeReader, existingInstance);
        }

        return ReadObjectInternal<T>(existingInstance);
    }

    private T Invoke<T>(ContentTypeReader reader, object? existingInstance)
    {
        object? value = reader.Read(this, existingInstance);
        if (value is null && default(T) is null)
        {
            return default!;
        }

        if (value is T typed)
        {
            return typed;
        }

        throw new ContentLoadException(
            $"Content reader '{reader.GetType().Name}' produced {value?.GetType().Name ?? "null"}, not {typeof(T).Name}.");
    }

    public void ReadSharedResource<T>(Action<T> fixup)
    {
        ArgumentNullException.ThrowIfNull(fixup);
        _ = Read7Bit();
    }

    public T ReadExternalReference<T>() =>
        throw new NotSupportedException("External references are not used by Zune HD content.");

    public Vector2 ReadVector2() => new(ReadSingle(), ReadSingle());

    public Vector3 ReadVector3() => new(ReadSingle(), ReadSingle(), ReadSingle());

    public Vector4 ReadVector4() => new(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());

    public Quaternion ReadQuaternion() => new(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());

    public Color ReadColor() => new(ReadUInt32());

    public Matrix ReadMatrix()
    {
        Matrix value = default;
        value.M11 = ReadSingle();
        value.M12 = ReadSingle();
        value.M13 = ReadSingle();
        value.M14 = ReadSingle();
        value.M21 = ReadSingle();
        value.M22 = ReadSingle();
        value.M23 = ReadSingle();
        value.M24 = ReadSingle();
        value.M31 = ReadSingle();
        value.M32 = ReadSingle();
        value.M33 = ReadSingle();
        value.M34 = ReadSingle();
        value.M41 = ReadSingle();
        value.M42 = ReadSingle();
        value.M43 = ReadSingle();
        value.M44 = ReadSingle();
        return value;
    }

    internal int Read7Bit() => base.Read7BitEncodedInt();
}

/// <summary>Resolves app-local XNB type readers (for example Microsoft.Xna.Zune's Gl readers).</summary>
internal static class ReflectiveReaders
{
    public static ContentTypeReader Create(string typeName)
    {
        Type? type = ResolveType(typeName);
        if (type is null || !typeof(ContentTypeReader).IsAssignableFrom(type))
        {
            throw new NotSupportedException($"Unsupported XNB content reader '{typeName}'.");
        }

        return Activator.CreateInstance(type) as ContentTypeReader ??
            throw new NotSupportedException($"Could not instantiate XNB content reader '{typeName}'.");
    }

    private static Type? ResolveType(string typeName)
    {
        string name = TypeNames.StripAssembly(typeName);
        foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? type = assembly.GetType(name, throwOnError: false);
            if (type is not null)
            {
                return type;
            }

            type = assembly.GetType(TypeNames.WithoutNamespace(name), throwOnError: false);
            if (type is not null)
            {
                return type;
            }
        }

        return null;
    }
}
