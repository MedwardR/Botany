using Botany.Rendering;
using Botany.Utilities;
using System.Collections;
using System.Numerics;

namespace Botany.Rendering;

internal class Turtle(string instructions, Random? random = null) : IEnumerator
{
    private readonly Random _random = random ?? Random.Shared;

    private readonly List<Segment> _segments = [];
    private readonly Stack<(Vector2 Position, float Angle)> _stack = [];

    public string Instructions { get; } = instructions;
    public Vector2 Position { get; private set; } = Vector2.Zero;
    public float Angle { get; private set; } = 0f;
    public IReadOnlyList<Segment> Segments => _segments;

    public float Step { get; set; } = 5f;
    public float Turn { get; set; } = 25f;
    public float Entropy { get; set; } = 0f;

    public int Index { get; private set; } = 0;
    public int Depth { get; private set; } = 0;

    object IEnumerator.Current => Instructions[Index];

    public bool MoveNext()
    {
        if (Index < Instructions.Length)
        {
            char c = Instructions[Index];
            float r;

            if (Entropy > 0f)
            {
                r = (_random.NextSingle() * 2f - 1f) * Entropy;
            }
            else r = 1f;

            if (c == 'F')
            {
                var direction = Trigonometry.GetDirection(Angle);
                var destination = Position + direction * (Step + r);

                var segment = new Segment(Position, destination, Depth);
                _segments.Add(segment);

                Position = destination;
            }
            else if (c == '+')
            {
                Angle += Turn + r;
            }
            else if (c == '-')
            {
                Angle -= Turn + r;
            }
            else if (c == '[')
            {
                _stack.Push((Position, Angle));
                Depth++;
            }
            else if (c == ']')
            {
                (Position, Angle) = _stack.Pop();
                Depth--;
            }
            Index++;

            return true;
        }
        else return false;
    }

    public IEnumerable<Segment> Enumerate()
    {
        int index = 0;
        do
        {
            while (index < _segments.Count)
            {
                yield return _segments[index++];
            }
        }
        while (MoveNext());
    }

    public void Reset()
    {
        _segments.Clear();
        _stack.Clear();

        Position = Vector2.Zero;
        Angle = 0f;
        Index = 0;
        Depth = 0;
    }
}
