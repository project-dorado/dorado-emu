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

    public void Normalize()
    {
        float length = Length();
        if (length == 0f)
        {
            X = 0f;
            Y = 0f;
            return;
        }

        float inverse = 1f / length;
        X *= inverse;
        Y *= inverse;
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

    public static void Distance(ref Vector2 value1, ref Vector2 value2, out float result) =>
        result = Distance(value1, value2);

    public static void DistanceSquared(ref Vector2 value1, ref Vector2 value2, out float result) =>
        result = DistanceSquared(value1, value2);

    public static void Dot(ref Vector2 value1, ref Vector2 value2, out float result) => result = Dot(value1, value2);

    public static void Normalize(ref Vector2 value, out Vector2 result) => result = Normalize(value);

    public static Vector2 Reflect(Vector2 vector, Vector2 normal) => vector - (2f * Dot(vector, normal) * normal);

    public static void Reflect(ref Vector2 vector, ref Vector2 normal, out Vector2 result) =>
        result = Reflect(vector, normal);

    public static void Min(ref Vector2 value1, ref Vector2 value2, out Vector2 result) => result = Min(value1, value2);

    public static void Max(ref Vector2 value1, ref Vector2 value2, out Vector2 result) => result = Max(value1, value2);

    public static void Clamp(ref Vector2 value1, ref Vector2 min, ref Vector2 max, out Vector2 result) =>
        result = Clamp(value1, min, max);

    public static void Lerp(ref Vector2 value1, ref Vector2 value2, float amount, out Vector2 result) =>
        result = Lerp(value1, value2, amount);

    public static Vector2 Barycentric(Vector2 value1, Vector2 value2, Vector2 value3, float amount1, float amount2) =>
        new(
            MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2),
            MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2));

    public static void Barycentric(
        ref Vector2 value1,
        ref Vector2 value2,
        ref Vector2 value3,
        float amount1,
        float amount2,
        out Vector2 result) =>
        result = Barycentric(value1, value2, value3, amount1, amount2);

    public static Vector2 SmoothStep(Vector2 value1, Vector2 value2, float amount) =>
        new(
            MathHelper.SmoothStep(value1.X, value2.X, amount),
            MathHelper.SmoothStep(value1.Y, value2.Y, amount));

    public static void SmoothStep(ref Vector2 value1, ref Vector2 value2, float amount, out Vector2 result) =>
        result = SmoothStep(value1, value2, amount);

    public static Vector2 CatmullRom(Vector2 value1, Vector2 value2, Vector2 value3, Vector2 value4, float amount) =>
        new(
            MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount),
            MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount));

    public static void CatmullRom(
        ref Vector2 value1,
        ref Vector2 value2,
        ref Vector2 value3,
        ref Vector2 value4,
        float amount,
        out Vector2 result) =>
        result = CatmullRom(value1, value2, value3, value4, amount);

    public static Vector2 Hermite(
        Vector2 value1,
        Vector2 tangent1,
        Vector2 value2,
        Vector2 tangent2,
        float amount) =>
        new(
            MathHelper.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount),
            MathHelper.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount));

    public static void Hermite(
        ref Vector2 value1,
        ref Vector2 tangent1,
        ref Vector2 value2,
        ref Vector2 tangent2,
        float amount,
        out Vector2 result) =>
        result = Hermite(value1, tangent1, value2, tangent2, amount);

    public static Vector2 Transform(Vector2 position, Matrix matrix)
    {
        float x = (position.X * matrix.M11) + (position.Y * matrix.M21) + matrix.M41;
        float y = (position.X * matrix.M12) + (position.Y * matrix.M22) + matrix.M42;
        float w = (position.X * matrix.M14) + (position.Y * matrix.M24) + matrix.M44;
        if (w != 1f)
        {
            float inverse = 1f / w;
            return new Vector2(x * inverse, y * inverse);
        }

        return new Vector2(x, y);
    }

    public static void Transform(ref Vector2 position, ref Matrix matrix, out Vector2 result) =>
        result = Transform(position, matrix);

    public static Vector2 TransformNormal(Vector2 normal, Matrix matrix) =>
        new(
            (normal.X * matrix.M11) + (normal.Y * matrix.M21),
            (normal.X * matrix.M12) + (normal.Y * matrix.M22));

    public static void TransformNormal(ref Vector2 normal, ref Matrix matrix, out Vector2 result) =>
        result = TransformNormal(normal, matrix);

    public static Vector2 Transform(Vector2 value, Quaternion rotation)
    {
        float x2 = rotation.X + rotation.X;
        float y2 = rotation.Y + rotation.Y;
        float z2 = rotation.Z + rotation.Z;
        float wx = rotation.W * x2;
        float wy = rotation.W * y2;
        float wz = rotation.W * z2;
        float xx = rotation.X * x2;
        float xy = rotation.X * y2;
        float yy = rotation.Y * y2;
        float zz = rotation.Z * z2;

        return new Vector2(
            (value.X * (1f - yy - zz)) + (value.Y * (xy - wz)),
            (value.X * (xy + wz)) + (value.Y * (1f - xx - zz)));
    }

    public static void Transform(ref Vector2 value, ref Quaternion rotation, out Vector2 result) =>
        result = Transform(value, rotation);

    public static void Transform(Vector2[] sourceArray, ref Matrix matrix, Vector2[] destinationArray)
    {
        ArgumentNullException.ThrowIfNull(sourceArray);
        ArgumentNullException.ThrowIfNull(destinationArray);
        if (destinationArray.Length < sourceArray.Length)
        {
            throw new ArgumentException("The destination array is too small.", nameof(destinationArray));
        }

        for (int i = 0; i < sourceArray.Length; i++)
        {
            Transform(ref sourceArray[i], ref matrix, out destinationArray[i]);
        }
    }

    public static void Transform(
        Vector2[] sourceArray,
        int sourceIndex,
        ref Matrix matrix,
        Vector2[] destinationArray,
        int destinationIndex,
        int length)
    {
        ArgumentNullException.ThrowIfNull(sourceArray);
        ArgumentNullException.ThrowIfNull(destinationArray);
        if (sourceIndex < 0 || length < 0 || sourceIndex + length > sourceArray.Length)
        {
            throw new ArgumentException("The source range is outside the array.", nameof(sourceArray));
        }

        if (destinationIndex < 0 || destinationIndex + length > destinationArray.Length)
        {
            throw new ArgumentException("The destination range is outside the array.", nameof(destinationArray));
        }

        for (int i = 0; i < length; i++)
        {
            Transform(ref sourceArray[sourceIndex + i], ref matrix, out destinationArray[destinationIndex + i]);
        }
    }

    public static void TransformNormal(Vector2[] sourceArray, ref Matrix matrix, Vector2[] destinationArray)
    {
        ArgumentNullException.ThrowIfNull(sourceArray);
        ArgumentNullException.ThrowIfNull(destinationArray);
        if (destinationArray.Length < sourceArray.Length)
        {
            throw new ArgumentException("The destination array is too small.", nameof(destinationArray));
        }

        for (int i = 0; i < sourceArray.Length; i++)
        {
            TransformNormal(ref sourceArray[i], ref matrix, out destinationArray[i]);
        }
    }

    public static void TransformNormal(
        Vector2[] sourceArray,
        int sourceIndex,
        ref Matrix matrix,
        Vector2[] destinationArray,
        int destinationIndex,
        int length)
    {
        ArgumentNullException.ThrowIfNull(sourceArray);
        ArgumentNullException.ThrowIfNull(destinationArray);
        if (sourceIndex < 0 || length < 0 || sourceIndex + length > sourceArray.Length)
        {
            throw new ArgumentException("The source range is outside the array.", nameof(sourceArray));
        }

        if (destinationIndex < 0 || destinationIndex + length > destinationArray.Length)
        {
            throw new ArgumentException("The destination range is outside the array.", nameof(destinationArray));
        }

        for (int i = 0; i < length; i++)
        {
            TransformNormal(ref sourceArray[sourceIndex + i], ref matrix, out destinationArray[destinationIndex + i]);
        }
    }

    public static void Transform(Vector2[] sourceArray, ref Quaternion rotation, Vector2[] destinationArray)
    {
        ArgumentNullException.ThrowIfNull(sourceArray);
        ArgumentNullException.ThrowIfNull(destinationArray);
        if (destinationArray.Length < sourceArray.Length)
        {
            throw new ArgumentException("The destination array is too small.", nameof(destinationArray));
        }

        for (int i = 0; i < sourceArray.Length; i++)
        {
            Transform(ref sourceArray[i], ref rotation, out destinationArray[i]);
        }
    }

    public static void Transform(
        Vector2[] sourceArray,
        int sourceIndex,
        ref Quaternion rotation,
        Vector2[] destinationArray,
        int destinationIndex,
        int length)
    {
        ArgumentNullException.ThrowIfNull(sourceArray);
        ArgumentNullException.ThrowIfNull(destinationArray);
        if (sourceIndex < 0 || length < 0 || sourceIndex + length > sourceArray.Length)
        {
            throw new ArgumentException("The source range is outside the array.", nameof(sourceArray));
        }

        if (destinationIndex < 0 || destinationIndex + length > destinationArray.Length)
        {
            throw new ArgumentException("The destination range is outside the array.", nameof(destinationArray));
        }

        for (int i = 0; i < length; i++)
        {
            Transform(ref sourceArray[sourceIndex + i], ref rotation, out destinationArray[destinationIndex + i]);
        }
    }

    public static Vector2 Negate(Vector2 value) => new(-value.X, -value.Y);

    public static void Negate(ref Vector2 value, out Vector2 result) => result = Negate(value);

    public static Vector2 Add(Vector2 value1, Vector2 value2) => new(value1.X + value2.X, value1.Y + value2.Y);

    public static void Add(ref Vector2 value1, ref Vector2 value2, out Vector2 result) => result = Add(value1, value2);

    public static Vector2 Subtract(Vector2 value1, Vector2 value2) => new(value1.X - value2.X, value1.Y - value2.Y);

    public static void Subtract(ref Vector2 value1, ref Vector2 value2, out Vector2 result) =>
        result = Subtract(value1, value2);

    public static Vector2 Multiply(Vector2 value1, Vector2 value2) => new(value1.X * value2.X, value1.Y * value2.Y);

    public static void Multiply(ref Vector2 value1, ref Vector2 value2, out Vector2 result) =>
        result = Multiply(value1, value2);

    public static Vector2 Multiply(Vector2 value1, float scaleFactor) =>
        new(value1.X * scaleFactor, value1.Y * scaleFactor);

    public static void Multiply(ref Vector2 value1, float scaleFactor, out Vector2 result) =>
        result = Multiply(value1, scaleFactor);

    public static Vector2 Divide(Vector2 value1, Vector2 value2) => new(value1.X / value2.X, value1.Y / value2.Y);

    public static void Divide(ref Vector2 value1, ref Vector2 value2, out Vector2 result) =>
        result = Divide(value1, value2);

    public static Vector2 Divide(Vector2 value1, float divider) => new(value1.X / divider, value1.Y / divider);

    public static void Divide(ref Vector2 value1, float divider, out Vector2 result) => result = Divide(value1, divider);

    public static Vector2 operator *(Vector2 value1, Vector2 value2) => Multiply(value1, value2);

    public static Vector2 operator /(Vector2 value1, Vector2 value2) => Divide(value1, value2);
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

    public void Normalize()
    {
        float length = Length();
        if (length == 0f)
        {
            X = 0f;
            Y = 0f;
            Z = 0f;
            return;
        }

        float inverse = 1f / length;
        X *= inverse;
        Y *= inverse;
        Z *= inverse;
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

    public static void Distance(ref Vector3 value1, ref Vector3 value2, out float result) =>
        result = Distance(value1, value2);

    public static float DistanceSquared(Vector3 value1, Vector3 value2)
    {
        float dx = value1.X - value2.X;
        float dy = value1.Y - value2.Y;
        float dz = value1.Z - value2.Z;
        return (dx * dx) + (dy * dy) + (dz * dz);
    }

    public static void DistanceSquared(ref Vector3 value1, ref Vector3 value2, out float result) =>
        result = DistanceSquared(value1, value2);

    public static void Dot(ref Vector3 value1, ref Vector3 value2, out float result) => result = Dot(value1, value2);

    public static void Normalize(ref Vector3 value, out Vector3 result) => result = Normalize(value);

    public static void Cross(ref Vector3 value1, ref Vector3 value2, out Vector3 result) =>
        result = Cross(value1, value2);

    public static Vector3 Reflect(Vector3 vector, Vector3 normal) => vector - (2f * Dot(vector, normal) * normal);

    public static void Reflect(ref Vector3 vector, ref Vector3 normal, out Vector3 result) =>
        result = Reflect(vector, normal);

    public static void Min(ref Vector3 value1, ref Vector3 value2, out Vector3 result) => result = Min(value1, value2);

    public static void Max(ref Vector3 value1, ref Vector3 value2, out Vector3 result) => result = Max(value1, value2);

    public static void Clamp(ref Vector3 value1, ref Vector3 min, ref Vector3 max, out Vector3 result) =>
        result = Clamp(value1, min, max);

    public static void Lerp(ref Vector3 value1, ref Vector3 value2, float amount, out Vector3 result) =>
        result = Lerp(value1, value2, amount);

    public static Vector3 Barycentric(Vector3 value1, Vector3 value2, Vector3 value3, float amount1, float amount2) =>
        new(
            MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2),
            MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2),
            MathHelper.Barycentric(value1.Z, value2.Z, value3.Z, amount1, amount2));

    public static void Barycentric(
        ref Vector3 value1,
        ref Vector3 value2,
        ref Vector3 value3,
        float amount1,
        float amount2,
        out Vector3 result) =>
        result = Barycentric(value1, value2, value3, amount1, amount2);

    public static Vector3 SmoothStep(Vector3 value1, Vector3 value2, float amount) =>
        new(
            MathHelper.SmoothStep(value1.X, value2.X, amount),
            MathHelper.SmoothStep(value1.Y, value2.Y, amount),
            MathHelper.SmoothStep(value1.Z, value2.Z, amount));

    public static void SmoothStep(ref Vector3 value1, ref Vector3 value2, float amount, out Vector3 result) =>
        result = SmoothStep(value1, value2, amount);

    public static Vector3 CatmullRom(Vector3 value1, Vector3 value2, Vector3 value3, Vector3 value4, float amount) =>
        new(
            MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount),
            MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount),
            MathHelper.CatmullRom(value1.Z, value2.Z, value3.Z, value4.Z, amount));

    public static void CatmullRom(
        ref Vector3 value1,
        ref Vector3 value2,
        ref Vector3 value3,
        ref Vector3 value4,
        float amount,
        out Vector3 result) =>
        result = CatmullRom(value1, value2, value3, value4, amount);

    public static Vector3 Hermite(
        Vector3 value1,
        Vector3 tangent1,
        Vector3 value2,
        Vector3 tangent2,
        float amount) =>
        new(
            MathHelper.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount),
            MathHelper.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount),
            MathHelper.Hermite(value1.Z, tangent1.Z, value2.Z, tangent2.Z, amount));

    public static void Hermite(
        ref Vector3 value1,
        ref Vector3 tangent1,
        ref Vector3 value2,
        ref Vector3 tangent2,
        float amount,
        out Vector3 result) =>
        result = Hermite(value1, tangent1, value2, tangent2, amount);

    public static Vector3 Transform(Vector3 position, Matrix matrix)
    {
        float x = (position.X * matrix.M11) + (position.Y * matrix.M21) + (position.Z * matrix.M31) + matrix.M41;
        float y = (position.X * matrix.M12) + (position.Y * matrix.M22) + (position.Z * matrix.M32) + matrix.M42;
        float z = (position.X * matrix.M13) + (position.Y * matrix.M23) + (position.Z * matrix.M33) + matrix.M43;
        float w = (position.X * matrix.M14) + (position.Y * matrix.M24) + (position.Z * matrix.M34) + matrix.M44;
        if (w != 1f)
        {
            float inverse = 1f / w;
            return new Vector3(x * inverse, y * inverse, z * inverse);
        }

        return new Vector3(x, y, z);
    }

    public static void Transform(ref Vector3 position, ref Matrix matrix, out Vector3 result) =>
        result = Transform(position, matrix);

    public static Vector3 TransformNormal(Vector3 normal, Matrix matrix) =>
        new(
            (normal.X * matrix.M11) + (normal.Y * matrix.M21) + (normal.Z * matrix.M31),
            (normal.X * matrix.M12) + (normal.Y * matrix.M22) + (normal.Z * matrix.M32),
            (normal.X * matrix.M13) + (normal.Y * matrix.M23) + (normal.Z * matrix.M33));

    public static void TransformNormal(ref Vector3 normal, ref Matrix matrix, out Vector3 result) =>
        result = TransformNormal(normal, matrix);

    public static Vector3 Transform(Vector3 value, Quaternion rotation)
    {
        float x2 = rotation.X + rotation.X;
        float y2 = rotation.Y + rotation.Y;
        float z2 = rotation.Z + rotation.Z;
        float wx = rotation.W * x2;
        float wy = rotation.W * y2;
        float wz = rotation.W * z2;
        float xx = rotation.X * x2;
        float xy = rotation.X * y2;
        float xz = rotation.X * z2;
        float yy = rotation.Y * y2;
        float yz = rotation.Y * z2;
        float zz = rotation.Z * z2;

        return new Vector3(
            (value.X * (1f - yy - zz)) + (value.Y * (xy - wz)) + (value.Z * (xz + wy)),
            (value.X * (xy + wz)) + (value.Y * (1f - xx - zz)) + (value.Z * (yz - wx)),
            (value.X * (xz - wy)) + (value.Y * (yz + wx)) + (value.Z * (1f - xx - yy)));
    }

    public static void Transform(ref Vector3 value, ref Quaternion rotation, out Vector3 result) =>
        result = Transform(value, rotation);

    public static void Transform(Vector3[] sourceArray, ref Matrix matrix, Vector3[] destinationArray)
    {
        ArgumentNullException.ThrowIfNull(sourceArray);
        ArgumentNullException.ThrowIfNull(destinationArray);
        if (destinationArray.Length < sourceArray.Length)
        {
            throw new ArgumentException("The destination array is too small.", nameof(destinationArray));
        }

        for (int i = 0; i < sourceArray.Length; i++)
        {
            Transform(ref sourceArray[i], ref matrix, out destinationArray[i]);
        }
    }

    public static void Transform(
        Vector3[] sourceArray,
        int sourceIndex,
        ref Matrix matrix,
        Vector3[] destinationArray,
        int destinationIndex,
        int length)
    {
        ArgumentNullException.ThrowIfNull(sourceArray);
        ArgumentNullException.ThrowIfNull(destinationArray);
        if (sourceIndex < 0 || length < 0 || sourceIndex + length > sourceArray.Length)
        {
            throw new ArgumentException("The source range is outside the array.", nameof(sourceArray));
        }

        if (destinationIndex < 0 || destinationIndex + length > destinationArray.Length)
        {
            throw new ArgumentException("The destination range is outside the array.", nameof(destinationArray));
        }

        for (int i = 0; i < length; i++)
        {
            Transform(ref sourceArray[sourceIndex + i], ref matrix, out destinationArray[destinationIndex + i]);
        }
    }

    public static void TransformNormal(Vector3[] sourceArray, ref Matrix matrix, Vector3[] destinationArray)
    {
        ArgumentNullException.ThrowIfNull(sourceArray);
        ArgumentNullException.ThrowIfNull(destinationArray);
        if (destinationArray.Length < sourceArray.Length)
        {
            throw new ArgumentException("The destination array is too small.", nameof(destinationArray));
        }

        for (int i = 0; i < sourceArray.Length; i++)
        {
            TransformNormal(ref sourceArray[i], ref matrix, out destinationArray[i]);
        }
    }

    public static void TransformNormal(
        Vector3[] sourceArray,
        int sourceIndex,
        ref Matrix matrix,
        Vector3[] destinationArray,
        int destinationIndex,
        int length)
    {
        ArgumentNullException.ThrowIfNull(sourceArray);
        ArgumentNullException.ThrowIfNull(destinationArray);
        if (sourceIndex < 0 || length < 0 || sourceIndex + length > sourceArray.Length)
        {
            throw new ArgumentException("The source range is outside the array.", nameof(sourceArray));
        }

        if (destinationIndex < 0 || destinationIndex + length > destinationArray.Length)
        {
            throw new ArgumentException("The destination range is outside the array.", nameof(destinationArray));
        }

        for (int i = 0; i < length; i++)
        {
            TransformNormal(ref sourceArray[sourceIndex + i], ref matrix, out destinationArray[destinationIndex + i]);
        }
    }

    public static void Transform(Vector3[] sourceArray, ref Quaternion rotation, Vector3[] destinationArray)
    {
        ArgumentNullException.ThrowIfNull(sourceArray);
        ArgumentNullException.ThrowIfNull(destinationArray);
        if (destinationArray.Length < sourceArray.Length)
        {
            throw new ArgumentException("The destination array is too small.", nameof(destinationArray));
        }

        for (int i = 0; i < sourceArray.Length; i++)
        {
            Transform(ref sourceArray[i], ref rotation, out destinationArray[i]);
        }
    }

    public static void Transform(
        Vector3[] sourceArray,
        int sourceIndex,
        ref Quaternion rotation,
        Vector3[] destinationArray,
        int destinationIndex,
        int length)
    {
        ArgumentNullException.ThrowIfNull(sourceArray);
        ArgumentNullException.ThrowIfNull(destinationArray);
        if (sourceIndex < 0 || length < 0 || sourceIndex + length > sourceArray.Length)
        {
            throw new ArgumentException("The source range is outside the array.", nameof(sourceArray));
        }

        if (destinationIndex < 0 || destinationIndex + length > destinationArray.Length)
        {
            throw new ArgumentException("The destination range is outside the array.", nameof(destinationArray));
        }

        for (int i = 0; i < length; i++)
        {
            Transform(ref sourceArray[sourceIndex + i], ref rotation, out destinationArray[destinationIndex + i]);
        }
    }

    public static Vector3 Negate(Vector3 value) => new(-value.X, -value.Y, -value.Z);

    public static void Negate(ref Vector3 value, out Vector3 result) => result = Negate(value);

    public static Vector3 Add(Vector3 value1, Vector3 value2) =>
        new(value1.X + value2.X, value1.Y + value2.Y, value1.Z + value2.Z);

    public static void Add(ref Vector3 value1, ref Vector3 value2, out Vector3 result) => result = Add(value1, value2);

    public static Vector3 Subtract(Vector3 value1, Vector3 value2) =>
        new(value1.X - value2.X, value1.Y - value2.Y, value1.Z - value2.Z);

    public static void Subtract(ref Vector3 value1, ref Vector3 value2, out Vector3 result) =>
        result = Subtract(value1, value2);

    public static Vector3 Multiply(Vector3 value1, Vector3 value2) =>
        new(value1.X * value2.X, value1.Y * value2.Y, value1.Z * value2.Z);

    public static void Multiply(ref Vector3 value1, ref Vector3 value2, out Vector3 result) =>
        result = Multiply(value1, value2);

    public static Vector3 Multiply(Vector3 value1, float scaleFactor) =>
        new(value1.X * scaleFactor, value1.Y * scaleFactor, value1.Z * scaleFactor);

    public static void Multiply(ref Vector3 value1, float scaleFactor, out Vector3 result) =>
        result = Multiply(value1, scaleFactor);

    public static Vector3 Divide(Vector3 value1, Vector3 value2) =>
        new(value1.X / value2.X, value1.Y / value2.Y, value1.Z / value2.Z);

    public static void Divide(ref Vector3 value1, ref Vector3 value2, out Vector3 result) =>
        result = Divide(value1, value2);

    public static Vector3 Divide(Vector3 value1, float divider) =>
        new(value1.X / divider, value1.Y / divider, value1.Z / divider);

    public static void Divide(ref Vector3 value1, float divider, out Vector3 result) => result = Divide(value1, divider);

    public static Vector3 operator *(Vector3 value1, Vector3 value2) => Multiply(value1, value2);

    public static Vector3 operator /(Vector3 value1, Vector3 value2) => Divide(value1, value2);
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

    public void Normalize()
    {
        float length = Length();
        if (length == 0f)
        {
            X = 0f;
            Y = 0f;
            Z = 0f;
            W = 0f;
            return;
        }

        float inverse = 1f / length;
        X *= inverse;
        Y *= inverse;
        Z *= inverse;
        W *= inverse;
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

    public static float Distance(Vector4 value1, Vector4 value2)
    {
        float dx = value1.X - value2.X;
        float dy = value1.Y - value2.Y;
        float dz = value1.Z - value2.Z;
        float dw = value1.W - value2.W;
        return MathF.Sqrt((dx * dx) + (dy * dy) + (dz * dz) + (dw * dw));
    }

    public static void Distance(ref Vector4 value1, ref Vector4 value2, out float result) =>
        result = Distance(value1, value2);

    public static float DistanceSquared(Vector4 value1, Vector4 value2)
    {
        float dx = value1.X - value2.X;
        float dy = value1.Y - value2.Y;
        float dz = value1.Z - value2.Z;
        float dw = value1.W - value2.W;
        return (dx * dx) + (dy * dy) + (dz * dz) + (dw * dw);
    }

    public static void DistanceSquared(ref Vector4 value1, ref Vector4 value2, out float result) =>
        result = DistanceSquared(value1, value2);

    public static void Dot(ref Vector4 value1, ref Vector4 value2, out float result) => result = Dot(value1, value2);

    public static void Normalize(ref Vector4 value, out Vector4 result) => result = Normalize(value);

    public static Vector4 Min(Vector4 value1, Vector4 value2) => new(
        MathF.Min(value1.X, value2.X),
        MathF.Min(value1.Y, value2.Y),
        MathF.Min(value1.Z, value2.Z),
        MathF.Min(value1.W, value2.W));

    public static void Min(ref Vector4 value1, ref Vector4 value2, out Vector4 result) => result = Min(value1, value2);

    public static Vector4 Max(Vector4 value1, Vector4 value2) => new(
        MathF.Max(value1.X, value2.X),
        MathF.Max(value1.Y, value2.Y),
        MathF.Max(value1.Z, value2.Z),
        MathF.Max(value1.W, value2.W));

    public static void Max(ref Vector4 value1, ref Vector4 value2, out Vector4 result) => result = Max(value1, value2);

    public static Vector4 Clamp(Vector4 value1, Vector4 min, Vector4 max) => new(
        Math.Clamp(value1.X, min.X, max.X),
        Math.Clamp(value1.Y, min.Y, max.Y),
        Math.Clamp(value1.Z, min.Z, max.Z),
        Math.Clamp(value1.W, min.W, max.W));

    public static void Clamp(ref Vector4 value1, ref Vector4 min, ref Vector4 max, out Vector4 result) =>
        result = Clamp(value1, min, max);

    public static Vector4 Lerp(Vector4 value1, Vector4 value2, float amount) => new(
        value1.X + ((value2.X - value1.X) * amount),
        value1.Y + ((value2.Y - value1.Y) * amount),
        value1.Z + ((value2.Z - value1.Z) * amount),
        value1.W + ((value2.W - value1.W) * amount));

    public static void Lerp(ref Vector4 value1, ref Vector4 value2, float amount, out Vector4 result) =>
        result = Lerp(value1, value2, amount);

    public static Vector4 Barycentric(Vector4 value1, Vector4 value2, Vector4 value3, float amount1, float amount2) =>
        new(
            MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2),
            MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2),
            MathHelper.Barycentric(value1.Z, value2.Z, value3.Z, amount1, amount2),
            MathHelper.Barycentric(value1.W, value2.W, value3.W, amount1, amount2));

    public static void Barycentric(
        ref Vector4 value1,
        ref Vector4 value2,
        ref Vector4 value3,
        float amount1,
        float amount2,
        out Vector4 result) =>
        result = Barycentric(value1, value2, value3, amount1, amount2);

    public static Vector4 SmoothStep(Vector4 value1, Vector4 value2, float amount) => new(
        MathHelper.SmoothStep(value1.X, value2.X, amount),
        MathHelper.SmoothStep(value1.Y, value2.Y, amount),
        MathHelper.SmoothStep(value1.Z, value2.Z, amount),
        MathHelper.SmoothStep(value1.W, value2.W, amount));

    public static void SmoothStep(ref Vector4 value1, ref Vector4 value2, float amount, out Vector4 result) =>
        result = SmoothStep(value1, value2, amount);

    public static Vector4 CatmullRom(Vector4 value1, Vector4 value2, Vector4 value3, Vector4 value4, float amount) =>
        new(
            MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount),
            MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount),
            MathHelper.CatmullRom(value1.Z, value2.Z, value3.Z, value4.Z, amount),
            MathHelper.CatmullRom(value1.W, value2.W, value3.W, value4.W, amount));

    public static void CatmullRom(
        ref Vector4 value1,
        ref Vector4 value2,
        ref Vector4 value3,
        ref Vector4 value4,
        float amount,
        out Vector4 result) =>
        result = CatmullRom(value1, value2, value3, value4, amount);

    public static Vector4 Hermite(
        Vector4 value1,
        Vector4 tangent1,
        Vector4 value2,
        Vector4 tangent2,
        float amount) =>
        new(
            MathHelper.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount),
            MathHelper.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount),
            MathHelper.Hermite(value1.Z, tangent1.Z, value2.Z, tangent2.Z, amount),
            MathHelper.Hermite(value1.W, tangent1.W, value2.W, tangent2.W, amount));

    public static void Hermite(
        ref Vector4 value1,
        ref Vector4 tangent1,
        ref Vector4 value2,
        ref Vector4 tangent2,
        float amount,
        out Vector4 result) =>
        result = Hermite(value1, tangent1, value2, tangent2, amount);

    public static Vector4 Transform(Vector2 position, Matrix matrix) => new(
        (position.X * matrix.M11) + (position.Y * matrix.M21) + matrix.M41,
        (position.X * matrix.M12) + (position.Y * matrix.M22) + matrix.M42,
        (position.X * matrix.M13) + (position.Y * matrix.M23) + matrix.M43,
        (position.X * matrix.M14) + (position.Y * matrix.M24) + matrix.M44);

    public static void Transform(ref Vector2 position, ref Matrix matrix, out Vector4 result) =>
        result = Transform(position, matrix);

    public static Vector4 Transform(Vector3 position, Matrix matrix) => new(
        (position.X * matrix.M11) + (position.Y * matrix.M21) + (position.Z * matrix.M31) + matrix.M41,
        (position.X * matrix.M12) + (position.Y * matrix.M22) + (position.Z * matrix.M32) + matrix.M42,
        (position.X * matrix.M13) + (position.Y * matrix.M23) + (position.Z * matrix.M33) + matrix.M43,
        (position.X * matrix.M14) + (position.Y * matrix.M24) + (position.Z * matrix.M34) + matrix.M44);

    public static void Transform(ref Vector3 position, ref Matrix matrix, out Vector4 result) =>
        result = Transform(position, matrix);

    public static Vector4 Transform(Vector4 vector, Matrix matrix) => new(
        (vector.X * matrix.M11) + (vector.Y * matrix.M21) + (vector.Z * matrix.M31) + (vector.W * matrix.M41),
        (vector.X * matrix.M12) + (vector.Y * matrix.M22) + (vector.Z * matrix.M32) + (vector.W * matrix.M42),
        (vector.X * matrix.M13) + (vector.Y * matrix.M23) + (vector.Z * matrix.M33) + (vector.W * matrix.M43),
        (vector.X * matrix.M14) + (vector.Y * matrix.M24) + (vector.Z * matrix.M34) + (vector.W * matrix.M44));

    public static void Transform(ref Vector4 vector, ref Matrix matrix, out Vector4 result) =>
        result = Transform(vector, matrix);

    public static Vector4 Transform(Vector2 value, Quaternion rotation)
    {
        Vector3 rotated = Vector3.Transform(new Vector3(value.X, value.Y, 0f), rotation);
        return new Vector4(rotated, 1f);
    }

    public static void Transform(ref Vector2 value, ref Quaternion rotation, out Vector4 result) =>
        result = Transform(value, rotation);

    public static Vector4 Transform(Vector3 value, Quaternion rotation)
    {
        Vector3 rotated = Vector3.Transform(value, rotation);
        return new Vector4(rotated, 1f);
    }

    public static void Transform(ref Vector3 value, ref Quaternion rotation, out Vector4 result) =>
        result = Transform(value, rotation);

    public static Vector4 Transform(Vector4 value, Quaternion rotation)
    {
        Vector3 rotated = Vector3.Transform(new Vector3(value.X, value.Y, value.Z), rotation);
        return new Vector4(rotated, value.W);
    }

    public static void Transform(ref Vector4 value, ref Quaternion rotation, out Vector4 result) =>
        result = Transform(value, rotation);

    public static void Transform(Vector4[] sourceArray, ref Matrix matrix, Vector4[] destinationArray)
    {
        ArgumentNullException.ThrowIfNull(sourceArray);
        ArgumentNullException.ThrowIfNull(destinationArray);
        if (destinationArray.Length < sourceArray.Length)
        {
            throw new ArgumentException("The destination array is too small.", nameof(destinationArray));
        }

        for (int i = 0; i < sourceArray.Length; i++)
        {
            Transform(ref sourceArray[i], ref matrix, out destinationArray[i]);
        }
    }

    public static void Transform(
        Vector4[] sourceArray,
        int sourceIndex,
        ref Matrix matrix,
        Vector4[] destinationArray,
        int destinationIndex,
        int length)
    {
        ArgumentNullException.ThrowIfNull(sourceArray);
        ArgumentNullException.ThrowIfNull(destinationArray);
        if (sourceIndex < 0 || length < 0 || sourceIndex + length > sourceArray.Length)
        {
            throw new ArgumentException("The source range is outside the array.", nameof(sourceArray));
        }

        if (destinationIndex < 0 || destinationIndex + length > destinationArray.Length)
        {
            throw new ArgumentException("The destination range is outside the array.", nameof(destinationArray));
        }

        for (int i = 0; i < length; i++)
        {
            Transform(ref sourceArray[sourceIndex + i], ref matrix, out destinationArray[destinationIndex + i]);
        }
    }

    public static void Transform(Vector4[] sourceArray, ref Quaternion rotation, Vector4[] destinationArray)
    {
        ArgumentNullException.ThrowIfNull(sourceArray);
        ArgumentNullException.ThrowIfNull(destinationArray);
        if (destinationArray.Length < sourceArray.Length)
        {
            throw new ArgumentException("The destination array is too small.", nameof(destinationArray));
        }

        for (int i = 0; i < sourceArray.Length; i++)
        {
            Transform(ref sourceArray[i], ref rotation, out destinationArray[i]);
        }
    }

    public static void Transform(
        Vector4[] sourceArray,
        int sourceIndex,
        ref Quaternion rotation,
        Vector4[] destinationArray,
        int destinationIndex,
        int length)
    {
        ArgumentNullException.ThrowIfNull(sourceArray);
        ArgumentNullException.ThrowIfNull(destinationArray);
        if (sourceIndex < 0 || length < 0 || sourceIndex + length > sourceArray.Length)
        {
            throw new ArgumentException("The source range is outside the array.", nameof(sourceArray));
        }

        if (destinationIndex < 0 || destinationIndex + length > destinationArray.Length)
        {
            throw new ArgumentException("The destination range is outside the array.", nameof(destinationArray));
        }

        for (int i = 0; i < length; i++)
        {
            Transform(ref sourceArray[sourceIndex + i], ref rotation, out destinationArray[destinationIndex + i]);
        }
    }

    public static Vector4 Negate(Vector4 value) => new(-value.X, -value.Y, -value.Z, -value.W);

    public static void Negate(ref Vector4 value, out Vector4 result) => result = Negate(value);

    public static Vector4 Add(Vector4 value1, Vector4 value2) =>
        new(value1.X + value2.X, value1.Y + value2.Y, value1.Z + value2.Z, value1.W + value2.W);

    public static void Add(ref Vector4 value1, ref Vector4 value2, out Vector4 result) => result = Add(value1, value2);

    public static Vector4 Subtract(Vector4 value1, Vector4 value2) =>
        new(value1.X - value2.X, value1.Y - value2.Y, value1.Z - value2.Z, value1.W - value2.W);

    public static void Subtract(ref Vector4 value1, ref Vector4 value2, out Vector4 result) =>
        result = Subtract(value1, value2);

    public static Vector4 Multiply(Vector4 value1, Vector4 value2) =>
        new(value1.X * value2.X, value1.Y * value2.Y, value1.Z * value2.Z, value1.W * value2.W);

    public static void Multiply(ref Vector4 value1, ref Vector4 value2, out Vector4 result) =>
        result = Multiply(value1, value2);

    public static Vector4 Multiply(Vector4 value1, float scaleFactor) =>
        new(value1.X * scaleFactor, value1.Y * scaleFactor, value1.Z * scaleFactor, value1.W * scaleFactor);

    public static void Multiply(ref Vector4 value1, float scaleFactor, out Vector4 result) =>
        result = Multiply(value1, scaleFactor);

    public static Vector4 Divide(Vector4 value1, Vector4 value2) =>
        new(value1.X / value2.X, value1.Y / value2.Y, value1.Z / value2.Z, value1.W / value2.W);

    public static void Divide(ref Vector4 value1, ref Vector4 value2, out Vector4 result) =>
        result = Divide(value1, value2);

    public static Vector4 Divide(Vector4 value1, float divider) =>
        new(value1.X / divider, value1.Y / divider, value1.Z / divider, value1.W / divider);

    public static void Divide(ref Vector4 value1, float divider, out Vector4 result) => result = Divide(value1, divider);

    public static Vector4 operator *(Vector4 value1, Vector4 value2) => Multiply(value1, value2);

    public static Vector4 operator /(Vector4 value1, Vector4 value2) => Divide(value1, value2);
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

    public Point Location
    {
        get => new(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }

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

    public readonly ContainmentType Contains(Vector3 point) =>
        Min.X <= point.X && point.X <= Max.X &&
        Min.Y <= point.Y && point.Y <= Max.Y &&
        Min.Z <= point.Z && point.Z <= Max.Z
            ? ContainmentType.Contains
            : ContainmentType.Disjoint;

    public readonly void Contains(ref Vector3 point, out ContainmentType result) => result = Contains(point);

    public readonly ContainmentType Contains(BoundingBox box)
    {
        if (Max.X < box.Min.X || Min.X > box.Max.X ||
            Max.Y < box.Min.Y || Min.Y > box.Max.Y ||
            Max.Z < box.Min.Z || Min.Z > box.Max.Z)
        {
            return ContainmentType.Disjoint;
        }

        return box.Min.X >= Min.X && box.Min.Y >= Min.Y && box.Min.Z >= Min.Z &&
               box.Max.X <= Max.X && box.Max.Y <= Max.Y && box.Max.Z <= Max.Z
            ? ContainmentType.Contains
            : ContainmentType.Intersects;
    }

    public readonly void Contains(ref BoundingBox box, out ContainmentType result) => result = Contains(box);

    public readonly float? Intersects(Ray ray)
    {
        Intersects(ref ray, out float? result);
        return result;
    }

    public readonly void Intersects(ref Ray ray, out float? result)
    {
        float minimum = float.MinValue;
        float maximum = float.MaxValue;

        for (int axis = 0; axis < 3; axis++)
        {
            float origin = axis switch { 0 => ray.Position.X, 1 => ray.Position.Y, _ => ray.Position.Z };
            float direction = axis switch { 0 => ray.Direction.X, 1 => ray.Direction.Y, _ => ray.Direction.Z };
            float min = axis switch { 0 => Min.X, 1 => Min.Y, _ => Min.Z };
            float max = axis switch { 0 => Max.X, 1 => Max.Y, _ => Max.Z };

            if (MathF.Abs(direction) < 1e-8f)
            {
                if (origin < min || origin > max)
                {
                    result = null;
                    return;
                }

                continue;
            }

            float inverse = 1f / direction;
            float near = (min - origin) * inverse;
            float far = (max - origin) * inverse;
            if (near > far)
            {
                (near, far) = (far, near);
            }

            minimum = MathF.Max(minimum, near);
            maximum = MathF.Min(maximum, far);
            if (minimum > maximum)
            {
                result = null;
                return;
            }
        }

        if (maximum < 0f)
        {
            result = null;
            return;
        }

        result = minimum < 0f ? 0f : minimum;
    }

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
