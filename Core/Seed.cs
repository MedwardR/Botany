namespace Botany.Core;

public record Seed
{
    public required string Axiom { get; set; }

    public IDictionary<char, string> Rules { get; set; } = new Dictionary<char, string>();

    public required int Iterations { get; set; }

    public required float Step { get; set; }

    public required float Turn { get; set; }

    public float Entropy { get; set; } = 0f;

    public int Random { get; set; } = System.Random.Shared.Next();
}
