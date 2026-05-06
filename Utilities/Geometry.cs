using System.Numerics;

namespace Botany.Utilities;

internal static class Geometry
{
    public static Vector2 GetDirection(float angle)
    {
        float radians = DegToRad(angle);
        float x = MathF.Cos(radians);
        float y = MathF.Sin(radians);
        return new(x, y);
    }

    public static float DegToRad(float angle) => angle * (MathF.PI / 180f);
}
