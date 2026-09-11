namespace Microsoft.Xna.Framework;

/// <summary>A convex view frustum defined by a view-projection matrix.</summary>
public class BoundingFrustum : IEquatable<BoundingFrustum>
{
    public const int CornerCount = 8;

    private readonly Plane[] _planes = new Plane[6];
    private Matrix _matrix;

    public BoundingFrustum(Matrix value)
    {
        Matrix = value;
    }

    public Plane Bottom => _planes[3];

    public Plane Far => _planes[5];

    public Plane Left => _planes[0];

    public Matrix Matrix
    {
        get => _matrix;
        set
        {
            _matrix = value;
            _planes[0] = Plane.Normalize(new Plane(
                value.M14 + value.M11, value.M24 + value.M21, value.M34 + value.M31, value.M44 + value.M41));
            _planes[1] = Plane.Normalize(new Plane(
                value.M14 - value.M11, value.M24 - value.M21, value.M34 - value.M31, value.M44 - value.M41));
            _planes[2] = Plane.Normalize(new Plane(
                value.M14 - value.M12, value.M24 - value.M22, value.M34 - value.M32, value.M44 - value.M42));
            _planes[3] = Plane.Normalize(new Plane(
                value.M14 + value.M12, value.M24 + value.M22, value.M34 + value.M32, value.M44 + value.M42));
            _planes[4] = Plane.Normalize(new Plane(value.M13, value.M23, value.M33, value.M43));
            _planes[5] = Plane.Normalize(new Plane(
                value.M14 - value.M13, value.M24 - value.M23, value.M34 - value.M33, value.M44 - value.M43));
        }
    }

    public Plane Near => _planes[4];

    public Plane Right => _planes[1];

    public Plane Top => _planes[2];

    public Vector3[] GetCorners()
    {
        Vector3[] corners = new Vector3[CornerCount];
        GetCorners(corners);
        return corners;
    }

    public void GetCorners(Vector3[] corners)
    {
        ArgumentNullException.ThrowIfNull(corners);
        if (corners.Length < CornerCount)
        {
            throw new ArgumentOutOfRangeException(nameof(corners), "The destination array must have at least eight elements.");
        }

        Matrix inverse = Matrix.Invert(_matrix);
        corners[0] = Vector3.Transform(new Vector3(-1f, -1f, 0f), inverse);
        corners[1] = Vector3.Transform(new Vector3(1f, -1f, 0f), inverse);
        corners[2] = Vector3.Transform(new Vector3(1f, 1f, 0f), inverse);
        corners[3] = Vector3.Transform(new Vector3(-1f, 1f, 0f), inverse);
        corners[4] = Vector3.Transform(new Vector3(-1f, -1f, 1f), inverse);
        corners[5] = Vector3.Transform(new Vector3(1f, -1f, 1f), inverse);
        corners[6] = Vector3.Transform(new Vector3(1f, 1f, 1f), inverse);
        corners[7] = Vector3.Transform(new Vector3(-1f, 1f, 1f), inverse);
    }

    public ContainmentType Contains(BoundingBox box)
    {
        ContainmentType result = ContainmentType.Contains;

        foreach (Plane plane in _planes)
        {
            Vector3 normal = plane.Normal;
            Vector3 positive = new(
                normal.X >= 0f ? box.Max.X : box.Min.X,
                normal.Y >= 0f ? box.Max.Y : box.Min.Y,
                normal.Z >= 0f ? box.Max.Z : box.Min.Z);
            Vector3 negative = new(
                normal.X >= 0f ? box.Min.X : box.Max.X,
                normal.Y >= 0f ? box.Min.Y : box.Max.Y,
                normal.Z >= 0f ? box.Min.Z : box.Max.Z);

            if (plane.DotCoordinate(positive) < 0f)
            {
                return ContainmentType.Disjoint;
            }

            if (plane.DotCoordinate(negative) < 0f)
            {
                result = ContainmentType.Intersects;
            }
        }

        return result;
    }

    public void Contains(ref BoundingBox box, out ContainmentType result) => result = Contains(box);

    public ContainmentType Contains(BoundingFrustum frustum)
    {
        ArgumentNullException.ThrowIfNull(frustum);

        ContainmentType result = ContainmentType.Disjoint;
        foreach (Vector3 corner in frustum.GetCorners())
        {
            ContainmentType containment = Contains(corner);
            if (containment == ContainmentType.Contains)
            {
                result = ContainmentType.Contains;
            }
            else if (containment == ContainmentType.Intersects)
            {
                return ContainmentType.Intersects;
            }
        }

        return result;
    }

    public ContainmentType Contains(Vector3 point)
    {
        foreach (Plane plane in _planes)
        {
            if (plane.DotCoordinate(point) < 0f)
            {
                return ContainmentType.Disjoint;
            }
        }

        return ContainmentType.Contains;
    }

    public void Contains(ref Vector3 point, out ContainmentType result) => result = Contains(point);

    public ContainmentType Contains(BoundingSphere sphere)
    {
        bool intersects = false;

        foreach (Plane plane in _planes)
        {
            float distance = plane.DotCoordinate(sphere.Center);
            if (distance < -sphere.Radius)
            {
                return ContainmentType.Disjoint;
            }

            if (distance < sphere.Radius)
            {
                intersects = true;
            }
        }

        return intersects ? ContainmentType.Intersects : ContainmentType.Contains;
    }

    public void Contains(ref BoundingSphere sphere, out ContainmentType result) => result = Contains(sphere);

    public bool Intersects(BoundingBox box)
    {
        foreach (Plane plane in _planes)
        {
            Vector3 normal = plane.Normal;
            Vector3 positive = new(
                normal.X >= 0f ? box.Max.X : box.Min.X,
                normal.Y >= 0f ? box.Max.Y : box.Min.Y,
                normal.Z >= 0f ? box.Max.Z : box.Min.Z);

            if (plane.DotCoordinate(positive) < 0f)
            {
                return false;
            }
        }

        return true;
    }

    public void Intersects(ref BoundingBox box, out bool result) => result = Intersects(box);

    public bool Intersects(BoundingFrustum frustum)
    {
        ArgumentNullException.ThrowIfNull(frustum);
        return Contains(frustum) != ContainmentType.Disjoint;
    }

    public PlaneIntersectionType Intersects(Plane plane)
    {
        bool front = false;
        bool back = false;

        Vector3[] corners = GetCorners();
        foreach (Vector3 corner in corners)
        {
            float distance = plane.DotCoordinate(corner);
            if (distance < 0f)
            {
                back = true;
            }
            else
            {
                front = true;
            }

            if (front && back)
            {
                return PlaneIntersectionType.Intersecting;
            }
        }

        return back ? PlaneIntersectionType.Back : PlaneIntersectionType.Front;
    }

    public void Intersects(ref Plane plane, out PlaneIntersectionType result) => result = Intersects(plane);

    public float? Intersects(Ray ray)
    {
        float minimum = 0f;
        float maximum = float.MaxValue;

        foreach (Plane plane in _planes)
        {
            float denominator = Vector3.Dot(plane.Normal, ray.Direction);
            float distance = plane.DotCoordinate(ray.Position);

            if (MathF.Abs(denominator) < 1e-6f)
            {
                if (distance < 0f)
                {
                    return null;
                }
            }
            else
            {
                float intersection = -distance / denominator;
                if (denominator < 0f)
                {
                    if (intersection > minimum)
                    {
                        minimum = intersection;
                    }
                }
                else if (intersection < maximum)
                {
                    maximum = intersection;
                }

                if (minimum > maximum)
                {
                    return null;
                }
            }
        }

        return minimum;
    }

    public void Intersects(ref Ray ray, out float? result) => result = Intersects(ray);

    public bool Intersects(BoundingSphere sphere)
    {
        foreach (Plane plane in _planes)
        {
            if (plane.DotCoordinate(sphere.Center) < -sphere.Radius)
            {
                return false;
            }
        }

        return true;
    }

    public void Intersects(ref BoundingSphere sphere, out bool result) => result = Intersects(sphere);

    public bool Equals(BoundingFrustum? other)
    {
        if (other is null)
        {
            return false;
        }

        if (!_matrix.Equals(other._matrix))
        {
            return false;
        }

        for (int i = 0; i < _planes.Length; i++)
        {
            if (!_planes[i].Equals(other._planes[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is BoundingFrustum other && Equals(other);

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(_matrix);
        foreach (Plane plane in _planes)
        {
            hash.Add(plane);
        }

        return hash.ToHashCode();
    }

    public override string ToString() =>
        $"{{Near:{Near} Far:{Far} Left:{Left} Right:{Right} Top:{Top} Bottom:{Bottom}}}";

    public static bool operator ==(BoundingFrustum? a, BoundingFrustum? b)
    {
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(BoundingFrustum? a, BoundingFrustum? b) => !(a == b);
}
