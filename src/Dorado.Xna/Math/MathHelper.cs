namespace Microsoft.Xna.Framework;

/// <summary>Floating-point constants and interpolation helpers matching XNA 3.1.</summary>
public static class MathHelper
{
    public const float E = 2.71828175f;

    public const float Log10E = 0.4342945f;

    public const float Log2E = 1.442695f;

    public const float Pi = 3.14159274f;

    public const float PiOver2 = 1.57079637f;

    public const float PiOver4 = 0.785398185f;

    public const float TwoPi = 6.28318548f;

    public static float ToRadians(float degrees) => degrees * 0.0174532924f;

    public static float ToDegrees(float radians) => radians * 57.29578f;

    public static float Distance(float value1, float value2) => MathF.Abs(value2 - value1);

    public static float Min(float value1, float value2) => MathF.Min(value1, value2);

    public static float Max(float value1, float value2) => MathF.Max(value1, value2);

    public static float Clamp(float value, float min, float max)
    {
        value = value > max ? max : value;
        return value < min ? min : value;
    }

    public static float Lerp(float value1, float value2, float amount) => value1 + ((value2 - value1) * amount);

    public static float Barycentric(float value1, float value2, float value3, float amount1, float amount2) =>
        (value1 * (1f - amount1 - amount2)) + (value2 * amount1) + (value3 * amount2);

    public static float SmoothStep(float value1, float value2, float amount)
    {
        amount = Clamp(amount, 0f, 1f);
        return Hermite(value1, 0f, value2, 0f, amount);
    }

    public static float CatmullRom(float value1, float value2, float value3, float value4, float amount)
    {
        float squared = amount * amount;
        float cubed = amount * squared;
        return 0.5f * (
            (2f * value2) +
            ((value3 - value1) * amount) +
            (((2f * value1) - (5f * value2) + (4f * value3) - value4) * squared) +
            ((((3f * value2) - value1) - (3f * value3) + value4) * cubed));
    }

    public static float Hermite(float value1, float tangent1, float value2, float tangent2, float amount)
    {
        float squared = amount * amount;
        float cubed = amount * squared;

        if (amount == 0f)
        {
            return value1;
        }

        if (amount == 1f)
        {
            return value2;
        }

        return ((((2f * value1) - (2f * value2) + tangent1 + tangent2) * cubed) +
                (((-3f * value1) + (3f * value2) - (2f * tangent1) - tangent2) * squared) +
                (tangent1 * amount)) +
               value1;
    }

    public static float WrapAngle(float angle)
    {
        angle = MathF.IEEERemainder(angle, TwoPi);
        if (angle <= -Pi)
        {
            angle += TwoPi;
        }
        else if (angle > Pi)
        {
            angle -= TwoPi;
        }

        return angle;
    }
}
