namespace Microsoft.Xna.Framework;

/// <summary>A 4x4 matrix using XNA's row-vector convention.</summary>
public struct Matrix : IEquatable<Matrix>
{
    public float M11;
    public float M12;
    public float M13;
    public float M14;
    public float M21;
    public float M22;
    public float M23;
    public float M24;
    public float M31;
    public float M32;
    public float M33;
    public float M34;
    public float M41;
    public float M42;
    public float M43;
    public float M44;

    public Matrix(
        float m11, float m12, float m13, float m14,
        float m21, float m22, float m23, float m24,
        float m31, float m32, float m33, float m34,
        float m41, float m42, float m43, float m44)
    {
        M11 = m11;
        M12 = m12;
        M13 = m13;
        M14 = m14;
        M21 = m21;
        M22 = m22;
        M23 = m23;
        M24 = m24;
        M31 = m31;
        M32 = m32;
        M33 = m33;
        M34 = m34;
        M41 = m41;
        M42 = m42;
        M43 = m43;
        M44 = m44;
    }

    public static Matrix Identity => new(
        1f, 0f, 0f, 0f,
        0f, 1f, 0f, 0f,
        0f, 0f, 1f, 0f,
        0f, 0f, 0f, 1f);

    public readonly Vector3 Backward => new(M31, M32, M33);

    public readonly Vector3 Down => new(-M21, -M22, -M23);

    public readonly Vector3 Forward => new(-M31, -M32, -M33);

    public readonly Vector3 Left => new(-M11, -M12, -M13);

    public readonly Vector3 Right => new(M11, M12, M13);

    public readonly Vector3 Up => new(M21, M22, M23);

    public readonly Vector3 Translation => new(M41, M42, M43);

    public static Matrix CreateBillboard(
        Vector3 objectPosition,
        Vector3 cameraPosition,
        Vector3 cameraUpVector,
        Vector3? cameraForwardVector)
    {
        Vector3 forward = objectPosition - cameraPosition;
        float lengthSquared = forward.LengthSquared();
        if (lengthSquared < 0.0001f)
        {
            forward = cameraForwardVector.HasValue ? -cameraForwardVector.Value : Vector3.Forward;
        }
        else
        {
            forward *= 1f / MathF.Sqrt(lengthSquared);
        }

        Vector3 right = Vector3.Normalize(Vector3.Cross(cameraUpVector, forward));
        Vector3 up = Vector3.Cross(forward, right);

        return new Matrix(
            right.X, right.Y, right.Z, 0f,
            up.X, up.Y, up.Z, 0f,
            forward.X, forward.Y, forward.Z, 0f,
            objectPosition.X, objectPosition.Y, objectPosition.Z, 1f);
    }

    public static void CreateBillboard(
        ref Vector3 objectPosition,
        ref Vector3 cameraPosition,
        ref Vector3 cameraUpVector,
        Vector3? cameraForwardVector,
        out Matrix result) =>
        result = CreateBillboard(objectPosition, cameraPosition, cameraUpVector, cameraForwardVector);

    public static Matrix CreateConstrainedBillboard(
        Vector3 objectPosition,
        Vector3 cameraPosition,
        Vector3 rotateAxis,
        Vector3? cameraForwardVector,
        Vector3? objectForwardVector)
    {
        Vector3 forward = objectPosition - cameraPosition;
        float lengthSquared = forward.LengthSquared();
        if (lengthSquared < 0.0001f)
        {
            forward = cameraForwardVector.HasValue ? -cameraForwardVector.Value : Vector3.Forward;
        }
        else
        {
            forward *= 1f / MathF.Sqrt(lengthSquared);
        }

        Vector3 right = Vector3.Cross(rotateAxis, forward);
        if (right.LengthSquared() < 0.0001f)
        {
            Vector3 candidate = objectForwardVector ?? (MathF.Abs(rotateAxis.Z) > 0.9999f ? Vector3.Right : Vector3.Forward);
            if (objectForwardVector.HasValue && Vector3.Dot(rotateAxis, candidate) < 0f)
            {
                candidate = -candidate;
            }

            right = Vector3.Cross(rotateAxis, candidate);
        }

        right = Vector3.Normalize(right);

        Vector3 up = Vector3.Normalize(Vector3.Cross(rotateAxis, right));
        forward = Vector3.Cross(right, up);

        return new Matrix(
            right.X, right.Y, right.Z, 0f,
            up.X, up.Y, up.Z, 0f,
            forward.X, forward.Y, forward.Z, 0f,
            objectPosition.X, objectPosition.Y, objectPosition.Z, 1f);
    }

    public static void CreateConstrainedBillboard(
        ref Vector3 objectPosition,
        ref Vector3 cameraPosition,
        ref Vector3 rotateAxis,
        Vector3? cameraForwardVector,
        Vector3? objectForwardVector,
        out Matrix result) =>
        result = CreateConstrainedBillboard(
            objectPosition,
            cameraPosition,
            rotateAxis,
            cameraForwardVector,
            objectForwardVector);

    public static Matrix CreateTranslation(Vector3 position) => new(
        1f, 0f, 0f, 0f,
        0f, 1f, 0f, 0f,
        0f, 0f, 1f, 0f,
        position.X, position.Y, position.Z, 1f);

    public static void CreateTranslation(ref Vector3 position, out Matrix result) =>
        result = CreateTranslation(position);

    public static Matrix CreateTranslation(float xPosition, float yPosition, float zPosition) => new(
        1f, 0f, 0f, 0f,
        0f, 1f, 0f, 0f,
        0f, 0f, 1f, 0f,
        xPosition, yPosition, zPosition, 1f);

    public static void CreateTranslation(
        float xPosition,
        float yPosition,
        float zPosition,
        out Matrix result) =>
        result = CreateTranslation(xPosition, yPosition, zPosition);

    public static Matrix CreateScale(float xScale, float yScale, float zScale) => new(
        xScale, 0f, 0f, 0f,
        0f, yScale, 0f, 0f,
        0f, 0f, zScale, 0f,
        0f, 0f, 0f, 1f);

    public static void CreateScale(float xScale, float yScale, float zScale, out Matrix result) =>
        result = CreateScale(xScale, yScale, zScale);

    public static Matrix CreateScale(Vector3 scales) => CreateScale(scales.X, scales.Y, scales.Z);

    public static void CreateScale(ref Vector3 scales, out Matrix result) => result = CreateScale(scales);

    public static Matrix CreateScale(float scale) => CreateScale(scale, scale, scale);

    public static void CreateScale(float scale, out Matrix result) => result = CreateScale(scale);

    public static Matrix CreateRotationX(float radians)
    {
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);

        return new Matrix(
            1f, 0f, 0f, 0f,
            0f, cos, sin, 0f,
            0f, -sin, cos, 0f,
            0f, 0f, 0f, 1f);
    }

    public static void CreateRotationX(float radians, out Matrix result) => result = CreateRotationX(radians);

    public static Matrix CreateRotationY(float radians)
    {
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);

        return new Matrix(
            cos, 0f, -sin, 0f,
            0f, 1f, 0f, 0f,
            sin, 0f, cos, 0f,
            0f, 0f, 0f, 1f);
    }

    public static void CreateRotationY(float radians, out Matrix result) => result = CreateRotationY(radians);

    public static Matrix CreateRotationZ(float radians)
    {
        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);

        return new Matrix(
            cos, sin, 0f, 0f,
            -sin, cos, 0f, 0f,
            0f, 0f, 1f, 0f,
            0f, 0f, 0f, 1f);
    }

    public static void CreateRotationZ(float radians, out Matrix result) => result = CreateRotationZ(radians);

    public static Matrix CreateFromAxisAngle(Vector3 axis, float angle)
    {
        float x = axis.X;
        float y = axis.Y;
        float z = axis.Z;
        float sin = MathF.Sin(angle);
        float cos = MathF.Cos(angle);
        float xx = x * x;
        float yy = y * y;
        float zz = z * z;
        float xy = x * y;
        float xz = x * z;
        float yz = y * z;
        float sx = x * sin;
        float sy = y * sin;
        float sz = z * sin;
        float oneMinusCos = 1f - cos;

        return new Matrix(
            (oneMinusCos * xx) + cos, (oneMinusCos * xy) + sz, (oneMinusCos * xz) - sy, 0f,
            (oneMinusCos * xy) - sz, (oneMinusCos * yy) + cos, (oneMinusCos * yz) + sx, 0f,
            (oneMinusCos * xz) + sy, (oneMinusCos * yz) - sx, (oneMinusCos * zz) + cos, 0f,
            0f, 0f, 0f, 1f);
    }

    public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Matrix result) =>
        result = CreateFromAxisAngle(axis, angle);

    public static Matrix CreatePerspectiveFieldOfView(
        float fieldOfView,
        float aspectRatio,
        float nearPlaneDistance,
        float farPlaneDistance)
    {
        float cotangent = 1f / MathF.Tan(fieldOfView * 0.5f);

        return new Matrix(
            cotangent / aspectRatio, 0f, 0f, 0f,
            0f, cotangent, 0f, 0f,
            0f, 0f, farPlaneDistance / (nearPlaneDistance - farPlaneDistance), -1f,
            0f, 0f, (nearPlaneDistance * farPlaneDistance) / (nearPlaneDistance - farPlaneDistance), 0f);
    }

    public static void CreatePerspectiveFieldOfView(
        float fieldOfView,
        float aspectRatio,
        float nearPlaneDistance,
        float farPlaneDistance,
        out Matrix result) =>
        result = CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance);

    public static Matrix CreatePerspective(
        float width,
        float height,
        float nearPlaneDistance,
        float farPlaneDistance)
    {
        return new Matrix(
            (2f * nearPlaneDistance) / width, 0f, 0f, 0f,
            0f, (2f * nearPlaneDistance) / height, 0f, 0f,
            0f, 0f, farPlaneDistance / (nearPlaneDistance - farPlaneDistance), -1f,
            0f, 0f, (nearPlaneDistance * farPlaneDistance) / (nearPlaneDistance - farPlaneDistance), 0f);
    }

    public static void CreatePerspective(
        float width,
        float height,
        float nearPlaneDistance,
        float farPlaneDistance,
        out Matrix result) =>
        result = CreatePerspective(width, height, nearPlaneDistance, farPlaneDistance);

    public static Matrix CreatePerspectiveOffCenter(
        float left,
        float right,
        float bottom,
        float top,
        float nearPlaneDistance,
        float farPlaneDistance)
    {
        return new Matrix(
            (2f * nearPlaneDistance) / (right - left), 0f, 0f, 0f,
            0f, (2f * nearPlaneDistance) / (top - bottom), 0f, 0f,
            (left + right) / (right - left), (top + bottom) / (top - bottom), farPlaneDistance / (nearPlaneDistance - farPlaneDistance), -1f,
            0f, 0f, (nearPlaneDistance * farPlaneDistance) / (nearPlaneDistance - farPlaneDistance), 0f);
    }

    public static void CreatePerspectiveOffCenter(
        float left,
        float right,
        float bottom,
        float top,
        float nearPlaneDistance,
        float farPlaneDistance,
        out Matrix result) =>
        result = CreatePerspectiveOffCenter(left, right, bottom, top, nearPlaneDistance, farPlaneDistance);

    public static Matrix CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane)
    {
        return new Matrix(
            2f / width, 0f, 0f, 0f,
            0f, 2f / height, 0f, 0f,
            0f, 0f, 1f / (zNearPlane - zFarPlane), 0f,
            0f, 0f, zNearPlane / (zNearPlane - zFarPlane), 1f);
    }

    public static void CreateOrthographic(
        float width,
        float height,
        float zNearPlane,
        float zFarPlane,
        out Matrix result) =>
        result = CreateOrthographic(width, height, zNearPlane, zFarPlane);

    public static Matrix CreateOrthographicOffCenter(
        float left,
        float right,
        float bottom,
        float top,
        float zNearPlane,
        float zFarPlane)
    {
        return new Matrix(
            2f / (right - left), 0f, 0f, 0f,
            0f, 2f / (top - bottom), 0f, 0f,
            0f, 0f, 1f / (zNearPlane - zFarPlane), 0f,
            (left + right) / (left - right), (top + bottom) / (bottom - top), zNearPlane / (zNearPlane - zFarPlane), 1f);
    }

    public static void CreateOrthographicOffCenter(
        float left,
        float right,
        float bottom,
        float top,
        float zNearPlane,
        float zFarPlane,
        out Matrix result) =>
        result = CreateOrthographicOffCenter(left, right, bottom, top, zNearPlane, zFarPlane);

    public static Matrix CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
    {
        Vector3 zaxis = Vector3.Normalize(cameraPosition - cameraTarget);
        Vector3 xaxis = Vector3.Normalize(Vector3.Cross(cameraUpVector, zaxis));
        Vector3 yaxis = Vector3.Cross(zaxis, xaxis);

        return new Matrix(
            xaxis.X, xaxis.Y, xaxis.Z, 0f,
            yaxis.X, yaxis.Y, yaxis.Z, 0f,
            zaxis.X, zaxis.Y, zaxis.Z, 0f,
            -Vector3.Dot(xaxis, cameraPosition), -Vector3.Dot(yaxis, cameraPosition), -Vector3.Dot(zaxis, cameraPosition), 1f);
    }

    public static void CreateLookAt(
        ref Vector3 cameraPosition,
        ref Vector3 cameraTarget,
        ref Vector3 cameraUpVector,
        out Matrix result) =>
        result = CreateLookAt(cameraPosition, cameraTarget, cameraUpVector);

    public static Matrix CreateWorld(Vector3 position, Vector3 forward, Vector3 up)
    {
        Vector3 zaxis = Vector3.Normalize(-forward);
        Vector3 xaxis = Vector3.Normalize(Vector3.Cross(up, zaxis));
        Vector3 yaxis = Vector3.Cross(zaxis, xaxis);

        return new Matrix(
            xaxis.X, xaxis.Y, xaxis.Z, 0f,
            yaxis.X, yaxis.Y, yaxis.Z, 0f,
            zaxis.X, zaxis.Y, zaxis.Z, 0f,
            position.X, position.Y, position.Z, 1f);
    }

    public static void CreateWorld(
        ref Vector3 position,
        ref Vector3 forward,
        ref Vector3 up,
        out Matrix result) =>
        result = CreateWorld(position, forward, up);

    public static Matrix CreateFromQuaternion(Quaternion quaternion)
    {
        float xx = quaternion.X * quaternion.X;
        float yy = quaternion.Y * quaternion.Y;
        float zz = quaternion.Z * quaternion.Z;
        float xy = quaternion.X * quaternion.Y;
        float zw = quaternion.Z * quaternion.W;
        float zx = quaternion.Z * quaternion.X;
        float yw = quaternion.Y * quaternion.W;
        float yz = quaternion.Y * quaternion.Z;
        float xw = quaternion.X * quaternion.W;

        return new Matrix(
            1f - (2f * (yy + zz)), 2f * (xy + zw), 2f * (zx - yw), 0f,
            2f * (xy - zw), 1f - (2f * (zz + xx)), 2f * (yz + xw), 0f,
            2f * (zx + yw), 2f * (yz - xw), 1f - (2f * (yy + xx)), 0f,
            0f, 0f, 0f, 1f);
    }

    public static void CreateFromQuaternion(ref Quaternion quaternion, out Matrix result) =>
        result = CreateFromQuaternion(quaternion);

    public static Matrix CreateFromYawPitchRoll(float yaw, float pitch, float roll)
    {
        Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
        return CreateFromQuaternion(quaternion);
    }

    public static void CreateFromYawPitchRoll(float yaw, float pitch, float roll, out Matrix result) =>
        result = CreateFromYawPitchRoll(yaw, pitch, roll);

    public static Matrix CreateShadow(Vector3 lightDirection, Plane plane)
    {
        Plane normalizedPlane = Plane.Normalize(plane);
        Vector3 normal = normalizedPlane.Normal;
        float dot = Vector3.Dot(normal, lightDirection);
        float x = -normal.X;
        float y = -normal.Y;
        float z = -normal.Z;
        float d = -normalizedPlane.D;

        return new Matrix(
            (x * lightDirection.X) + dot, x * lightDirection.Y, x * lightDirection.Z, 0f,
            y * lightDirection.X, (y * lightDirection.Y) + dot, y * lightDirection.Z, 0f,
            z * lightDirection.X, z * lightDirection.Y, (z * lightDirection.Z) + dot, 0f,
            d * lightDirection.X, d * lightDirection.Y, d * lightDirection.Z, dot);
    }

    public static void CreateShadow(ref Vector3 lightDirection, ref Plane plane, out Matrix result) =>
        result = CreateShadow(lightDirection, plane);

    public static Matrix CreateReflection(Plane value)
    {
        Plane plane = Plane.Normalize(value);
        Vector3 normal = plane.Normal;
        float x = normal.X;
        float y = normal.Y;
        float z = normal.Z;
        float d = plane.D;

        return new Matrix(
            (-2f * x * x) + 1f, -2f * x * y, -2f * x * z, 0f,
            -2f * x * y, (-2f * y * y) + 1f, -2f * y * z, 0f,
            -2f * x * z, -2f * y * z, (-2f * z * z) + 1f, 0f,
            -2f * x * d, -2f * y * d, -2f * z * d, 1f);
    }

    public static void CreateReflection(ref Plane value, out Matrix result) => result = CreateReflection(value);

    public bool Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation)
    {
        translation = new Vector3(M41, M42, M43);

        float sx = MathF.Sqrt((M11 * M11) + (M12 * M12) + (M13 * M13));
        float sy = MathF.Sqrt((M21 * M21) + (M22 * M22) + (M23 * M23));
        float sz = MathF.Sqrt((M31 * M31) + (M32 * M32) + (M33 * M33));

        if (sx == 0f || sy == 0f || sz == 0f)
        {
            scale = new Vector3(sx, sy, sz);
            rotation = Quaternion.Identity;
            return false;
        }

        if (Determinant() < 0f)
        {
            sx = -sx;
        }

        scale = new Vector3(sx, sy, sz);

        Matrix rotationMatrix = new(
            M11 / sx, M12 / sx, M13 / sx, 0f,
            M21 / sy, M22 / sy, M23 / sy, 0f,
            M31 / sz, M32 / sz, M33 / sz, 0f,
            0f, 0f, 0f, 1f);

        rotation = Quaternion.CreateFromRotationMatrix(rotationMatrix);
        return true;
    }

    public static Matrix Transform(Matrix value, Quaternion rotation) =>
        CreateFromQuaternion(rotation) * value;

    public static void Transform(ref Matrix value, ref Quaternion rotation, out Matrix result) =>
        result = Transform(value, rotation);

    public static Matrix Transpose(Matrix matrix) => new(
        matrix.M11, matrix.M21, matrix.M31, matrix.M41,
        matrix.M12, matrix.M22, matrix.M32, matrix.M42,
        matrix.M13, matrix.M23, matrix.M33, matrix.M43,
        matrix.M14, matrix.M24, matrix.M34, matrix.M44);

    public static void Transpose(ref Matrix matrix, out Matrix result) => result = Transpose(matrix);

    public readonly float Determinant()
    {
        float minor11 = (M11 * M22) - (M12 * M21);
        float minor12 = (M11 * M23) - (M13 * M21);
        float minor13 = (M11 * M24) - (M14 * M21);
        float minor14 = (M12 * M23) - (M13 * M22);
        float minor15 = (M12 * M24) - (M14 * M22);
        float minor16 = (M13 * M24) - (M14 * M23);
        float minor21 = (M31 * M42) - (M32 * M41);
        float minor22 = (M31 * M43) - (M33 * M41);
        float minor23 = (M31 * M44) - (M34 * M41);
        float minor24 = (M32 * M43) - (M33 * M42);
        float minor25 = (M32 * M44) - (M34 * M42);
        float minor26 = (M33 * M44) - (M34 * M43);

        return (minor11 * minor26) - (minor12 * minor25) + (minor13 * minor24) +
               (minor14 * minor23) - (minor15 * minor22) + (minor16 * minor21);
    }

    public static Matrix Invert(Matrix matrix)
    {
        Invert(ref matrix, out Matrix result);
        return result;
    }

    public static void Invert(ref Matrix matrix, out Matrix result)
    {
        float minor11 = (matrix.M11 * matrix.M22) - (matrix.M12 * matrix.M21);
        float minor12 = (matrix.M11 * matrix.M23) - (matrix.M13 * matrix.M21);
        float minor13 = (matrix.M11 * matrix.M24) - (matrix.M14 * matrix.M21);
        float minor14 = (matrix.M12 * matrix.M23) - (matrix.M13 * matrix.M22);
        float minor15 = (matrix.M12 * matrix.M24) - (matrix.M14 * matrix.M22);
        float minor16 = (matrix.M13 * matrix.M24) - (matrix.M14 * matrix.M23);
        float minor21 = (matrix.M31 * matrix.M42) - (matrix.M32 * matrix.M41);
        float minor22 = (matrix.M31 * matrix.M43) - (matrix.M33 * matrix.M41);
        float minor23 = (matrix.M31 * matrix.M44) - (matrix.M34 * matrix.M41);
        float minor24 = (matrix.M32 * matrix.M43) - (matrix.M33 * matrix.M42);
        float minor25 = (matrix.M32 * matrix.M44) - (matrix.M34 * matrix.M42);
        float minor26 = (matrix.M33 * matrix.M44) - (matrix.M34 * matrix.M43);
        float inverseDeterminant = 1f / ((minor11 * minor26) - (minor12 * minor25) + (minor13 * minor24) +
                                         (minor14 * minor23) - (minor15 * minor22) + (minor16 * minor21));

        result.M11 = ((matrix.M22 * minor26) - (matrix.M23 * minor25) + (matrix.M24 * minor24)) * inverseDeterminant;
        result.M12 = ((-matrix.M12 * minor26) + (matrix.M13 * minor25) - (matrix.M14 * minor24)) * inverseDeterminant;
        result.M13 = ((matrix.M42 * minor16) - (matrix.M43 * minor15) + (matrix.M44 * minor14)) * inverseDeterminant;
        result.M14 = ((-matrix.M32 * minor16) + (matrix.M33 * minor15) - (matrix.M34 * minor14)) * inverseDeterminant;
        result.M21 = ((-matrix.M21 * minor26) + (matrix.M23 * minor23) - (matrix.M24 * minor22)) * inverseDeterminant;
        result.M22 = ((matrix.M11 * minor26) - (matrix.M13 * minor23) + (matrix.M14 * minor22)) * inverseDeterminant;
        result.M23 = ((-matrix.M41 * minor16) + (matrix.M43 * minor13) - (matrix.M44 * minor12)) * inverseDeterminant;
        result.M24 = ((matrix.M31 * minor16) - (matrix.M33 * minor13) + (matrix.M34 * minor12)) * inverseDeterminant;
        result.M31 = ((matrix.M21 * minor25) - (matrix.M22 * minor23) + (matrix.M24 * minor21)) * inverseDeterminant;
        result.M32 = ((-matrix.M11 * minor25) + (matrix.M12 * minor23) - (matrix.M14 * minor21)) * inverseDeterminant;
        result.M33 = ((matrix.M41 * minor15) - (matrix.M42 * minor13) + (matrix.M44 * minor11)) * inverseDeterminant;
        result.M34 = ((-matrix.M31 * minor15) + (matrix.M32 * minor13) - (matrix.M34 * minor11)) * inverseDeterminant;
        result.M41 = ((-matrix.M21 * minor24) + (matrix.M22 * minor22) - (matrix.M23 * minor21)) * inverseDeterminant;
        result.M42 = ((matrix.M11 * minor24) - (matrix.M12 * minor22) + (matrix.M13 * minor21)) * inverseDeterminant;
        result.M43 = ((-matrix.M41 * minor14) + (matrix.M42 * minor12) - (matrix.M43 * minor11)) * inverseDeterminant;
        result.M44 = ((matrix.M31 * minor14) - (matrix.M32 * minor12) + (matrix.M33 * minor11)) * inverseDeterminant;
    }

    public static Matrix Lerp(Matrix matrix1, Matrix matrix2, float amount) => new(
        matrix1.M11 + ((matrix2.M11 - matrix1.M11) * amount),
        matrix1.M12 + ((matrix2.M12 - matrix1.M12) * amount),
        matrix1.M13 + ((matrix2.M13 - matrix1.M13) * amount),
        matrix1.M14 + ((matrix2.M14 - matrix1.M14) * amount),
        matrix1.M21 + ((matrix2.M21 - matrix1.M21) * amount),
        matrix1.M22 + ((matrix2.M22 - matrix1.M22) * amount),
        matrix1.M23 + ((matrix2.M23 - matrix1.M23) * amount),
        matrix1.M24 + ((matrix2.M24 - matrix1.M24) * amount),
        matrix1.M31 + ((matrix2.M31 - matrix1.M31) * amount),
        matrix1.M32 + ((matrix2.M32 - matrix1.M32) * amount),
        matrix1.M33 + ((matrix2.M33 - matrix1.M33) * amount),
        matrix1.M34 + ((matrix2.M34 - matrix1.M34) * amount),
        matrix1.M41 + ((matrix2.M41 - matrix1.M41) * amount),
        matrix1.M42 + ((matrix2.M42 - matrix1.M42) * amount),
        matrix1.M43 + ((matrix2.M43 - matrix1.M43) * amount),
        matrix1.M44 + ((matrix2.M44 - matrix1.M44) * amount));

    public static void Lerp(ref Matrix matrix1, ref Matrix matrix2, float amount, out Matrix result) =>
        result = Lerp(matrix1, matrix2, amount);

    public static Matrix Negate(Matrix matrix) => new(
        -matrix.M11, -matrix.M12, -matrix.M13, -matrix.M14,
        -matrix.M21, -matrix.M22, -matrix.M23, -matrix.M24,
        -matrix.M31, -matrix.M32, -matrix.M33, -matrix.M34,
        -matrix.M41, -matrix.M42, -matrix.M43, -matrix.M44);

    public static void Negate(ref Matrix matrix, out Matrix result) => result = Negate(matrix);

    public static Matrix Add(Matrix matrix1, Matrix matrix2) => new(
        matrix1.M11 + matrix2.M11, matrix1.M12 + matrix2.M12, matrix1.M13 + matrix2.M13, matrix1.M14 + matrix2.M14,
        matrix1.M21 + matrix2.M21, matrix1.M22 + matrix2.M22, matrix1.M23 + matrix2.M23, matrix1.M24 + matrix2.M24,
        matrix1.M31 + matrix2.M31, matrix1.M32 + matrix2.M32, matrix1.M33 + matrix2.M33, matrix1.M34 + matrix2.M34,
        matrix1.M41 + matrix2.M41, matrix1.M42 + matrix2.M42, matrix1.M43 + matrix2.M43, matrix1.M44 + matrix2.M44);

    public static void Add(ref Matrix matrix1, ref Matrix matrix2, out Matrix result) => result = Add(matrix1, matrix2);

    public static Matrix Subtract(Matrix matrix1, Matrix matrix2) => new(
        matrix1.M11 - matrix2.M11, matrix1.M12 - matrix2.M12, matrix1.M13 - matrix2.M13, matrix1.M14 - matrix2.M14,
        matrix1.M21 - matrix2.M21, matrix1.M22 - matrix2.M22, matrix1.M23 - matrix2.M23, matrix1.M24 - matrix2.M24,
        matrix1.M31 - matrix2.M31, matrix1.M32 - matrix2.M32, matrix1.M33 - matrix2.M33, matrix1.M34 - matrix2.M34,
        matrix1.M41 - matrix2.M41, matrix1.M42 - matrix2.M42, matrix1.M43 - matrix2.M43, matrix1.M44 - matrix2.M44);

    public static void Subtract(ref Matrix matrix1, ref Matrix matrix2, out Matrix result) =>
        result = Subtract(matrix1, matrix2);

    public static Matrix Multiply(Matrix matrix1, Matrix matrix2) => new(
        (matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21) + (matrix1.M13 * matrix2.M31) + (matrix1.M14 * matrix2.M41),
        (matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22) + (matrix1.M13 * matrix2.M32) + (matrix1.M14 * matrix2.M42),
        (matrix1.M11 * matrix2.M13) + (matrix1.M12 * matrix2.M23) + (matrix1.M13 * matrix2.M33) + (matrix1.M14 * matrix2.M43),
        (matrix1.M11 * matrix2.M14) + (matrix1.M12 * matrix2.M24) + (matrix1.M13 * matrix2.M34) + (matrix1.M14 * matrix2.M44),
        (matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21) + (matrix1.M23 * matrix2.M31) + (matrix1.M24 * matrix2.M41),
        (matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22) + (matrix1.M23 * matrix2.M32) + (matrix1.M24 * matrix2.M42),
        (matrix1.M21 * matrix2.M13) + (matrix1.M22 * matrix2.M23) + (matrix1.M23 * matrix2.M33) + (matrix1.M24 * matrix2.M43),
        (matrix1.M21 * matrix2.M14) + (matrix1.M22 * matrix2.M24) + (matrix1.M23 * matrix2.M34) + (matrix1.M24 * matrix2.M44),
        (matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21) + (matrix1.M33 * matrix2.M31) + (matrix1.M34 * matrix2.M41),
        (matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22) + (matrix1.M33 * matrix2.M32) + (matrix1.M34 * matrix2.M42),
        (matrix1.M31 * matrix2.M13) + (matrix1.M32 * matrix2.M23) + (matrix1.M33 * matrix2.M33) + (matrix1.M34 * matrix2.M43),
        (matrix1.M31 * matrix2.M14) + (matrix1.M32 * matrix2.M24) + (matrix1.M33 * matrix2.M34) + (matrix1.M34 * matrix2.M44),
        (matrix1.M41 * matrix2.M11) + (matrix1.M42 * matrix2.M21) + (matrix1.M43 * matrix2.M31) + (matrix1.M44 * matrix2.M41),
        (matrix1.M41 * matrix2.M12) + (matrix1.M42 * matrix2.M22) + (matrix1.M43 * matrix2.M32) + (matrix1.M44 * matrix2.M42),
        (matrix1.M41 * matrix2.M13) + (matrix1.M42 * matrix2.M23) + (matrix1.M43 * matrix2.M33) + (matrix1.M44 * matrix2.M43),
        (matrix1.M41 * matrix2.M14) + (matrix1.M42 * matrix2.M24) + (matrix1.M43 * matrix2.M34) + (matrix1.M44 * matrix2.M44));

    public static void Multiply(ref Matrix matrix1, ref Matrix matrix2, out Matrix result) =>
        result = Multiply(matrix1, matrix2);

    public static Matrix Multiply(Matrix matrix1, float scaleFactor) => new(
        matrix1.M11 * scaleFactor, matrix1.M12 * scaleFactor, matrix1.M13 * scaleFactor, matrix1.M14 * scaleFactor,
        matrix1.M21 * scaleFactor, matrix1.M22 * scaleFactor, matrix1.M23 * scaleFactor, matrix1.M24 * scaleFactor,
        matrix1.M31 * scaleFactor, matrix1.M32 * scaleFactor, matrix1.M33 * scaleFactor, matrix1.M34 * scaleFactor,
        matrix1.M41 * scaleFactor, matrix1.M42 * scaleFactor, matrix1.M43 * scaleFactor, matrix1.M44 * scaleFactor);

    public static void Multiply(ref Matrix matrix1, float scaleFactor, out Matrix result) =>
        result = Multiply(matrix1, scaleFactor);

    public static Matrix Divide(Matrix matrix1, Matrix matrix2) => new(
        matrix1.M11 / matrix2.M11, matrix1.M12 / matrix2.M12, matrix1.M13 / matrix2.M13, matrix1.M14 / matrix2.M14,
        matrix1.M21 / matrix2.M21, matrix1.M22 / matrix2.M22, matrix1.M23 / matrix2.M23, matrix1.M24 / matrix2.M24,
        matrix1.M31 / matrix2.M31, matrix1.M32 / matrix2.M32, matrix1.M33 / matrix2.M33, matrix1.M34 / matrix2.M34,
        matrix1.M41 / matrix2.M41, matrix1.M42 / matrix2.M42, matrix1.M43 / matrix2.M43, matrix1.M44 / matrix2.M44);

    public static void Divide(ref Matrix matrix1, ref Matrix matrix2, out Matrix result) =>
        result = Divide(matrix1, matrix2);

    public static Matrix Divide(Matrix matrix1, float divider) => new(
        matrix1.M11 / divider, matrix1.M12 / divider, matrix1.M13 / divider, matrix1.M14 / divider,
        matrix1.M21 / divider, matrix1.M22 / divider, matrix1.M23 / divider, matrix1.M24 / divider,
        matrix1.M31 / divider, matrix1.M32 / divider, matrix1.M33 / divider, matrix1.M34 / divider,
        matrix1.M41 / divider, matrix1.M42 / divider, matrix1.M43 / divider, matrix1.M44 / divider);

    public static void Divide(ref Matrix matrix1, float divider, out Matrix result) => result = Divide(matrix1, divider);

    public static Matrix operator -(Matrix matrix1) => Negate(matrix1);

    public static Matrix operator +(Matrix matrix1, Matrix matrix2) => Add(matrix1, matrix2);

    public static Matrix operator -(Matrix matrix1, Matrix matrix2) => Subtract(matrix1, matrix2);

    public static Matrix operator *(Matrix matrix1, Matrix matrix2) => Multiply(matrix1, matrix2);

    public static Matrix operator *(Matrix matrix, float scaleFactor) => Multiply(matrix, scaleFactor);

    public static Matrix operator *(float scaleFactor, Matrix matrix) => Multiply(matrix, scaleFactor);

    public static Matrix operator /(Matrix matrix1, Matrix matrix2) => Divide(matrix1, matrix2);

    public static Matrix operator /(Matrix matrix1, float divider) => Divide(matrix1, divider);

    public static bool operator ==(Matrix matrix1, Matrix matrix2) => matrix1.Equals(matrix2);

    public static bool operator !=(Matrix matrix1, Matrix matrix2) => !matrix1.Equals(matrix2);

    public readonly bool Equals(Matrix other) =>
        M11.Equals(other.M11) && M12.Equals(other.M12) && M13.Equals(other.M13) && M14.Equals(other.M14) &&
        M21.Equals(other.M21) && M22.Equals(other.M22) && M23.Equals(other.M23) && M24.Equals(other.M24) &&
        M31.Equals(other.M31) && M32.Equals(other.M32) && M33.Equals(other.M33) && M34.Equals(other.M34) &&
        M41.Equals(other.M41) && M42.Equals(other.M42) && M43.Equals(other.M43) && M44.Equals(other.M44);

    public override readonly bool Equals(object? obj) => obj is Matrix other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(
        HashCode.Combine(M11, M12, M13, M14),
        HashCode.Combine(M21, M22, M23, M24),
        HashCode.Combine(M31, M32, M33, M34),
        HashCode.Combine(M41, M42, M43, M44));

    public override readonly string ToString() =>
        $"{{M11:{M11} M12:{M12} M13:{M13} M14:{M14}}} " +
        $"{{M21:{M21} M22:{M22} M23:{M23} M24:{M24}}} " +
        $"{{M31:{M31} M32:{M32} M33:{M33} M34:{M34}}} " +
        $"{{M41:{M41} M42:{M42} M43:{M43} M44:{M44}}}";
}
