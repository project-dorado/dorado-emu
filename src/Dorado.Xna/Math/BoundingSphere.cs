namespace Microsoft.Xna.Framework;

/// <summary>A sphere defined by a center and radius.</summary>
public struct BoundingSphere : IEquatable<BoundingSphere>
{
    public Vector3 Center;
    public float Radius;

    public BoundingSphere(Vector3 center, float radius)
    {
        Center = center;
        Radius = radius;
    }

    public static BoundingSphere CreateMerged(BoundingSphere original, BoundingSphere additional)
    {
        CreateMerged(ref original, ref additional, out BoundingSphere result);
        return result;
    }

    public static void CreateMerged(
        ref BoundingSphere original,
        ref BoundingSphere additional,
        out BoundingSphere result)
    {
        Vector3 difference = additional.Center - original.Center;
        float distance = difference.Length();

        if (distance + original.Radius <= additional.Radius)
        {
            result = additional;
            return;
        }

        if (distance + additional.Radius <= original.Radius)
        {
            result = original;
            return;
        }

        float radius = (distance + original.Radius + additional.Radius) * 0.5f;
        Vector3 center = original.Center + (difference * ((radius - original.Radius) / distance));
        result = new BoundingSphere(center, radius);
    }

    public static BoundingSphere CreateFromBoundingBox(BoundingBox box)
    {
        CreateFromBoundingBox(ref box, out BoundingSphere result);
        return result;
    }

    public static void CreateFromBoundingBox(ref BoundingBox box, out BoundingSphere result)
    {
        Vector3 center = (box.Min + box.Max) * 0.5f;
        float radius = (box.Max - box.Min).Length() * 0.5f;
        result = new BoundingSphere(center, radius);
    }

    public static BoundingSphere CreateFromPoints(IEnumerable<Vector3> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        List<Vector3> list = new(points);
        if (list.Count == 0)
        {
            return new BoundingSphere(Vector3.Zero, 0f);
        }

        Vector3 minX = list[0];
        Vector3 maxX = list[0];
        Vector3 minY = list[0];
        Vector3 maxY = list[0];
        Vector3 minZ = list[0];
        Vector3 maxZ = list[0];

        foreach (Vector3 point in list)
        {
            if (point.X < minX.X)
            {
                minX = point;
            }

            if (point.X > maxX.X)
            {
                maxX = point;
            }

            if (point.Y < minY.Y)
            {
                minY = point;
            }

            if (point.Y > maxY.Y)
            {
                maxY = point;
            }

            if (point.Z < minZ.Z)
            {
                minZ = point;
            }

            if (point.Z > maxZ.Z)
            {
                maxZ = point;
            }
        }

        (Vector3 first, Vector3 second) = (minX, maxX);
        float span = Vector3.DistanceSquared(minX, maxX);
        if (Vector3.DistanceSquared(minY, maxY) > span)
        {
            (first, second) = (minY, maxY);
            span = Vector3.DistanceSquared(minY, maxY);
        }

        if (Vector3.DistanceSquared(minZ, maxZ) > span)
        {
            (first, second) = (minZ, maxZ);
        }

        Vector3 center = (first + second) * 0.5f;
        float radius = Vector3.Distance(first, second) * 0.5f;

        foreach (Vector3 point in list)
        {
            Vector3 difference = point - center;
            float distance = difference.Length();
            if (distance > radius)
            {
                float newRadius = (radius + distance) * 0.5f;
                center += difference * ((distance - newRadius) / distance);
                radius = newRadius;
            }
        }

        return new BoundingSphere(center, radius);
    }

    public static BoundingSphere CreateFromFrustum(BoundingFrustum frustum)
    {
        ArgumentNullException.ThrowIfNull(frustum);
        return CreateFromPoints(frustum.GetCorners());
    }

    public readonly bool Intersects(BoundingBox box)
    {
        Intersects(ref box, out bool result);
        return result;
    }

    public readonly void Intersects(ref BoundingBox box, out bool result)
    {
        Vector3 closest = Vector3.Clamp(Center, box.Min, box.Max);
        result = Vector3.DistanceSquared(Center, closest) <= Radius * Radius;
    }

    public readonly bool Intersects(BoundingFrustum frustum)
    {
        ArgumentNullException.ThrowIfNull(frustum);
        return frustum.Intersects(this);
    }

    public readonly PlaneIntersectionType Intersects(Plane plane)
    {
        Intersects(ref plane, out PlaneIntersectionType result);
        return result;
    }

    public readonly void Intersects(ref Plane plane, out PlaneIntersectionType result)
    {
        float distance = plane.DotCoordinate(Center);
        if (distance > Radius)
        {
            result = PlaneIntersectionType.Front;
        }
        else if (distance < -Radius)
        {
            result = PlaneIntersectionType.Back;
        }
        else
        {
            result = PlaneIntersectionType.Intersecting;
        }
    }

    public readonly float? Intersects(Ray ray)
    {
        Intersects(ref ray, out float? result);
        return result;
    }

    public readonly void Intersects(ref Ray ray, out float? result)
    {
        Vector3 difference = Center - ray.Position;
        float distanceSquared = difference.LengthSquared();
        float radiusSquared = Radius * Radius;

        if (distanceSquared < radiusSquared)
        {
            result = 0f;
            return;
        }

        float dot = Vector3.Dot(difference, ray.Direction);
        if (dot < 0f)
        {
            result = null;
            return;
        }

        float discriminant = (dot * dot) - distanceSquared + radiusSquared;
        result = discriminant < 0f ? null : dot - MathF.Sqrt(discriminant);
    }

    public readonly bool Intersects(BoundingSphere sphere) =>
        Vector3.DistanceSquared(Center, sphere.Center) <= (Radius + sphere.Radius) * (Radius + sphere.Radius);

    public readonly void Intersects(ref BoundingSphere sphere, out bool result) => result = Intersects(sphere);

    public readonly ContainmentType Contains(BoundingBox box)
    {
        Contains(ref box, out ContainmentType result);
        return result;
    }

    public readonly void Contains(ref BoundingBox box, out ContainmentType result)
    {
        BoundingSphere sphere = CreateFromBoundingBox(box);
        result = Contains(sphere);
    }

    public readonly ContainmentType Contains(BoundingFrustum frustum)
    {
        ArgumentNullException.ThrowIfNull(frustum);
        return frustum.Contains(this);
    }

    public readonly ContainmentType Contains(Vector3 point)
    {
        Contains(ref point, out ContainmentType result);
        return result;
    }

    public readonly void Contains(ref Vector3 point, out ContainmentType result) =>
        result = Vector3.DistanceSquared(point, Center) <= Radius * Radius
            ? ContainmentType.Contains
            : ContainmentType.Disjoint;

    public readonly ContainmentType Contains(BoundingSphere sphere)
    {
        Contains(ref sphere, out ContainmentType result);
        return result;
    }

    public readonly void Contains(ref BoundingSphere sphere, out ContainmentType result)
    {
        float distance = Vector3.Distance(Center, sphere.Center);

        if (Radius + sphere.Radius < distance)
        {
            result = ContainmentType.Disjoint;
        }
        else if (Radius - sphere.Radius < distance)
        {
            result = ContainmentType.Intersects;
        }
        else
        {
            result = ContainmentType.Contains;
        }
    }

    public readonly BoundingSphere Transform(Matrix matrix)
    {
        Transform(ref matrix, out BoundingSphere result);
        return result;
    }

    public readonly void Transform(ref Matrix matrix, out BoundingSphere result)
    {
        result.Center = Vector3.Transform(Center, matrix);

        float scaleX = (matrix.M11 * matrix.M11) + (matrix.M12 * matrix.M12) + (matrix.M13 * matrix.M13);
        float scaleY = (matrix.M21 * matrix.M21) + (matrix.M22 * matrix.M22) + (matrix.M23 * matrix.M23);
        float scaleZ = (matrix.M31 * matrix.M31) + (matrix.M32 * matrix.M32) + (matrix.M33 * matrix.M33);
        result.Radius = Radius * MathF.Sqrt(MathF.Max(scaleX, MathF.Max(scaleY, scaleZ)));
    }

    public static bool operator ==(BoundingSphere a, BoundingSphere b) => a.Equals(b);

    public static bool operator !=(BoundingSphere a, BoundingSphere b) => !a.Equals(b);

    public readonly bool Equals(BoundingSphere other) => Center.Equals(other.Center) && Radius.Equals(other.Radius);

    public override readonly bool Equals(object? obj) => obj is BoundingSphere other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(Center, Radius);

    public override readonly string ToString() => $"{{Center:{Center} Radius:{Radius}}}";
}
