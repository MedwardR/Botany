using System.Collections;
using System.Numerics;
using Botany.Interfaces;
using Botany.Serialization;

namespace Botany.State;

public class Plant : IPositionable, IRotatable, IUpdateable, ISerializable<Plant>
{
    private const float COOLDOWN_SECONDS = 86400f; // 24 hours
    private const float MAX_VARIATION = 7200f; // 2 hours

    public Vector2 Position { get; set; }
    public float Rotation { get; set; }
    public float Speed { get; set; }

    public IReadOnlyList<Segment> Segments => _segments;
    public int Length => _length;

    private readonly Seed _seed;
    private readonly Random _random;
    private readonly LSystem _lsys;
    private readonly Turtle _turtle;
    private readonly List<Segment> _segments;

    private float _cooldown;
    private int _length;

    public Plant(Seed seed)
    {
        _seed = seed;
        _random = new(seed.Random);

        _lsys = new(seed.Axiom, seed.Rules);
        string instructions = _lsys.Expand(seed.Iterations);

        _turtle = new(instructions, _random)
        {
            Step = seed.Step,
            Turn = seed.Turn,
            Entropy = seed.Entropy,
        };
        _segments = [.. _turtle.Enumerate()];

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
        if (_length < _segments.Count)
        {
            _length++;
        }
    }

    public string Serialize()
    {
        var ini = new Ini
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
        return ini.Serialize();
    }

    public static Plant Deserialize(string ini)
    {
        var deserialized = Ini.Deserialize(ini);

        var seed = new Seed
        {
            Axiom = deserialized.GetRequiredValue<string>("axiom"),
            Rules = GetRules(deserialized),
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

    private static Dictionary<char, string> GetRules(Ini ini)
    {
        var raw = ini.GetRequiredValue<IDictionary>("rules");
        var rules = new Dictionary<char, string>();

        foreach (DictionaryEntry entry in raw)
        {
            string? key = entry.Key.ToString();
            string? value = entry.Value as string;

            if (key is not null && value is not null)
            {
                if (key.Length == 1)
                {
                    char c = key[0];
                    rules.Add(c, value);
                }
            }
        }
        return rules;
    }
}
