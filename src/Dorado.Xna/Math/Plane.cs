namespace Microsoft.Xna.Framework;

/// <summary>A plane defined by a normal and a distance from the origin.</summary>
public struct Plane : IEquatable<Plane>
{
    public Vector3 Normal;
    public float D;

    public Plane(float a, float b, float c, float d)
    {
        Normal = new Vector3(a, b, c);
        D = d;
    }

    public Plane(Vector3 normal, float d)
    {
        Normal = normal;
        D = d;
    }

    public Plane(Vector4 value)
    {
        Normal = new Vector3(value.X, value.Y, value.Z);
        D = value.W;
    }

    public Plane(Vector3 point1, Vector3 point2, Vector3 point3)
    {
        Vector3 normal = Vector3.Cross(point2 - point1, point3 - point1);
        Normal = Vector3.Normalize(normal);
        D = -Vector3.Dot(Normal, point1);
    }

    public void Normalize()
    {
        float magnitude = MathF.Sqrt((Normal.X * Normal.X) + (Normal.Y * Normal.Y) + (Normal.Z * Normal.Z));
        if (magnitude == 0f)
        {
            return;
        }

        float inverse = 1f / magnitude;
        Normal = new Vector3(Normal.X * inverse, Normal.Y * inverse, Normal.Z * inverse);
        D *= inverse;
    }

    public static Plane Normalize(Plane value)
    {
        Normalize(ref value, out Plane result);
        return result;
    }

    public static void Normalize(ref Plane value, out Plane result)
    {
        float magnitude = MathF.Sqrt(
            (value.Normal.X * value.Normal.X) +
            (value.Normal.Y * value.Normal.Y) +
            (value.Normal.Z * value.Normal.Z));

        if (magnitude == 0f)
        {
            result = value;
            return;
        }

        float inverse = 1f / magnitude;
        result.Normal = new Vector3(
            value.Normal.X * inverse,
            value.Normal.Y * inverse,
            value.Normal.Z * inverse);
        result.D = value.D * inverse;
    }

    public static Plane Transform(Plane plane, Matrix matrix)
    {
        Transform(ref plane, ref matrix, out Plane result);
        return result;
    }

    public static void Transform(ref Plane plane, ref Matrix matrix, out Plane result)
    {
        Matrix.Invert(ref matrix, out Matrix inverse);
        result.Normal = new Vector3(
            (plane.Normal.X * inverse.M11) + (plane.Normal.Y * inverse.M21) + (plane.Normal.Z * inverse.M31) + (plane.D * inverse.M41),
            (plane.Normal.X * inverse.M12) + (plane.Normal.Y * inverse.M22) + (plane.Normal.Z * inverse.M32) + (plane.D * inverse.M42),
            (plane.Normal.X * inverse.M13) + (plane.Normal.Y * inverse.M23) + (plane.Normal.Z * inverse.M33) + (plane.D * inverse.M43));
        result.D = (plane.Normal.X * inverse.M14) + (plane.Normal.Y * inverse.M24) + (plane.Normal.Z * inverse.M34) + (plane.D * inverse.M44);
    }

    public static Plane Transform(Plane plane, Quaternion rotation)
    {
        Transform(ref plane, ref rotation, out Plane result);
        return result;
    }

    public static void Transform(ref Plane plane, ref Quaternion rotation, out Plane result) =>
        result = new Plane(Vector3.Transform(plane.Normal, rotation), plane.D);

    public readonly float Dot(Vector4 value) =>
        (Normal.X * value.X) + (Normal.Y * value.Y) + (Normal.Z * value.Z) + (D * value.W);

    public readonly void Dot(ref Vector4 value, out float result) => result = Dot(value);

    public readonly float DotCoordinate(Vector3 value) =>
        (Normal.X * value.X) + (Normal.Y * value.Y) + (Normal.Z * value.Z) + D;

    public readonly void DotCoordinate(ref Vector3 value, out float result) => result = DotCoordinate(value);

    public readonly float DotNormal(Vector3 value) =>
        (Normal.X * value.X) + (Normal.Y * value.Y) + (Normal.Z * value.Z);

    public readonly void DotNormal(ref Vector3 value, out float result) => result = DotNormal(value);

    public readonly PlaneIntersectionType Intersects(BoundingBox box)
    {
        Intersects(ref box, out PlaneIntersectionType result);
        return result;
    }

    public readonly void Intersects(ref BoundingBox box, out PlaneIntersectionType result)
    {
        Vector3 normal = Normal;
        Vector3 positive = new(
            normal.X >= 0f ? box.Max.X : box.Min.X,
            normal.Y >= 0f ? box.Max.Y : box.Min.Y,
            normal.Z >= 0f ? box.Max.Z : box.Min.Z);
        Vector3 negative = new(
            normal.X >= 0f ? box.Min.X : box.Max.X,
            normal.Y >= 0f ? box.Min.Y : box.Max.Y,
            normal.Z >= 0f ? box.Min.Z : box.Max.Z);

        if (DotCoordinate(positive) > 0f)
        {
            result = PlaneIntersectionType.Front;
        }
        else if (DotCoordinate(negative) < 0f)
        {
            result = PlaneIntersectionType.Back;
        }
        else
        {
            result = PlaneIntersectionType.Intersecting;
        }
    }

    public readonly PlaneIntersectionType Intersects(BoundingFrustum frustum) => frustum.Intersects(this);

    public readonly PlaneIntersectionType Intersects(BoundingSphere sphere)
    {
        Intersects(ref sphere, out PlaneIntersectionType result);
        return result;
    }

    public readonly void Intersects(ref BoundingSphere sphere, out PlaneIntersectionType result)
    {
        float distance = DotCoordinate(sphere.Center);
        if (distance > sphere.Radius)
        {
            result = PlaneIntersectionType.Front;
        }
        else if (distance < -sphere.Radius)
        {
            result = PlaneIntersectionType.Back;
        }
        else
        {
            result = PlaneIntersectionType.Intersecting;
        }
    }

    public static bool operator ==(Plane lhs, Plane rhs) => lhs.Equals(rhs);

    public static bool operator !=(Plane lhs, Plane rhs) => !lhs.Equals(rhs);

    public readonly bool Equals(Plane other) => Normal.Equals(other.Normal) && D.Equals(other.D);

    public override readonly bool Equals(object? obj) => obj is Plane other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(Normal, D);

    public override readonly string ToString() => $"{{Normal:{Normal} D:{D}}}";
}
