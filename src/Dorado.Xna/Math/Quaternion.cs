namespace Microsoft.Xna.Framework;

/// <summary>A rotation expressed as a four-component quaternion.</summary>
public struct Quaternion : IEquatable<Quaternion>
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public Quaternion(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public Quaternion(Vector3 vectorPart, float scalarPart)
    {
        X = vectorPart.X;
        Y = vectorPart.Y;
        Z = vectorPart.Z;
        W = scalarPart;
    }

    public static Quaternion Identity => new(0f, 0f, 0f, 1f);

    public readonly float LengthSquared() => (X * X) + (Y * Y) + (Z * Z) + (W * W);

    public readonly float Length() => MathF.Sqrt(LengthSquared());

    public void Normalize()
    {
        float lengthSquared = LengthSquared();
        if (lengthSquared == 0f)
        {
            this = Identity;
            return;
        }

        float inverse = 1f / MathF.Sqrt(lengthSquared);
        X *= inverse;
        Y *= inverse;
        Z *= inverse;
        W *= inverse;
    }

    public static Quaternion Normalize(Quaternion quaternion)
    {
        quaternion.Normalize();
        return quaternion;
    }

    public static void Normalize(ref Quaternion quaternion, out Quaternion result)
    {
        result = quaternion;
        result.Normalize();
    }

    public void Conjugate()
    {
        X = -X;
        Y = -Y;
        Z = -Z;
    }

    public static Quaternion Conjugate(Quaternion value) => new(-value.X, -value.Y, -value.Z, value.W);

    public static void Conjugate(ref Quaternion value, out Quaternion result) => result = Conjugate(value);

    public static Quaternion Inverse(Quaternion quaternion)
    {
        float lengthSquared = quaternion.LengthSquared();
        if (lengthSquared == 0f)
        {
            return Identity;
        }

        float inverse = 1f / lengthSquared;
        return new Quaternion(
            -quaternion.X * inverse,
            -quaternion.Y * inverse,
            -quaternion.Z * inverse,
            quaternion.W * inverse);
    }

    public static void Inverse(ref Quaternion quaternion, out Quaternion result) => result = Inverse(quaternion);

    public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
    {
        float halfAngle = angle * 0.5f;
        float sin = MathF.Sin(halfAngle);
        return new Quaternion(axis.X * sin, axis.Y * sin, axis.Z * sin, MathF.Cos(halfAngle));
    }

    public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Quaternion result) =>
        result = CreateFromAxisAngle(axis, angle);

    public static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
    {
        float halfRoll = roll * 0.5f;
        float halfYaw = yaw * 0.5f;
        float halfPitch = pitch * 0.5f;
        float sinPitch = MathF.Sin(halfPitch);
        float cosPitch = MathF.Cos(halfPitch);
        float sinRoll = MathF.Sin(halfRoll);
        float cosRoll = MathF.Cos(halfRoll);
        float sinYaw = MathF.Sin(halfYaw);
        float cosYaw = MathF.Cos(halfYaw);

        return new Quaternion(
            (cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll),
            (sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll),
            (cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll),
            (cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll));
    }

    public static void CreateFromYawPitchRoll(float yaw, float pitch, float roll, out Quaternion result) =>
        result = CreateFromYawPitchRoll(yaw, pitch, roll);

    public static Quaternion CreateFromRotationMatrix(Matrix matrix)
    {
        CreateFromRotationMatrix(ref matrix, out Quaternion result);
        return result;
    }

    public static void CreateFromRotationMatrix(ref Matrix matrix, out Quaternion result)
    {
        float trace = matrix.M11 + matrix.M22 + matrix.M33;
        if (trace > 0f)
        {
            float scale = MathF.Sqrt(trace + 1f);
            result.W = scale * 0.5f;
            scale = 0.5f / scale;
            result.X = (matrix.M23 - matrix.M32) * scale;
            result.Y = (matrix.M31 - matrix.M13) * scale;
            result.Z = (matrix.M12 - matrix.M21) * scale;
        }
        else if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33)
        {
            float scale = MathF.Sqrt(1f + matrix.M11 - matrix.M22 - matrix.M33);
            float inverse = 0.5f / scale;
            result.X = 0.5f * scale;
            result.Y = (matrix.M12 + matrix.M21) * inverse;
            result.Z = (matrix.M13 + matrix.M31) * inverse;
            result.W = (matrix.M23 - matrix.M32) * inverse;
        }
        else if (matrix.M22 > matrix.M33)
        {
            float scale = MathF.Sqrt(1f + matrix.M22 - matrix.M11 - matrix.M33);
            float inverse = 0.5f / scale;
            result.X = (matrix.M12 + matrix.M21) * inverse;
            result.Y = 0.5f * scale;
            result.Z = (matrix.M23 + matrix.M32) * inverse;
            result.W = (matrix.M31 - matrix.M13) * inverse;
        }
        else
        {
            float scale = MathF.Sqrt(1f + matrix.M33 - matrix.M11 - matrix.M22);
            float inverse = 0.5f / scale;
            result.X = (matrix.M13 + matrix.M31) * inverse;
            result.Y = (matrix.M23 + matrix.M32) * inverse;
            result.Z = 0.5f * scale;
            result.W = (matrix.M12 - matrix.M21) * inverse;
        }
    }

    public static float Dot(Quaternion quaternion1, Quaternion quaternion2) =>
        (quaternion1.X * quaternion2.X) +
        (quaternion1.Y * quaternion2.Y) +
        (quaternion1.Z * quaternion2.Z) +
        (quaternion1.W * quaternion2.W);

    public static void Dot(ref Quaternion quaternion1, ref Quaternion quaternion2, out float result) =>
        result = Dot(quaternion1, quaternion2);

    public static Quaternion Slerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
    {
        float dot = Dot(quaternion1, quaternion2);
        bool negate = false;
        if (dot < 0f)
        {
            negate = true;
            dot = -dot;
        }

        float first;
        float second;
        if (dot > 0.999999f)
        {
            first = 1f - amount;
            second = negate ? -amount : amount;
        }
        else
        {
            float angle = MathF.Acos(dot);
            float inverseSin = 1f / MathF.Sin(angle);
            first = MathF.Sin((1f - amount) * angle) * inverseSin;
            second = negate
                ? -MathF.Sin(amount * angle) * inverseSin
                : MathF.Sin(amount * angle) * inverseSin;
        }

        return new Quaternion(
            (first * quaternion1.X) + (second * quaternion2.X),
            (first * quaternion1.Y) + (second * quaternion2.Y),
            (first * quaternion1.Z) + (second * quaternion2.Z),
            (first * quaternion1.W) + (second * quaternion2.W));
    }

    public static void Slerp(ref Quaternion quaternion1, ref Quaternion quaternion2, float amount, out Quaternion result) =>
        result = Slerp(quaternion1, quaternion2, amount);

    public static Quaternion Lerp(Quaternion quaternion1, Quaternion quaternion2, float amount) =>
        new(
            quaternion1.X + ((quaternion2.X - quaternion1.X) * amount),
            quaternion1.Y + ((quaternion2.Y - quaternion1.Y) * amount),
            quaternion1.Z + ((quaternion2.Z - quaternion1.Z) * amount),
            quaternion1.W + ((quaternion2.W - quaternion1.W) * amount));

    public static void Lerp(ref Quaternion quaternion1, ref Quaternion quaternion2, float amount, out Quaternion result) =>
        result = Lerp(quaternion1, quaternion2, amount);

    public static Quaternion Concatenate(Quaternion value1, Quaternion value2) => Multiply(value2, value1);

    public static void Concatenate(ref Quaternion value1, ref Quaternion value2, out Quaternion result) =>
        Multiply(ref value2, ref value1, out result);

    public static Quaternion Negate(Quaternion quaternion) => new(-quaternion.X, -quaternion.Y, -quaternion.Z, -quaternion.W);

    public static void Negate(ref Quaternion quaternion, out Quaternion result) => result = Negate(quaternion);

    public static Quaternion Add(Quaternion quaternion1, Quaternion quaternion2) =>
        new(
            quaternion1.X + quaternion2.X,
            quaternion1.Y + quaternion2.Y,
            quaternion1.Z + quaternion2.Z,
            quaternion1.W + quaternion2.W);

    public static void Add(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result) =>
        result = Add(quaternion1, quaternion2);

    public static Quaternion Subtract(Quaternion quaternion1, Quaternion quaternion2) =>
        new(
            quaternion1.X - quaternion2.X,
            quaternion1.Y - quaternion2.Y,
            quaternion1.Z - quaternion2.Z,
            quaternion1.W - quaternion2.W);

    public static void Subtract(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result) =>
        result = Subtract(quaternion1, quaternion2);

    public static Quaternion Multiply(Quaternion quaternion1, Quaternion quaternion2)
    {
        float x = quaternion2.X;
        float y = quaternion2.Y;
        float z = quaternion2.Z;
        float w = quaternion2.W;
        float num4 = quaternion1.X;
        float num3 = quaternion1.Y;
        float num2 = quaternion1.Z;
        float num = quaternion1.W;

        return new Quaternion(
            (num4 * w) + (num * x) + (num3 * z) - (num2 * y),
            (num3 * w) + (num * y) + (num2 * x) - (num4 * z),
            (num2 * w) + (num * z) + (num4 * y) - (num3 * x),
            (num * w) - (num4 * x) - (num3 * y) - (num2 * z));
    }

    public static void Multiply(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result) =>
        result = Multiply(quaternion1, quaternion2);

    public static Quaternion Multiply(Quaternion quaternion1, float scaleFactor) =>
        new(
            quaternion1.X * scaleFactor,
            quaternion1.Y * scaleFactor,
            quaternion1.Z * scaleFactor,
            quaternion1.W * scaleFactor);

    public static void Multiply(ref Quaternion quaternion1, float scaleFactor, out Quaternion result) =>
        result = Multiply(quaternion1, scaleFactor);

    public static Quaternion Divide(Quaternion quaternion1, Quaternion quaternion2) =>
        new(
            quaternion1.X / quaternion2.X,
            quaternion1.Y / quaternion2.Y,
            quaternion1.Z / quaternion2.Z,
            quaternion1.W / quaternion2.W);

    public static void Divide(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result) =>
        result = Divide(quaternion1, quaternion2);

    public static Quaternion operator -(Quaternion quaternion) => Negate(quaternion);

    public static Quaternion operator +(Quaternion quaternion1, Quaternion quaternion2) => Add(quaternion1, quaternion2);

    public static Quaternion operator -(Quaternion quaternion1, Quaternion quaternion2) => Subtract(quaternion1, quaternion2);

    public static Quaternion operator *(Quaternion quaternion1, Quaternion quaternion2) => Multiply(quaternion1, quaternion2);

    public static Quaternion operator *(Quaternion quaternion1, float scaleFactor) => Multiply(quaternion1, scaleFactor);

    public static Quaternion operator /(Quaternion quaternion1, Quaternion quaternion2) => Divide(quaternion1, quaternion2);

    public static bool operator ==(Quaternion quaternion1, Quaternion quaternion2) => quaternion1.Equals(quaternion2);

    public static bool operator !=(Quaternion quaternion1, Quaternion quaternion2) => !quaternion1.Equals(quaternion2);

    public readonly bool Equals(Quaternion other) =>
        X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);

    public override readonly bool Equals(object? obj) => obj is Quaternion other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(X, Y, Z, W);

    public override readonly string ToString() => $"{{X:{X} Y:{Y} Z:{Z} W:{W}}}";
}
