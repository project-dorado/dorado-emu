using Microsoft.Xna.Framework;

namespace Dorado.Tests;

/// <summary>Focused tests for the XNA 3.1 math surface added by Phase 3.</summary>
public sealed class MathTests
{
    private const float Tolerance = 1e-4f;

    [Fact]
    public void IdentityMultiplyLeavesMatrixUnchanged()
    {
        Matrix value = Matrix.CreateScale(2f, 3f, 4f) * Matrix.CreateTranslation(1f, -2f, 3f);

        AssertMatrixNear(value, value * Matrix.Identity);
        AssertMatrixNear(value, Matrix.Identity * value);
    }

    [Fact]
    public void ScaleThenRotationThenTranslationComposeInOrder()
    {
        Matrix world =
            Matrix.CreateScale(2f) *
            Matrix.CreateRotationZ(MathHelper.PiOver2) *
            Matrix.CreateTranslation(1f, 2f, 3f);

        Vector3 transformed = Vector3.Transform(Vector3.UnitX, world);

        AssertNear(1f, transformed.X);
        AssertNear(4f, transformed.Y);
        AssertNear(3f, transformed.Z);
    }

    [Fact]
    public void DeterminantAndInvertRoundTrip()
    {
        Matrix matrix =
            Matrix.CreateScale(2f, 3f, 4f) *
            Matrix.CreateRotationY(0.5f) *
            Matrix.CreateTranslation(5f, -2f, 7f);

        AssertNear(24f, matrix.Determinant());

        Matrix inverse = Matrix.Invert(matrix);
        AssertMatrixNear(Matrix.Identity, matrix * inverse);
    }

    [Fact]
    public void DecomposeRecoversScaleRotationTranslation()
    {
        Matrix matrix =
            Matrix.CreateScale(2f, 3f, 4f) *
            Matrix.CreateRotationY(0.75f) *
            Matrix.CreateTranslation(1f, 2f, 3f);

        Assert.True(matrix.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation));

        AssertVectorNear(new Vector3(2f, 3f, 4f), scale);
        AssertVectorNear(new Vector3(1f, 2f, 3f), translation);

        Matrix rebuilt =
            Matrix.CreateScale(scale) *
            Matrix.CreateFromQuaternion(rotation) *
            Matrix.CreateTranslation(translation);
        AssertMatrixNear(matrix, rebuilt);
    }

    [Fact]
    public void InvertRoundTripsForFullyPopulatedMatrix()
    {
        Matrix matrix = new(
            1.5f, 0.2f, -0.3f, 0.1f,
            0.4f, 2.1f, 0.7f, -0.2f,
            -0.5f, 0.3f, 1.8f, 0.6f,
            0.2f, -0.4f, 0.5f, 1.0f);

        Matrix inverse = Matrix.Invert(matrix);

        AssertMatrixNear(Matrix.Identity, matrix * inverse);
        AssertMatrixNear(Matrix.Identity, inverse * matrix);
    }

    [Fact]
    public void QuaternionAxisAngleMatchesMatrixRotation()
    {
        Quaternion rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathHelper.PiOver2);

        Vector3 byQuaternion = Vector3.Transform(Vector3.UnitX, rotation);
        Vector3 byMatrix = Vector3.Transform(Vector3.UnitX, Matrix.CreateFromQuaternion(rotation));

        AssertNear(0f, byQuaternion.X);
        AssertNear(1f, byQuaternion.Y);
        AssertNear(0f, byQuaternion.Z);
        AssertVectorNear(byQuaternion, byMatrix);
    }

    [Fact]
    public void PlaneDotCoordinateMeasuresSignedDistance()
    {
        Plane plane = new(Vector3.UnitY, -2f);

        AssertNear(3f, plane.DotCoordinate(new Vector3(0f, 5f, 0f)));
        AssertNear(-2f, plane.DotCoordinate(Vector3.Zero));
    }

    [Fact]
    public void BoundingSphereContainsAndIntersects()
    {
        BoundingSphere sphere = new(Vector3.Zero, 2f);

        Assert.Equal(ContainmentType.Contains, sphere.Contains(new Vector3(1f, 0f, 0f)));
        Assert.Equal(ContainmentType.Contains, sphere.Contains(new Vector3(2f, 0f, 0f)));
        Assert.Equal(ContainmentType.Disjoint, sphere.Contains(new Vector3(3f, 0f, 0f)));

        Assert.Equal(ContainmentType.Contains, sphere.Contains(new BoundingSphere(Vector3.Zero, 1f)));
        Assert.Equal(ContainmentType.Intersects, sphere.Contains(new BoundingSphere(new Vector3(2f, 0f, 0f), 1f)));
        Assert.Equal(ContainmentType.Disjoint, sphere.Contains(new BoundingSphere(new Vector3(5f, 0f, 0f), 1f)));

        Assert.True(sphere.Intersects(new BoundingSphere(new Vector3(3f, 0f, 0f), 1.5f)));
        Assert.False(sphere.Intersects(new BoundingSphere(new Vector3(5f, 0f, 0f), 1f)));
    }

    [Fact]
    public void BoundingFrustumContainsPointsInsideAndOutside()
    {
        Matrix projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4,
            4f / 3f,
            1f,
            100f);
        BoundingFrustum frustum = new(projection);

        Assert.Equal(ContainmentType.Contains, frustum.Contains(new Vector3(0f, 0f, -5f)));
        Assert.Equal(ContainmentType.Disjoint, frustum.Contains(new Vector3(0f, 0f, 5f)));
        Assert.Equal(ContainmentType.Disjoint, frustum.Contains(new Vector3(0f, 0f, -200f)));
    }

    private static void AssertNear(float expected, float actual)
    {
        Assert.True(
            MathF.Abs(expected - actual) <= Tolerance,
            $"Expected {expected}, but was {actual}.");
    }

    private static void AssertVectorNear(Vector3 expected, Vector3 actual)
    {
        AssertNear(expected.X, actual.X);
        AssertNear(expected.Y, actual.Y);
        AssertNear(expected.Z, actual.Z);
    }

    private static void AssertMatrixNear(Matrix expected, Matrix actual)
    {
        AssertNear(expected.M11, actual.M11);
        AssertNear(expected.M12, actual.M12);
        AssertNear(expected.M13, actual.M13);
        AssertNear(expected.M14, actual.M14);
        AssertNear(expected.M21, actual.M21);
        AssertNear(expected.M22, actual.M22);
        AssertNear(expected.M23, actual.M23);
        AssertNear(expected.M24, actual.M24);
        AssertNear(expected.M31, actual.M31);
        AssertNear(expected.M32, actual.M32);
        AssertNear(expected.M33, actual.M33);
        AssertNear(expected.M34, actual.M34);
        AssertNear(expected.M41, actual.M41);
        AssertNear(expected.M42, actual.M42);
        AssertNear(expected.M43, actual.M43);
        AssertNear(expected.M44, actual.M44);
    }
}
