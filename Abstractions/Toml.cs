using Botany.Interfaces;
using System.IO;
using System.Text;

namespace Botany.Abstractions;

internal class Toml : OrderedDictionary<string, object?>, ISerializable<Toml>
{
    private const string DEFAULT_SECTION = "miscellaneous";
    private const string NULL_VALUE = "null";

    private readonly Dictionary<string, string> _sections = [];

    public Toml() : base(StringComparer.OrdinalIgnoreCase) { }

    public void Add(string section, string key, object? value)
    {
        string trimmedKey = key.Trim();
        string trimmedSection = section.Trim();

        Add(trimmedKey, value);
        _sections.Add(trimmedKey, trimmedSection);
    }

    public string Serialize()
    {
        var builder = new StringBuilder();
        var groups = this.GroupBy(GetSection);
        var ordered = groups.OrderBy(IsDefaultSection);

        foreach (var g in ordered)
        {
            if (builder.Length > 0)
            {
                builder.AppendLine();
            }
            builder.AppendLine($"[{g}]");

            foreach ((string key, var value) in g)
            {
                if (value is not null)
                {
                    builder.AppendLine($"{key} = {value}");
                }
                else builder.AppendLine($"{key} = {NULL_VALUE}");
            }
        }
        builder.AppendLine();

        return builder.ToString();
    }

    private string GetSection(KeyValuePair<string, object?> entry)
    {
        if (_sections.TryGetValue(entry.Key, out var section))
        {
            return section;
        }
        else return DEFAULT_SECTION;
    }

    private static bool IsDefaultSection(IGrouping<string, KeyValuePair<string, object?>> group)
    {
        string trimmed = group.Key.Trim();
        return trimmed.Equals(DEFAULT_SECTION, StringComparison.OrdinalIgnoreCase);
    }

    public static Toml Deserialize(string s)
    {
        var toml = new Toml();

        using var reader = new StringReader(s);
        string section = DEFAULT_SECTION;
        string? line;

        while ((line = reader.ReadLine()) is not null)
        {
            int semicolon = line.IndexOf(';');
            int pound = line.IndexOf('#');
            int comment;

            if (semicolon > -1 && pound > -1)
            {
                comment = int.Min(semicolon, pound);
            }
            else comment = int.Max(semicolon, pound);

            string normalized = line[..comment].Trim();

            if (normalized.StartsWith('['))
            {
                section = normalized.TrimStart('[').TrimEnd(']').Trim();
            }
            else if (!string.IsNullOrWhiteSpace(normalized))
            {
                int separator = normalized.IndexOf('=');

                string key = normalized[..separator].Trim();
                string value = normalized[(separator + 1)..].Trim();

                bool valueIsNull =
                    value.Equals(NULL_VALUE, StringComparison.OrdinalIgnoreCase) ||
                    string.IsNullOrWhiteSpace(value);

                if (valueIsNull)
                {
                    toml.Add(section, key, null);
                }
                else toml.Add(section, key, value);
            }
        }
        return toml;
    }
}
