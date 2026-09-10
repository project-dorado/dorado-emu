namespace Microsoft.Xna.Framework.Graphics;

/// <summary>A 32-bit RGBA colour.</summary>
public struct Color : IEquatable<Color>
{
    public byte R;
    public byte G;
    public byte B;
    public byte A;

    public Color(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public Color(int r, int g, int b, int a)
        : this((byte)r, (byte)g, (byte)b, (byte)a)
    {
    }

    public Color(uint packedValue)
    {
        R = (byte)(packedValue >> 16);
        G = (byte)(packedValue >> 8);
        B = (byte)packedValue;
        A = (byte)(packedValue >> 24);
    }

    public Color(Vector3 vector)
        : this((byte)(vector.X * 255f), (byte)(vector.Y * 255f), (byte)(vector.Z * 255f), (byte)255)
    {
    }

    public static Color Transparent => new(0, 0, 0, 0);

    public static Color Black => new(0, 0, 0, 255);

    public static Color White => new(255, 255, 255, 255);

    public static Color LightGray => new(211, 211, 211, 255);

    public static Color Gray => new(128, 128, 128, 255);

    public static Color Red => new(255, 0, 0, 255);

    public static Color Green => new(0, 128, 0, 255);

    public static Color Blue => new(0, 0, 255, 255);

    public static Color Yellow => new(255, 255, 0, 255);

    public static Color Cyan => new(0, 255, 255, 255);

    public static Color Magenta => new(255, 0, 255, 255);

    public static Color Orange => new(255, 165, 0, 255);

    public static Color HotPink => new(255, 105, 180, 255);

    public readonly uint PackedValue => ((uint)A << 24) | ((uint)R << 16) | ((uint)G << 8) | B;

    public readonly Vector4 ToVector4() => new(R / 255f, G / 255f, B / 255f, A / 255f);

    public static Color Lerp(Color value1, Color value2, float amount)
    {
        amount = Math.Clamp(amount, 0f, 1f);
        return new Color(
            (byte)(value1.R + ((value2.R - value1.R) * amount)),
            (byte)(value1.G + ((value2.G - value1.G) * amount)),
            (byte)(value1.B + ((value2.B - value1.B) * amount)),
            (byte)(value1.A + ((value2.A - value1.A) * amount)));
    }

    public static Color operator *(Color value, float scale) => new(
        (byte)Math.Clamp(value.R * scale, 0f, 255f),
        (byte)Math.Clamp(value.G * scale, 0f, 255f),
        (byte)Math.Clamp(value.B * scale, 0f, 255f),
        (byte)Math.Clamp(value.A * scale, 0f, 255f));

    public readonly bool Equals(Color other) => PackedValue == other.PackedValue;

    public override readonly bool Equals(object? obj) => obj is Color other && Equals(other);

    public override readonly int GetHashCode() => (int)PackedValue;

    public override readonly string ToString() => $"{{R:{R} G:{G} B:{B} A:{A}}}";
}

/// <summary>A four-component floating-point vector.</summary>
public struct Vector4 : IEquatable<Vector4>
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public Vector4(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public static Vector4 Zero => new(0f, 0f, 0f, 0f);

    public static Vector4 One => new(1f, 1f, 1f, 1f);

    public readonly bool Equals(Vector4 other) =>
        X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);

    public override readonly bool Equals(object? obj) => obj is Vector4 other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z, W);
}
