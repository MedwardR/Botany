using System.Numerics;

namespace Botany.Rendering;

internal struct Camera(float x, float y, float zoom = 1f)
{
    public Vector2 Position { get; set; } = new(x, y);
    public float Zoom { get; set; } = zoom;

    public readonly Matrix3x2 ToMatrix(double width, double height)
    {
        float cx = (float)width * 0.5f;
        float cy = (float)height * 0.5f;

        var screen = Matrix3x2.CreateTranslation(cx, cy);
        var flip = Matrix3x2.CreateScale(-1, 1);
        var scale = Matrix3x2.CreateScale(Zoom, Zoom);
        var origin = Matrix3x2.CreateTranslation(-Position.X, -Position.Y);

        return origin * scale * flip * screen;
    }
}
