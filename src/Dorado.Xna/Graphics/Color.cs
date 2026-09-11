namespace Microsoft.Xna.Framework.Graphics;

/// <summary>A 32-bit RGBA colour; every XNA 3.1 named colour is available.</summary>
public struct Color : IEquatable<Color>
{
    public Color(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public Color(byte r, byte g, byte b)
        : this(r, g, b, 255)
    {
    }

    public Color(float r, float g, float b)
        : this((byte)(r * 255f), (byte)(g * 255f), (byte)(b * 255f), (byte)255)
    {
    }

    public Color(float r, float g, float b, float a)
        : this((byte)(r * 255f), (byte)(g * 255f), (byte)(b * 255f), (byte)(a * 255f))
    {
    }

    public Color(Color rgb, byte a)
        : this(rgb.R, rgb.G, rgb.B, a)
    {
    }

    public Color(Color rgb, float a)
        : this(rgb.R, rgb.G, rgb.B, (byte)(a * 255f))
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

    public Color(Vector4 vector)
        : this((byte)(vector.X * 255f), (byte)(vector.Y * 255f), (byte)(vector.Z * 255f), (byte)(vector.W * 255f))
    {
    }

    public byte R { get; set; }

    public byte G { get; set; }

    public byte B { get; set; }

    public byte A { get; set; }

    public static Color Transparent => new(0x00000000);

    public static Color TransparentBlack => new(0x00000000);

    public static Color TransparentWhite => new(0x00FFFFFF);

    public static Color AliceBlue => new(0xFFF0F8FF);

    public static Color AntiqueWhite => new(0xFFFAEBD7);

    public static Color Aqua => new(0xFF00FFFF);

    public static Color Aquamarine => new(0xFF7FFFD4);

    public static Color Azure => new(0xFFF0FFFF);

    public static Color Beige => new(0xFFF5F5DC);

    public static Color Bisque => new(0xFFFFE4C4);

    public static Color Black => new(0xFF000000);

    public static Color BlanchedAlmond => new(0xFFFFEBCD);

    public static Color Blue => new(0xFF0000FF);

    public static Color BlueViolet => new(0xFF8A2BE2);

    public static Color Brown => new(0xFFA52A2A);

    public static Color BurlyWood => new(0xFFDEB887);

    public static Color CadetBlue => new(0xFF5F9EA0);

    public static Color Chartreuse => new(0xFF7FFF00);

    public static Color Chocolate => new(0xFFD2691E);

    public static Color Coral => new(0xFFFF7F50);

    public static Color CornflowerBlue => new(0xFF6495ED);

    public static Color Cornsilk => new(0xFFFFF8DC);

    public static Color Crimson => new(0xFFDC143C);

    public static Color Cyan => new(0xFF00FFFF);

    public static Color DarkBlue => new(0xFF00008B);

    public static Color DarkCyan => new(0xFF008B8B);

    public static Color DarkGoldenrod => new(0xFFB8860B);

    public static Color DarkGray => new(0xFFA9A9A9);

    public static Color DarkGreen => new(0xFF006400);

    public static Color DarkKhaki => new(0xFFBDB76B);

    public static Color DarkMagenta => new(0xFF8B008B);

    public static Color DarkOliveGreen => new(0xFF556B2F);

    public static Color DarkOrange => new(0xFFFF8C00);

    public static Color DarkOrchid => new(0xFF9932CC);

    public static Color DarkRed => new(0xFF8B0000);

    public static Color DarkSalmon => new(0xFFE9967A);

    public static Color DarkSeaGreen => new(0xFF8FBC8B);

    public static Color DarkSlateBlue => new(0xFF483D8B);

    public static Color DarkSlateGray => new(0xFF2F4F4F);

    public static Color DarkTurquoise => new(0xFF00CED1);

    public static Color DarkViolet => new(0xFF9400D3);

    public static Color DeepPink => new(0xFFFF1493);

    public static Color DeepSkyBlue => new(0xFF00BFFF);

    public static Color DimGray => new(0xFF696969);

    public static Color DodgerBlue => new(0xFF1E90FF);

    public static Color Firebrick => new(0xFFB22222);

    public static Color FloralWhite => new(0xFFFFFAF0);

    public static Color ForestGreen => new(0xFF228B22);

    public static Color Fuchsia => new(0xFFFF00FF);

    public static Color Gainsboro => new(0xFFDCDCDC);

    public static Color GhostWhite => new(0xFFF8F8FF);

    public static Color Gold => new(0xFFFFD700);

    public static Color Goldenrod => new(0xFFDAA520);

    public static Color Gray => new(0xFF808080);

    public static Color Green => new(0xFF008000);

    public static Color GreenYellow => new(0xFFADFF2F);

    public static Color Honeydew => new(0xFFF0FFF0);

    public static Color HotPink => new(0xFFFF69B4);

    public static Color IndianRed => new(0xFFCD5C5C);

    public static Color Indigo => new(0xFF4B0082);

    public static Color Ivory => new(0xFFFFFFF0);

    public static Color Khaki => new(0xFFF0E68C);

    public static Color Lavender => new(0xFFE6E6FA);

    public static Color LavenderBlush => new(0xFFFFF0F5);

    public static Color LawnGreen => new(0xFF7CFC00);

    public static Color LemonChiffon => new(0xFFFFFACD);

    public static Color LightBlue => new(0xFFADD8E6);

    public static Color LightCoral => new(0xFFF08080);

    public static Color LightCyan => new(0xFFE0FFFF);

    public static Color LightGoldenrodYellow => new(0xFFFAFAD2);

    public static Color LightGray => new(0xFFD3D3D3);

    public static Color LightGreen => new(0xFF90EE90);

    public static Color LightPink => new(0xFFFFB6C1);

    public static Color LightSalmon => new(0xFFFFA07A);

    public static Color LightSeaGreen => new(0xFF20B2AA);

    public static Color LightSkyBlue => new(0xFF87CEFA);

    public static Color LightSlateGray => new(0xFF778899);

    public static Color LightSteelBlue => new(0xFFB0C4DE);

    public static Color LightYellow => new(0xFFFFFFE0);

    public static Color Lime => new(0xFF00FF00);

    public static Color LimeGreen => new(0xFF32CD32);

    public static Color Linen => new(0xFFFAF0E6);

    public static Color Magenta => new(0xFFFF00FF);

    public static Color Maroon => new(0xFF800000);

    public static Color MediumAquamarine => new(0xFF66CDAA);

    public static Color MediumBlue => new(0xFF0000CD);

    public static Color MediumOrchid => new(0xFFBA55D3);

    public static Color MediumPurple => new(0xFF9370DB);

    public static Color MediumSeaGreen => new(0xFF3CB371);

    public static Color MediumSlateBlue => new(0xFF7B68EE);

    public static Color MediumSpringGreen => new(0xFF00FA9A);

    public static Color MediumTurquoise => new(0xFF48D1CC);

    public static Color MediumVioletRed => new(0xFFC71585);

    public static Color MidnightBlue => new(0xFF191970);

    public static Color MintCream => new(0xFFF5FFFA);

    public static Color MistyRose => new(0xFFFFE4E1);

    public static Color Moccasin => new(0xFFFFE4B5);

    public static Color NavajoWhite => new(0xFFFFDEAD);

    public static Color Navy => new(0xFF000080);

    public static Color OldLace => new(0xFFFDF5E6);

    public static Color Olive => new(0xFF808000);

    public static Color OliveDrab => new(0xFF6B8E23);

    public static Color Orange => new(0xFFFFA500);

    public static Color OrangeRed => new(0xFFFF4500);

    public static Color Orchid => new(0xFFDA70D6);

    public static Color PaleGoldenrod => new(0xFFEEE8AA);

    public static Color PaleGreen => new(0xFF98FB98);

    public static Color PaleTurquoise => new(0xFFAFEEEE);

    public static Color PaleVioletRed => new(0xFFDB7093);

    public static Color PapayaWhip => new(0xFFFFEFD5);

    public static Color PeachPuff => new(0xFFFFDAB9);

    public static Color Peru => new(0xFFCD853F);

    public static Color Pink => new(0xFFFFC0CB);

    public static Color Plum => new(0xFFDDA0DD);

    public static Color PowderBlue => new(0xFFB0E0E6);

    public static Color Purple => new(0xFF800080);

    public static Color Red => new(0xFFFF0000);

    public static Color RosyBrown => new(0xFFBC8F8F);

    public static Color RoyalBlue => new(0xFF4169E1);

    public static Color SaddleBrown => new(0xFF8B4513);

    public static Color Salmon => new(0xFFFA8072);

    public static Color SandyBrown => new(0xFFF4A460);

    public static Color SeaGreen => new(0xFF2E8B57);

    public static Color SeaShell => new(0xFFFFF5EE);

    public static Color Sienna => new(0xFFA0522D);

    public static Color Silver => new(0xFFC0C0C0);

    public static Color SkyBlue => new(0xFF87CEEB);

    public static Color SlateBlue => new(0xFF6A5ACD);

    public static Color SlateGray => new(0xFF708090);

    public static Color Snow => new(0xFFFFFAFA);

    public static Color SpringGreen => new(0xFF00FF7F);

    public static Color SteelBlue => new(0xFF4682B4);

    public static Color Tan => new(0xFFD2B48C);

    public static Color Teal => new(0xFF008080);

    public static Color Thistle => new(0xFFD8BFD8);

    public static Color Tomato => new(0xFFFF6347);

    public static Color Turquoise => new(0xFF40E0D0);

    public static Color Violet => new(0xFFEE82EE);

    public static Color Wheat => new(0xFFF5DEB3);

    public static Color White => new(0xFFFFFFFF);

    public static Color WhiteSmoke => new(0xFFF5F5F5);

    public static Color Yellow => new(0xFFFFFF00);

    public static Color YellowGreen => new(0xFF9ACD32);

    public uint PackedValue
    {
        readonly get => ((uint)A << 24) | ((uint)R << 16) | ((uint)G << 8) | B;
        set
        {
            R = (byte)(value >> 16);
            G = (byte)(value >> 8);
            B = (byte)value;
            A = (byte)(value >> 24);
        }
    }

    public readonly Vector3 ToVector3() => new(R / 255f, G / 255f, B / 255f);

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

    public static Color Multiply(Color value, float scale) => new(
        (byte)Math.Clamp(value.R * scale, 0f, 255f),
        (byte)Math.Clamp(value.G * scale, 0f, 255f),
        (byte)Math.Clamp(value.B * scale, 0f, 255f),
        (byte)Math.Clamp(value.A * scale, 0f, 255f));

    public static Color operator *(Color value, float scale) => Multiply(value, scale);

    public static bool operator ==(Color a, Color b) => a.Equals(b);

    public static bool operator !=(Color a, Color b) => !a.Equals(b);

    public readonly bool Equals(Color other) => PackedValue == other.PackedValue;

    public override readonly bool Equals(object? obj) => obj is Color other && Equals(other);

    public override readonly int GetHashCode() => (int)PackedValue;

    public override readonly string ToString() => $"{{R:{R} G:{G} B:{B} A:{A}}}";
}
