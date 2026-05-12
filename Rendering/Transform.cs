using Botany.Utilities;
using System.Numerics;

namespace Botany.Rendering;

public readonly struct Transform(float x, float y, float rotation = 0f, float scale = 1f)
{
    public Vector2 Position { get; init; } = new(x, y);
    public float Rotation { get; init; } = rotation;
    public float Scale { get; init; } = scale;

    public readonly Matrix3x2 ToMatrix()
    {
        float radians = Trigonometry.DegToRad(Rotation);

        float cos = MathF.Cos(radians);
        float sin = MathF.Sin(radians);

        float m11 = cos * Scale;
        float m12 = sin * Scale;

        float m21 = -sin * Scale;
        float m22 = cos * Scale;

        float m31 = Position.X;
        float m32 = Position.Y;

        return new Matrix3x2(m11, m12, m21, m22, m31, m32);
    }
}
