using System.Text;
using System.Text.Json;

namespace Botany.State;

public record Seed
{
    public string Axiom { get; set; } = string.Empty;

    public IDictionary<char, string> Rules { get; set; } = new Dictionary<char, string>();

    public float Step { get; set; }

    public float Turn { get; set; }

    public int Iterations { get; set; }

    public string Encode()
    {
        string json = JsonSerializer.Serialize(this);
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        return Convert.ToBase64String(bytes);
    }

    public static Seed Decode(string base64)
    {
        byte[] bytes = Convert.FromBase64String(base64);
        string json = Encoding.UTF8.GetString(bytes);
        return JsonSerializer.Deserialize<Seed>(json) ?? new();
    }
}
