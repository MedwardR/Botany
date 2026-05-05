using System.Text;

namespace Botany.State;

internal class LSystem(string axiom, IDictionary<char, string> rules)
{
    public string Axiom { get; } = axiom;
    public IDictionary<char, string> Rules { get; } = rules;

    public string Expand(int iterations)
    {
        string source = Axiom;
        var target = new StringBuilder();

        for (int index = 0; index < iterations; index++)
        {
            if (index > 0) target.Clear();

            foreach (var c in source)
            {
                if (Rules.TryGetValue(c, out var replacement))
                {
                    target.Append(replacement);
                }
                else target.Append(c);
            }
            source = target.ToString();
        }
        return source;
    }
}
