using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Dorado.Containers.Internal;

/// <summary>Helpers for reading the .NET <c>.resources</c> value encoding.</summary>
internal static class ResourceBlob
{
    /// <summary>Reads a .NET 7-bit-encoded integer.</summary>
    public static int Read7BitEncodedInt(ReadOnlySpan<byte> data, ref int pos)
    {
        int count = 0;
        int shift = 0;
        byte b;
        do
        {
            if (pos >= data.Length || shift >= 35)
            {
                throw new FormatException("Malformed 7-bit encoded integer.");
            }

            b = data[pos++];
            count |= (b & 0x7F) << shift;
            shift += 7;
        }
        while ((b & 0x80) != 0);

        return count;
    }

    /// <summary>
    /// Decodes a resource value whose type is a primitive string: a 7-bit byte count
    /// followed by UTF-8 bytes (the .NET <see cref="BinaryWriter"/> string encoding).
    /// </summary>
    public static bool TryDecodePrimitiveString(ReadOnlySpan<byte> data, out string value)
    {
        value = string.Empty;
        try
        {
            int pos = 0;
            int len = Read7BitEncodedInt(data, ref pos);
            if (len < 0 || pos + len > data.Length)
            {
                return false;
            }

            value = Encoding.UTF8.GetString(data.Slice(pos, len));
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Deserializes a legacy .resources value. The .NET resource format stores
    /// non-primitive values (for example <c>System.String[]</c>) with
    /// <see cref="BinaryFormatter"/>.
    /// </summary>
    public static object? Deserialize(ReadOnlySpan<byte> data)
    {
#pragma warning disable SYSLIB0011 // BinaryFormatter is required to read legacy .resources arrays.
        using var ms = new MemoryStream(data.ToArray(), writable: false);
        return new BinaryFormatter().Deserialize(ms);
#pragma warning restore SYSLIB0011
    }

    /// <summary>Flattens a deserialized value into its string elements.</summary>
    public static string[] Flatten(object? value)
    {
        switch (value)
        {
            case null:
                return Array.Empty<string>();
            case string s:
                return new[] { s };
            case Array array:
            {
                var result = new List<string>(array.Length);
                foreach (object? item in array)
                {
                    if (item is string str)
                    {
                        result.Add(str);
                    }
                }

                return result.ToArray();
            }
            default:
                return Array.Empty<string>();
        }
    }

    /// <summary>Extracts printable UTF-16LE runs from a blob (fallback/diagnostic).</summary>
    public static IReadOnlyList<string> ExtractUtf16Strings(ReadOnlySpan<byte> data, int minChars = 3)
    {
        var results = new List<string>();
        var sb = new StringBuilder();
        for (int i = 0; i + 1 < data.Length; i += 2)
        {
            char c = (char)(data[i] | (data[i + 1] << 8));
            if (c is >= (char)0x20 and < (char)0x7F)
            {
                sb.Append(c);
            }
            else
            {
                if (sb.Length >= minChars)
                {
                    results.Add(sb.ToString());
                }

                sb.Clear();
            }
        }

        if (sb.Length >= minChars)
        {
            results.Add(sb.ToString());
        }

        return results;
    }
}
