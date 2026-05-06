using Botany.Abstractions;
using Botany.Interfaces;

namespace Botany.State;

internal class Plant : IUpdateable, ISerializable<Plant>
{
    private const float COOLDOWN_SECONDS = 86400f; // 24 hours
    private const float MAX_VARIATION = 7200f; // 2 hours

    private readonly Seed _seed;
    private readonly Random _random;
    private readonly LSystem _lsys;
    private readonly Turtle _turtle;
    private readonly List<Line> _lines;

    private float _cooldown;
    private int _length;

    public IEnumerable<Line> Lines => _lines.Take(_length);
    public float Speed { get; set; }

    public Plant(Seed seed)
    {
        _seed = seed;
        _random = new();

        _lsys = new(seed.Axiom, seed.Rules);
        _turtle = new(seed.Step, seed.Turn)
        {
            Entropy = 2f,
        };
        string commands = _lsys.Expand(seed.Iterations);

        _lines = [.. _turtle.Run(commands)];
        _cooldown = 0;
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

            _cooldown = period + entropy;
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
            { "seed", "step", _seed.Step },
            { "seed", "turn", _seed.Turn },
            { "seed", "iterations", _seed.Iterations },

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
            Axiom = GetRequiredValue<string>(deserialized, "axiom"),
            Rules = GetRequiredValue<IDictionary<char, string>>(deserialized, "rules"),
            Step = GetRequiredValue<float>(deserialized, "step"),
            Turn = GetRequiredValue<float>(deserialized, "turn"),
            Iterations = GetRequiredValue<int>(deserialized, "iterations"),
        };
        var plant = new Plant(seed)
        {
            _cooldown = GetValueOrDefault(deserialized, "cooldown", 0f),
            _length = GetValueOrDefault(deserialized, "length", 0),
        };
        return plant;
    }

    private static T GetRequiredValue<T>(Toml toml, string key)
    {
        T? value;

        if (toml.TryGetValue(key, out var raw))
        {
            if (raw is T v)
            {
                value = v;
            }
            else value = default;
        }
        else value = default;

        if (value is null)
        {
            throw new InvalidOperationException($"Required value not provided: '{key}'");
        }
        else return value;
    }

    private static T GetValueOrDefault<T>(Toml toml, string key, T defaultValue)
    {
        T? value;

        if (toml.TryGetValue(key, out var raw))
        {
            if (raw is T v)
            {
                value = v;
            }
            else value = default;
        }
        else value = default;

        if (value is null)
        {
            return defaultValue;
        }
        else return value;
    }
}
