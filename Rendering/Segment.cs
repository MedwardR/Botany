using System.Numerics;

namespace Botany.Rendering;

public readonly struct Segment(Vector2 start, Vector2 end, int depth)
{
    public Vector2 Start { get; init; } = start;
    public Vector2 End { get; init; } = end;
    public int Depth { get; init; } = depth;
}
