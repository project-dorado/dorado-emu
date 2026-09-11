namespace Microsoft.Xna.Framework;

/// <summary>A ray defined by an origin and a direction.</summary>
public struct Ray : IEquatable<Ray>
{
    public Vector3 Position;
    public Vector3 Direction;

    public Ray(Vector3 position, Vector3 direction)
    {
        Position = position;
        Direction = direction;
    }

    public readonly float? Intersects(BoundingBox box)
    {
        Intersects(ref box, out float? result);
        return result;
    }

    public readonly void Intersects(ref BoundingBox box, out float? result)
    {
        float minimum = 0f;
        float maximum = float.MaxValue;

        if (!ClipAxis(Position.X, Direction.X, box.Min.X, box.Max.X, ref minimum, ref maximum) ||
            !ClipAxis(Position.Y, Direction.Y, box.Min.Y, box.Max.Y, ref minimum, ref maximum) ||
            !ClipAxis(Position.Z, Direction.Z, box.Min.Z, box.Max.Z, ref minimum, ref maximum))
        {
            result = null;
            return;
        }

        result = minimum;
    }

    public readonly float? Intersects(BoundingFrustum frustum) => frustum.Intersects(this);

    public readonly float? Intersects(Plane plane)
    {
        Intersects(ref plane, out float? result);
        return result;
    }

    public readonly void Intersects(ref Plane plane, out float? result)
    {
        float directionDotNormal =
            (Direction.X * plane.Normal.X) +
            (Direction.Y * plane.Normal.Y) +
            (Direction.Z * plane.Normal.Z);

        if (directionDotNormal == 0f)
        {
            result = null;
            return;
        }

        float positionDotNormal =
            (Position.X * plane.Normal.X) +
            (Position.Y * plane.Normal.Y) +
            (Position.Z * plane.Normal.Z) +
            plane.D;

        float distance = -positionDotNormal / directionDotNormal;
        result = distance < 0f ? null : distance;
    }

    public readonly float? Intersects(BoundingSphere sphere)
    {
        Intersects(ref sphere, out float? result);
        return result;
    }

    public readonly void Intersects(ref BoundingSphere sphere, out float? result)
    {
        Vector3 difference = sphere.Center - Position;
        float distanceSquared = difference.LengthSquared();
        float radiusSquared = sphere.Radius * sphere.Radius;

        if (distanceSquared < radiusSquared)
        {
            result = 0f;
            return;
        }

        float dot = Vector3.Dot(difference, Direction);
        if (dot < 0f)
        {
            result = null;
            return;
        }

        float discriminant = (dot * dot) - distanceSquared + radiusSquared;
        result = discriminant < 0f ? null : dot - MathF.Sqrt(discriminant);
    }

    public static bool operator ==(Ray a, Ray b) => a.Equals(b);

    public static bool operator !=(Ray a, Ray b) => !a.Equals(b);

    public readonly bool Equals(Ray other) => Position.Equals(other.Position) && Direction.Equals(other.Direction);

    public override readonly bool Equals(object? obj) => obj is Ray other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(Position, Direction);

    public override readonly string ToString() => $"{{Position:{Position} Direction:{Direction}}}";

    private static bool ClipAxis(
        float position,
        float direction,
        float minimum,
        float maximum,
        ref float enter,
        ref float exit)
    {
        if (MathF.Abs(direction) < 1e-6f)
        {
            return position >= minimum && position <= maximum;
        }

        float inverse = 1f / direction;
        float near = (minimum - position) * inverse;
        float far = (maximum - position) * inverse;
        if (near > far)
        {
            (near, far) = (far, near);
        }

        enter = MathF.Max(enter, near);
        exit = MathF.Min(exit, far);
        return enter <= exit;
    }
}
