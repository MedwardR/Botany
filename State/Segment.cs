using System.Numerics;

namespace Botany.State;

public readonly struct Segment(Vector2 start, Vector2 end, int depth)
{
    public readonly Vector2 Start = start;
    public readonly Vector2 End = end;
    public readonly int Depth = depth;
}
