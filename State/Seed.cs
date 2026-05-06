namespace Botany.State;

public record Seed
{
    public string Axiom { get; set; } = string.Empty;

    public IDictionary<char, string> Rules { get; set; } = new Dictionary<char, string>();

    public float Step { get; set; }

    public float Turn { get; set; }

    public int Iterations { get; set; }
}
