using System.Numerics;
using Botany.Abstractions;
using Botany.Interfaces;

namespace Botany.State;

internal class Plant : IPositionable, IUpdateable, ISerializable<Plant>
{
    private const float COOLDOWN_SECONDS = 86400f; // 24 hours
    private const float MAX_VARIATION = 7200f; // 2 hours

    public Vector2 Position { get; set; }
    public float Speed { get; set; }

    public IReadOnlyList<Line> Lines => _lines;
    public int Length => _length;

    private readonly Seed _seed;
    private readonly Random _random;
    private readonly LSystem _lsys;
    private readonly Turtle _turtle;
    private readonly List<Line> _lines;

    private float _cooldown;
    private int _length;

    public Plant(Seed seed)
    {
        _seed = seed;
        _random = new(seed.Random);

        _lsys = new(seed.Axiom, seed.Rules);
        _turtle = new(seed.Step, seed.Turn, _random)
        {
            Entropy = seed.Entropy,
        };
        string commands = _lsys.Expand(seed.Iterations);

        _lines = [.. _turtle.Run(commands)];
        _cooldown = 0f;
        _length = 0;

        Speed = 1f;
    }

    public void Update(float deltaTime)
    {
        _cooldown -= deltaTime * Speed;

        while (_cooldown <= 0f)
        {
            Grow();

            float period = COOLDOWN_SECONDS;
            float entropy = (_random.NextSingle() * 2f - 1f) * MAX_VARIATION;

            _cooldown += period + entropy;
        }
    }

    public void Grow()
    {
        if (_length < _lines.Count)
        {
            _length++;
        }
    }

    public string Serialize()
    {
        var toml = new Toml
        {
            { "seed", "axiom", _seed.Axiom },
            { "seed", "rules", _seed.Rules },
            { "seed", "iterations", _seed.Iterations },
            { "seed", "step", _seed.Step },
            { "seed", "turn", _seed.Turn },
            { "seed", "entropy", _seed.Entropy },
            { "seed", "random", _seed.Random },

            { "state", "cooldown", _cooldown },
            { "state", "length", _length },
        };
        return toml.Serialize();
    }

    public static Plant Deserialize(string toml)
    {
        var deserialized = Toml.Deserialize(toml);

        var seed = new Seed
        {
            Axiom = deserialized.GetRequiredValue<string>("axiom"),
            Rules = deserialized.GetRequiredValue<IDictionary<char, string>>("rules"),
            Iterations = deserialized.GetRequiredValue<int>("iterations"),
            Step = deserialized.GetRequiredValue<float>("step"),
            Turn = deserialized.GetRequiredValue<float>("turn"),
        };
        if (deserialized.TryGetValue("entropy", out float entropy))
        {
            seed.Entropy = entropy;
        }
        if (deserialized.TryGetValue("random", out int random))
        {
            seed.Random = random;
        }
        var plant = new Plant(seed);

        if (deserialized.TryGetValue("cooldown", out float cooldown))
        {
            plant._cooldown = cooldown;
        }
        if (deserialized.TryGetValue("length", out int length))
        {
           plant._length = length;
        }
        return plant;
    }
}
