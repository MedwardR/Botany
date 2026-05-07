using Botany.Abstractions;
using Botany.Utilities;
using System.Numerics;

namespace Botany.State;

internal class Turtle(float step, float turn, Random? random = null)
{
    public Vector2 Position { get; private set; } = Vector2.Zero;
    public float Angle { get; set; } = -90f;
    public float Step { get; set; } = step;
    public float Turn { get; set; } = turn;
    public float Entropy { get; set; } = 0f;

    private readonly Stack<(Vector2 Position, float Angle)> _stack = [];
    private readonly Random _random = random ?? Random.Shared;

    public IEnumerable<Line> Run(string commands)
    {
        foreach (char c in commands)
        {
            float r;

            if (Entropy > 0f)
            {
                r = (_random.NextSingle() * 2f - 1f) * Entropy;
            }
            else r = 1f;

            if (c == 'F')
            {
                var direction = Geometry.GetDirection(Angle);
                var destination = Position + direction * (Step + r);

                yield return new(Position, destination);

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
            }
            else if (c == ']')
            {
                (Position, Angle) = _stack.Pop();
            }
        }
    }
}
