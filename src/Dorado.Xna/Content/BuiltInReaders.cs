using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Microsoft.Xna.Framework.Content;

/// <summary>Assembly-qualified XNB type-name helpers.</summary>
internal static class TypeNames
{
    public static (string Name, string[] Arguments) Parse(string typeName)
    {
        string name = StripAssembly(typeName).Trim();
        int bracket = name.IndexOf('[');
        if (bracket < 0)
        {
            return (name, Array.Empty<string>());
        }

        string head = name[..bracket].Trim();
        string arguments = name[bracket..].Trim();
        if (arguments.StartsWith('[') && arguments.EndsWith(']'))
        {
            arguments = arguments[1..^1];
        }

        return (head, SplitTopLevel(arguments));
    }

    public static string StripAssembly(string typeName)
    {
        int depth = 0;
        for (int i = 0; i < typeName.Length; i++)
        {
            char c = typeName[i];
            if (c == '[')
            {
                depth++;
            }
            else if (c == ']')
            {
                depth--;
            }
            else if (c == ',' && depth == 0)
            {
                return typeName[..i].Trim();
            }
        }

        return typeName.Trim();
    }

    public static string WithoutNamespace(string typeName)
    {
        int lastDot = typeName.LastIndexOf('.');
        return lastDot < 0 ? typeName : typeName[(lastDot + 1)..];
    }

    public static Type? Resolve(string typeName)
    {
        string name = StripAssembly(typeName);
        Type? type = Type.GetType(name, throwOnError: false);
        if (type is not null)
        {
            return type;
        }

        string simple = WithoutNamespace(name);
        foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(name, throwOnError: false) ?? assembly.GetType(simple, throwOnError: false);
            if (type is not null)
            {
                return type;
            }
        }

        return null;
    }

    private static string[] SplitTopLevel(string text)
    {
        var parts = new List<string>();
        int depth = 0;
        int start = 0;
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c == '[')
            {
                depth++;
            }
            else if (c == ']')
            {
                depth--;
            }
            else if (c == ',' && depth == 0)
            {
                parts.Add(text[start..i].Trim());
                start = i + 1;
            }
        }

        parts.Add(text[start..].Trim());
        for (int i = 0; i < parts.Count; i++)
        {
            string part = parts[i];
            if (part.StartsWith('[') && part.EndsWith(']'))
            {
                part = part[1..^1];
            }

            parts[i] = StripAssembly(part);
        }

        return parts.Where(p => p.Length > 0).ToArray();
    }
}

/// <summary>The XNA 3.1 built-in XNB readers.</summary>
internal static class BuiltInReaders
{
    public static ContentTypeReader? Create(string typeName)
    {
        (string name, string[] arguments) = TypeNames.Parse(typeName);
        switch (TypeNames.WithoutNamespace(name))
        {
            case "Texture2DReader": return new Texture2DReader();
            case "SoundEffectReader": return new SoundEffectReader();
            case "SongReader": return new SongReader();
            case "SpriteFontReader": return new SpriteFontReader();
            case "StringReader": return new StringReader();
            case "CharReader": return new CharReader();
            case "BooleanReader": return new BooleanReader();
            case "ByteReader": return new ByteReader();
            case "Int32Reader": return new Int32Reader();
            case "UInt32Reader": return new UInt32Reader();
            case "SingleReader": return new SingleReader();
            case "DoubleReader": return new DoubleReader();
            case "RectangleReader": return new RectangleReader();
            case "Vector2Reader": return new Vector2Reader();
            case "Vector3Reader": return new Vector3Reader();
            case "Vector4Reader": return new Vector4Reader();
            case "MatrixReader": return new MatrixReader();
            case "QuaternionReader": return new QuaternionReader();
            case "ColorReader": return new ColorReader();
            case "PointReader": return new PointReader();
            case "ListReader`1": return CreateGeneric(typeof(ListReader<>), arguments);
            case "ArrayReader`1": return CreateGeneric(typeof(ArrayReader<>), arguments);
            case "DictionaryReader`2": return CreateGeneric(typeof(DictionaryReader<,>), arguments);
            case "NullableReader`1": return CreateGeneric(typeof(NullableReader<>), arguments);
            case "EnumReader`1": return CreateGeneric(typeof(EnumReader<>), arguments);
            default: return null;
        }
    }

    public static ContentTypeReader CreateForType(Type targetType)
    {
        ArgumentNullException.ThrowIfNull(targetType);
        if (targetType == typeof(Texture2D)) return new Texture2DReader();
        if (targetType == typeof(SoundEffect)) return new SoundEffectReader();
        if (targetType == typeof(Song)) return new SongReader();
        if (targetType == typeof(SpriteFont)) return new SpriteFontReader();
        if (targetType == typeof(string)) return new StringReader();
        if (targetType == typeof(char)) return new CharReader();
        if (targetType == typeof(bool)) return new BooleanReader();
        if (targetType == typeof(byte)) return new ByteReader();
        if (targetType == typeof(int)) return new Int32Reader();
        if (targetType == typeof(uint)) return new UInt32Reader();
        if (targetType == typeof(float)) return new SingleReader();
        if (targetType == typeof(double)) return new DoubleReader();
        if (targetType == typeof(Rectangle)) return new RectangleReader();
        if (targetType == typeof(Vector2)) return new Vector2Reader();
        if (targetType == typeof(Vector3)) return new Vector3Reader();
        if (targetType == typeof(Vector4)) return new Vector4Reader();
        if (targetType == typeof(Matrix)) return new MatrixReader();
        if (targetType == typeof(Quaternion)) return new QuaternionReader();
        if (targetType == typeof(Color)) return new ColorReader();
        if (targetType == typeof(Point)) return new PointReader();
        if (targetType.IsArray && targetType.GetArrayRank() == 1)
        {
            return (ContentTypeReader)Activator.CreateInstance(typeof(ArrayReader<>).MakeGenericType(targetType.GetElementType()!))!;
        }

        if (targetType.IsGenericType)
        {
            Type definition = targetType.GetGenericTypeDefinition();
            Type[] arguments = targetType.GetGenericArguments();
            if (definition == typeof(List<>)) return CreateGeneric(typeof(ListReader<>), arguments);
            if (definition == typeof(Dictionary<,>)) return CreateGeneric(typeof(DictionaryReader<,>), arguments);
            if (definition == typeof(Nullable<>)) return CreateGeneric(typeof(NullableReader<>), arguments);
        }

        if (targetType.IsEnum)
        {
            return (ContentTypeReader)Activator.CreateInstance(typeof(EnumReader<>).MakeGenericType(targetType))!;
        }

        throw new NotSupportedException($"No XNB type reader for '{targetType.FullName}'.");
    }

    private static ContentTypeReader? CreateGeneric(Type definition, string[] arguments)
    {
        Type[] types = new Type[arguments.Length];
        for (int i = 0; i < arguments.Length; i++)
        {
            Type? resolved = TypeNames.Resolve(arguments[i]);
            if (resolved is null)
            {
                return null;
            }

            types[i] = resolved;
        }

        return CreateGeneric(definition, types);
    }

    private static ContentTypeReader CreateGeneric(Type definition, Type[] types) =>
        (ContentTypeReader)Activator.CreateInstance(definition.MakeGenericType(types))!;
}

internal sealed class StringReader : ContentTypeReader<string>
{
    protected internal override string Read(ContentReader input, string? existingInstance) => input.ReadString();
}

internal sealed class CharReader : ContentTypeReader<char>
{
    protected internal override char Read(ContentReader input, char existingInstance) => input.ReadChar();
}

internal sealed class BooleanReader : ContentTypeReader<bool>
{
    protected internal override bool Read(ContentReader input, bool existingInstance) => input.ReadBoolean();
}

internal sealed class ByteReader : ContentTypeReader<byte>
{
    protected internal override byte Read(ContentReader input, byte existingInstance) => input.ReadByte();
}

internal sealed class Int32Reader : ContentTypeReader<int>
{
    protected internal override int Read(ContentReader input, int existingInstance) => input.ReadInt32();
}

internal sealed class UInt32Reader : ContentTypeReader<uint>
{
    protected internal override uint Read(ContentReader input, uint existingInstance) => input.ReadUInt32();
}

internal sealed class SingleReader : ContentTypeReader<float>
{
    protected internal override float Read(ContentReader input, float existingInstance) => input.ReadSingle();
}

internal sealed class DoubleReader : ContentTypeReader<double>
{
    protected internal override double Read(ContentReader input, double existingInstance) => input.ReadDouble();
}

internal sealed class RectangleReader : ContentTypeReader<Rectangle>
{
    protected internal override Rectangle Read(ContentReader input, Rectangle existingInstance) =>
        new(input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
}

internal sealed class Vector2Reader : ContentTypeReader<Vector2>
{
    protected internal override Vector2 Read(ContentReader input, Vector2 existingInstance) => input.ReadVector2();
}

internal sealed class Vector3Reader : ContentTypeReader<Vector3>
{
    protected internal override Vector3 Read(ContentReader input, Vector3 existingInstance) => input.ReadVector3();
}

internal sealed class Vector4Reader : ContentTypeReader<Vector4>
{
    protected internal override Vector4 Read(ContentReader input, Vector4 existingInstance) => input.ReadVector4();
}

internal sealed class MatrixReader : ContentTypeReader<Matrix>
{
    protected internal override Matrix Read(ContentReader input, Matrix existingInstance) => input.ReadMatrix();
}

internal sealed class QuaternionReader : ContentTypeReader<Quaternion>
{
    protected internal override Quaternion Read(ContentReader input, Quaternion existingInstance) => input.ReadQuaternion();
}

internal sealed class ColorReader : ContentTypeReader<Color>
{
    protected internal override Color Read(ContentReader input, Color existingInstance) => input.ReadColor();
}

internal sealed class PointReader : ContentTypeReader<Point>
{
    protected internal override Point Read(ContentReader input, Point existingInstance) =>
        new(input.ReadInt32(), input.ReadInt32());
}

internal sealed class ListReader<T> : ContentTypeReader<List<T>>
{
    private ContentTypeReader? _elementReader;

    protected internal override void Initialize(ContentTypeReaderManager manager) =>
        _elementReader = manager.GetTypeReader(typeof(T));

    protected internal override List<T> Read(ContentReader input, List<T>? existingInstance)
    {
        int count = input.ReadInt32();
        var list = existingInstance ?? new List<T>(count);
        list.Clear();
        for (int i = 0; i < count; i++)
        {
            list.Add(input.ReadObject<T>(ElementReader));
        }

        return list;
    }

    private ContentTypeReader ElementReader =>
        _elementReader ?? throw new InvalidOperationException("The list reader was not initialized.");
}

internal sealed class ArrayReader<T> : ContentTypeReader<T[]>
{
    private ContentTypeReader? _elementReader;

    protected internal override void Initialize(ContentTypeReaderManager manager) =>
        _elementReader = manager.GetTypeReader(typeof(T));

    protected internal override T[] Read(ContentReader input, T[]? existingInstance)
    {
        int count = input.ReadInt32();
        var array = new T[count];
        for (int i = 0; i < count; i++)
        {
            array[i] = input.ReadObject<T>(
                _elementReader ?? throw new InvalidOperationException("The array reader was not initialized."));
        }

        return array;
    }
}

internal sealed class DictionaryReader<TKey, TValue> : ContentTypeReader<Dictionary<TKey, TValue>>
    where TKey : notnull
{
    private ContentTypeReader? _keyReader;
    private ContentTypeReader? _valueReader;

    protected internal override void Initialize(ContentTypeReaderManager manager)
    {
        _keyReader = manager.GetTypeReader(typeof(TKey));
        _valueReader = manager.GetTypeReader(typeof(TValue));
    }

    protected internal override Dictionary<TKey, TValue> Read(
        ContentReader input,
        Dictionary<TKey, TValue>? existingInstance)
    {
        int count = input.ReadInt32();
        var dictionary = existingInstance ?? new Dictionary<TKey, TValue>(count);
        dictionary.Clear();
        for (int i = 0; i < count; i++)
        {
            TKey key = input.ReadObject<TKey>(
                _keyReader ?? throw new InvalidOperationException("The dictionary reader was not initialized."));
            TValue value = input.ReadObject<TValue>(
                _valueReader ?? throw new InvalidOperationException("The dictionary reader was not initialized."));
            dictionary[key] = value;
        }

        return dictionary;
    }
}

internal sealed class NullableReader<T> : ContentTypeReader<T?>
    where T : struct
{
    protected internal override T? Read(ContentReader input, T? existingInstance) =>
        input.ReadBoolean() ? input.ReadObject<T>() : null;
}

internal sealed class EnumReader<T> : ContentTypeReader<T>
    where T : struct, Enum
{
    protected internal override T Read(ContentReader input, T existingInstance) =>
        (T)Enum.ToObject(typeof(T), input.ReadInt32());
}

internal sealed class Texture2DReader : ContentTypeReader<Texture2D>
{
    protected internal override Texture2D Read(ContentReader input, Texture2D? existingInstance)
    {
        int surfaceFormat = input.ReadInt32();
        int width = input.ReadInt32();
        int height = input.ReadInt32();
        int mipCount = input.ReadInt32();
        int dataSize = input.ReadInt32();
        if (dataSize < 0)
        {
            throw new InvalidDataException("Texture payload is truncated.");
        }

        byte[] data = input.ReadBytes(dataSize);
        byte[] pixels = XnbPixelDecoder.Decode(data, width, height, surfaceFormat, mipCount);
        return Texture2D.FromPixels(width, height, pixels, premultiplied: true);
    }
}

internal sealed class SoundEffectReader : ContentTypeReader<SoundEffect>
{
    protected internal override SoundEffect Read(ContentReader input, SoundEffect? existingInstance)
    {
        int formatSize = input.ReadInt32();
        if (formatSize < 0)
        {
            throw new InvalidDataException("Sound effect format is truncated.");
        }

        byte[] format = input.ReadBytes(formatSize);
        int dataSize = input.ReadInt32();
        if (dataSize < 0)
        {
            throw new InvalidDataException("Sound effect payload is truncated.");
        }

        byte[] pcm = input.ReadBytes(dataSize);
        int loopStart = input.ReadInt32();
        int loopLength = input.ReadInt32();
        int duration = input.ReadInt32();

        return SoundEffect.FromRaw(pcm, format, duration, loopStart, loopLength);
    }
}

/// <summary>
/// Reads a Zune media-library reference. XNA stores only the song's file path,
/// so the offline shim surfaces that as a <see cref="Song"/> with no playback.
/// </summary>
internal sealed class SongReader : ContentTypeReader<Song>
{
    protected internal override Song Read(ContentReader input, Song? existingInstance)
    {
        string fileName = input.ReadString();
        string name = Path.GetFileNameWithoutExtension(fileName);
        return new Song(string.IsNullOrEmpty(name) ? fileName : name, fileName, 0);
    }
}

internal sealed class SpriteFontReader : ContentTypeReader<SpriteFont>
{
    protected internal override SpriteFont Read(ContentReader input, SpriteFont? existingInstance)
    {
        Texture2D texture = input.ReadObject<Texture2D>();
        List<Rectangle> glyphs = input.ReadObject<List<Rectangle>>();
        List<Rectangle> cropping = input.ReadObject<List<Rectangle>>();
        List<char> characters = input.ReadObject<List<char>>();
        int lineSpacing = input.ReadInt32();
        float spacing = input.ReadSingle();
        List<Vector3> kerning = input.ReadObject<List<Vector3>>();
        char? defaultCharacter = null;
        if (input.ReadBoolean())
        {
            defaultCharacter = input.ReadChar();
        }

        return new SpriteFont(texture, glyphs, cropping, characters, lineSpacing, spacing, kerning, defaultCharacter);
    }
}

/// <summary>Shared XNB pixel decoding for texture readers.</summary>
internal static class XnbPixelDecoder
{
    public static byte[] Decode(byte[] data, int width, int height, int surfaceFormat, int mipCount)
    {
        if (width <= 0 || height <= 0)
        {
            throw new InvalidDataException("Texture has invalid dimensions.");
        }

        int pixels = width * height;
        if (data.Length == pixels * 4)
        {
            return data;
        }

        if (data.Length == pixels * 2)
        {
            var rgba = new byte[pixels * 4];
            for (int i = 0; i < pixels; i++)
            {
                int packed = data[(i * 2)] | (data[(i * 2) + 1] << 8);
                byte r = (byte)((packed >> 11) & 0x1F);
                byte g = (byte)((packed >> 5) & 0x3F);
                byte b = (byte)(packed & 0x1F);
                rgba[i * 4] = (byte)((r << 3) | (r >> 2));
                rgba[(i * 4) + 1] = (byte)((g << 2) | (g >> 4));
                rgba[(i * 4) + 2] = (byte)((b << 3) | (b >> 2));
                rgba[(i * 4) + 3] = 255;
            }

            return rgba;
        }

        throw new NotSupportedException(
            $"Unsupported texture encoding: {data.Length} bytes for {width}x{height} (surface format {surfaceFormat}, {mipCount} mips).");
    }
}
