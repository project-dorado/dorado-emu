namespace Microsoft.Xna.Framework;

/// <summary>A two-component integer point; fields match XNA 3.1.</summary>
public struct Point : IEquatable<Point>
{
    public int X;
    public int Y;

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Point(Point value)
    {
        X = value.X;
        Y = value.Y;
    }

    public static Point Zero => new(0, 0);

    public static Point operator +(Point a, Point b) => new(a.X + b.X, a.Y + b.Y);

    public static Point operator -(Point a, Point b) => new(a.X - b.X, a.Y - b.Y);

    public static bool operator ==(Point a, Point b) => a.Equals(b);

    public static bool operator !=(Point a, Point b) => !a.Equals(b);

    public readonly bool Equals(Point other) => X == other.X && Y == other.Y;

    public override readonly bool Equals(object? obj) => obj is Point other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(X, Y);

    public override readonly string ToString() => $"{{X:{X} Y:{Y}}}";
}

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

    public Vector2(float value)
    {
        X = value;
        Y = value;
    }

    public static Vector2 Zero => new(0f, 0f);

    public static Vector2 One => new(1f, 1f);

    public static Vector2 UnitX => new(1f, 0f);

    public static Vector2 UnitY => new(0f, 1f);

    public readonly float LengthSquared() => (X * X) + (Y * Y);

    public readonly float Length() => MathF.Sqrt(LengthSquared());

    public static float Distance(Vector2 value1, Vector2 value2)
    {
        float dx = value1.X - value2.X;
        float dy = value1.Y - value2.Y;
        return MathF.Sqrt((dx * dx) + (dy * dy));
    }

    public static float DistanceSquared(Vector2 value1, Vector2 value2)
    {
        float dx = value1.X - value2.X;
        float dy = value1.Y - value2.Y;
        return (dx * dx) + (dy * dy);
    }

    public static float Dot(Vector2 value1, Vector2 value2) => (value1.X * value2.X) + (value1.Y * value2.Y);

    public static Vector2 Normalize(Vector2 value)
    {
        float length = value.Length();
        return length == 0f ? Zero : new Vector2(value.X / length, value.Y / length);
    }

    public static Vector2 Min(Vector2 value1, Vector2 value2) =>
        new(MathF.Min(value1.X, value2.X), MathF.Min(value1.Y, value2.Y));

    public static Vector2 Max(Vector2 value1, Vector2 value2) =>
        new(MathF.Max(value1.X, value2.X), MathF.Max(value1.Y, value2.Y));

    public static Vector2 Clamp(Vector2 value1, Vector2 min, Vector2 max) =>
        new(Math.Clamp(value1.X, min.X, max.X), Math.Clamp(value1.Y, min.Y, max.Y));

    public static Vector2 Lerp(Vector2 value1, Vector2 value2, float amount) =>
        new(
            value1.X + ((value2.X - value1.X) * amount),
            value1.Y + ((value2.Y - value1.Y) * amount));

    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);

    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);

    public static Vector2 operator -(Vector2 a) => new(-a.X, -a.Y);

    public static Vector2 operator *(Vector2 a, float scale) => new(a.X * scale, a.Y * scale);

    public static Vector2 operator *(float scale, Vector2 a) => new(a.X * scale, a.Y * scale);

    public static Vector2 operator /(Vector2 a, float divider) => new(a.X / divider, a.Y / divider);

    public static bool operator ==(Vector2 a, Vector2 b) => a.Equals(b);

    public static bool operator !=(Vector2 a, Vector2 b) => !a.Equals(b);

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

    public Vector3(float value)
    {
        X = value;
        Y = value;
        Z = value;
    }

    public static Vector3 Zero => new(0f, 0f, 0f);

    public static Vector3 One => new(1f, 1f, 1f);

    public static Vector3 UnitX => new(1f, 0f, 0f);

    public static Vector3 UnitY => new(0f, 1f, 0f);

    public static Vector3 UnitZ => new(0f, 0f, 1f);

    public static Vector3 Up => new(0f, 1f, 0f);

    public static Vector3 Down => new(0f, -1f, 0f);

    public static Vector3 Right => new(1f, 0f, 0f);

    public static Vector3 Left => new(-1f, 0f, 0f);

    public static Vector3 Forward => new(0f, 0f, -1f);

    public static Vector3 Backward => new(0f, 0f, 1f);

    public readonly float LengthSquared() => (X * X) + (Y * Y) + (Z * Z);

    public readonly float Length() => MathF.Sqrt(LengthSquared());

    public static float Distance(Vector3 value1, Vector3 value2)
    {
        float dx = value1.X - value2.X;
        float dy = value1.Y - value2.Y;
        float dz = value1.Z - value2.Z;
        return MathF.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
    }

    public static float Dot(Vector3 value1, Vector3 value2) =>
        (value1.X * value2.X) + (value1.Y * value2.Y) + (value1.Z * value2.Z);

    public static Vector3 Cross(Vector3 value1, Vector3 value2) => new(
        (value1.Y * value2.Z) - (value1.Z * value2.Y),
        (value1.Z * value2.X) - (value1.X * value2.Z),
        (value1.X * value2.Y) - (value1.Y * value2.X));

    public static Vector3 Normalize(Vector3 value)
    {
        float length = value.Length();
        return length == 0f ? Zero : new Vector3(value.X / length, value.Y / length, value.Z / length);
    }

    public static Vector3 Min(Vector3 value1, Vector3 value2) => new(
        MathF.Min(value1.X, value2.X),
        MathF.Min(value1.Y, value2.Y),
        MathF.Min(value1.Z, value2.Z));

    public static Vector3 Max(Vector3 value1, Vector3 value2) => new(
        MathF.Max(value1.X, value2.X),
        MathF.Max(value1.Y, value2.Y),
        MathF.Max(value1.Z, value2.Z));

    public static Vector3 Clamp(Vector3 value1, Vector3 min, Vector3 max) => new(
        Math.Clamp(value1.X, min.X, max.X),
        Math.Clamp(value1.Y, min.Y, max.Y),
        Math.Clamp(value1.Z, min.Z, max.Z));

    public static Vector3 Lerp(Vector3 value1, Vector3 value2, float amount) => new(
        value1.X + ((value2.X - value1.X) * amount),
        value1.Y + ((value2.Y - value1.Y) * amount),
        value1.Z + ((value2.Z - value1.Z) * amount));

    public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vector3 operator -(Vector3 a, Vector3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vector3 operator -(Vector3 a) => new(-a.X, -a.Y, -a.Z);

    public static Vector3 operator *(Vector3 a, float scale) => new(a.X * scale, a.Y * scale, a.Z * scale);

    public static Vector3 operator *(float scale, Vector3 a) => new(a.X * scale, a.Y * scale, a.Z * scale);

    public static Vector3 operator /(Vector3 a, float divider) => new(a.X / divider, a.Y / divider, a.Z / divider);

    public static bool operator ==(Vector3 a, Vector3 b) => a.Equals(b);

    public static bool operator !=(Vector3 a, Vector3 b) => !a.Equals(b);

    public readonly bool Equals(Vector3 other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);

    public override readonly bool Equals(object? obj) => obj is Vector3 other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z);

    public override readonly string ToString() => $"{{X:{X} Y:{Y} Z:{Z}}}";
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

    public Vector4(float value)
    {
        X = value;
        Y = value;
        Z = value;
        W = value;
    }

    public Vector4(Vector2 xy, float z, float w)
    {
        X = xy.X;
        Y = xy.Y;
        Z = z;
        W = w;
    }

    public Vector4(Vector3 xyz, float w)
    {
        X = xyz.X;
        Y = xyz.Y;
        Z = xyz.Z;
        W = w;
    }

    public static Vector4 Zero => new(0f, 0f, 0f, 0f);

    public static Vector4 One => new(1f, 1f, 1f, 1f);

    public static Vector4 UnitX => new(1f, 0f, 0f, 0f);

    public static Vector4 UnitY => new(0f, 1f, 0f, 0f);

    public static Vector4 UnitZ => new(0f, 0f, 1f, 0f);

    public static Vector4 UnitW => new(0f, 0f, 0f, 1f);

    public readonly float LengthSquared() => (X * X) + (Y * Y) + (Z * Z) + (W * W);

    public readonly float Length() => MathF.Sqrt(LengthSquared());

    public static float Dot(Vector4 value1, Vector4 value2) =>
        (value1.X * value2.X) + (value1.Y * value2.Y) + (value1.Z * value2.Z) + (value1.W * value2.W);

    public static Vector4 Normalize(Vector4 value)
    {
        float length = value.Length();
        return length == 0f
            ? Zero
            : new Vector4(value.X / length, value.Y / length, value.Z / length, value.W / length);
    }

    public static Vector4 operator +(Vector4 a, Vector4 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);

    public static Vector4 operator -(Vector4 a, Vector4 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);

    public static Vector4 operator -(Vector4 a) => new(-a.X, -a.Y, -a.Z, -a.W);

    public static Vector4 operator *(Vector4 a, float scale) => new(a.X * scale, a.Y * scale, a.Z * scale, a.W * scale);

    public static Vector4 operator *(float scale, Vector4 a) => new(a.X * scale, a.Y * scale, a.Z * scale, a.W * scale);

    public static Vector4 operator /(Vector4 a, float divider) => new(a.X / divider, a.Y / divider, a.Z / divider, a.W / divider);

    public static bool operator ==(Vector4 a, Vector4 b) => a.Equals(b);

    public static bool operator !=(Vector4 a, Vector4 b) => !a.Equals(b);

    public readonly bool Equals(Vector4 other) =>
        X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);

    public override readonly bool Equals(object? obj) => obj is Vector4 other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z, W);

    public override readonly string ToString() => $"{{X:{X} Y:{Y} Z:{Z} W:{W}}}";
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

    public readonly bool IsEmpty => Width == 0 && Height == 0 && X == 0 && Y == 0;

    public readonly Point Location => new(X, Y);

    public readonly Point Center => new(X + (Width / 2), Y + (Height / 2));

    public readonly bool Contains(int x, int y) => x >= X && x < X + Width && y >= Y && y < Y + Height;

    public readonly bool Contains(Point value) => Contains(value.X, value.Y);

    public readonly void Contains(ref Point value, out bool result) => result = Contains(value.X, value.Y);

    public readonly bool Contains(Rectangle value) =>
        value.Left >= Left && value.Right <= Right && value.Top >= Top && value.Bottom <= Bottom;

    public readonly void Contains(ref Rectangle value, out bool result) => result = Contains(value);

    public readonly bool Intersects(Rectangle value) =>
        value.Left < Right && Left < value.Right && value.Top < Bottom && Top < value.Bottom;

    public readonly void Intersects(ref Rectangle value, out bool result) => result = Intersects(value);

    public static Rectangle Intersect(Rectangle value1, Rectangle value2)
    {
        Intersect(ref value1, ref value2, out Rectangle result);
        return result;
    }

    public static void Intersect(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
    {
        if (value1.Intersects(value2))
        {
            int left = Math.Max(value1.Left, value2.Left);
            int top = Math.Max(value1.Top, value2.Top);
            int right = Math.Min(value1.Right, value2.Right);
            int bottom = Math.Min(value1.Bottom, value2.Bottom);
            result = new Rectangle(left, top, right - left, bottom - top);
        }
        else
        {
            result = Empty;
        }
    }

    public static Rectangle Union(Rectangle value1, Rectangle value2)
    {
        Union(ref value1, ref value2, out Rectangle result);
        return result;
    }

    public static void Union(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
    {
        int left = Math.Min(value1.Left, value2.Left);
        int top = Math.Min(value1.Top, value2.Top);
        int right = Math.Max(value1.Right, value2.Right);
        int bottom = Math.Max(value1.Bottom, value2.Bottom);
        result = new Rectangle(left, top, right - left, bottom - top);
    }

    public void Offset(Point amount) => Offset(amount.X, amount.Y);

    public void Offset(int offsetX, int offsetY)
    {
        X += offsetX;
        Y += offsetY;
    }

    public void Inflate(int horizontalAmount, int verticalAmount)
    {
        X -= horizontalAmount;
        Y -= verticalAmount;
        Width += horizontalAmount * 2;
        Height += verticalAmount * 2;
    }

    public static bool operator ==(Rectangle a, Rectangle b) => a.Equals(b);

    public static bool operator !=(Rectangle a, Rectangle b) => !a.Equals(b);

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
