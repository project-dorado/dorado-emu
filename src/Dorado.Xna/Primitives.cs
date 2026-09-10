namespace Microsoft.Xna.Framework;

/// <summary>A two-component floating-point vector.</summary>
public struct Vector2 : IEquatable<Vector2>
{
    public float X;
    public float Y;

    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public static Vector2 Zero => new(0f, 0f);

    public static Vector2 One => new(1f, 1f);

    public readonly float LengthSquared() => (X * X) + (Y * Y);

    public readonly float Length() => MathF.Sqrt(LengthSquared());

    public static float Distance(Vector2 value1, Vector2 value2)
    {
        float dx = value1.X - value2.X;
        float dy = value1.Y - value2.Y;
        return MathF.Sqrt((dx * dx) + (dy * dy));
    }

    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);

    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);

    public static Vector2 operator *(Vector2 a, float scale) => new(a.X * scale, a.Y * scale);

    public static Vector2 operator *(float scale, Vector2 a) => new(a.X * scale, a.Y * scale);

    public readonly bool Equals(Vector2 other) => X.Equals(other.X) && Y.Equals(other.Y);

    public override readonly bool Equals(object? obj) => obj is Vector2 other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(X, Y);

    public override readonly string ToString() => $"{{X:{X} Y:{Y}}}";
}

/// <summary>A three-component floating-point vector.</summary>
public struct Vector3 : IEquatable<Vector3>
{
    public float X;
    public float Y;
    public float Z;

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vector3(Vector2 xy, float z)
    {
        X = xy.X;
        Y = xy.Y;
        Z = z;
    }

    public static Vector3 Zero => new(0f, 0f, 0f);

    public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vector3 operator -(Vector3 a, Vector3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vector3 operator *(Vector3 a, float scale) => new(a.X * scale, a.Y * scale, a.Z * scale);

    public readonly bool Equals(Vector3 other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);

    public override readonly bool Equals(object? obj) => obj is Vector3 other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);

    public override readonly string ToString() => $"{{X:{X} Y:{Y} Z:{Z}}}";
}

/// <summary>An axis-aligned integer rectangle; fields match XNA 3.1.</summary>
public struct Rectangle : IEquatable<Rectangle>
{
    public int X;
    public int Y;
    public int Width;
    public int Height;

    public Rectangle(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public static Rectangle Empty => new(0, 0, 0, 0);

    public readonly int Left => X;

    public readonly int Top => Y;

    public readonly int Right => X + Width;

    public readonly int Bottom => Y + Height;

    public readonly bool Contains(int x, int y) => x >= X && x < X + Width && y >= Y && y < Y + Height;

    public readonly bool Equals(Rectangle other) =>
        X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

    public override readonly bool Equals(object? obj) => obj is Rectangle other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

    public override readonly string ToString() => $"{{X:{X} Y:{Y} Width:{Width} Height:{Height}}}";
}

/// <summary>An axis-aligned bounding box.</summary>
public struct BoundingBox : IEquatable<BoundingBox>
{
    public Vector3 Min;
    public Vector3 Max;

    public BoundingBox(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }

    public readonly bool Intersects(BoundingBox other) =>
        Min.X <= other.Max.X && Max.X >= other.Min.X &&
        Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
        Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;

    public readonly bool Equals(BoundingBox other) => Min.Equals(other.Min) && Max.Equals(other.Max);

    public override readonly bool Equals(object? obj) => obj is BoundingBox other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(Min, Max);
}

/// <summary>Controller slot.</summary>
public enum PlayerIndex
{
    One = 0,
    Two = 1,
    Three = 2,
    Four = 3,
}
