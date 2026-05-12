using Botany.Interfaces;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Text;

namespace Botany.Serialization;

internal class Ini : OrderedDictionary<string, object?>, ISerializable<Ini>
{
    private const string DEFAULT_SECTION = "miscellaneous";
    private const string NULL_KEYWORD = "null";

    private readonly Dictionary<string, string> _sections = [];

    public Ini() : base(StringComparer.OrdinalIgnoreCase) { }

    public void Add(string section, string key, object? value)
    {
        string trimmedKey = key.Trim();
        string trimmedSection = section.Trim();

        Add(trimmedKey, value);
        _sections.Add(trimmedKey, trimmedSection);
    }

    public T GetRequiredValue<T>(string key)
    {
        string trimmed = key.Trim();

        if (base.TryGetValue(trimmed, out object? raw))
        {
            if (raw is not null)
            {
                try
                {
                    if (raw is T value)
                    {
                        return value;
                    }
                    else return (T)Convert.ChangeType(raw, typeof(T));
                }
                catch (Exception ex)
                {
                    string message = $"Required value '{trimmed}' was an invalid type: '{raw.GetType()}'";
                    throw new InvalidOperationException(message, ex);
                }
            }
            else throw new InvalidOperationException($"Required value was null: '{trimmed}'");
        }
        else throw new KeyNotFoundException($"Required value not found: '{trimmed}'");
    }

    public T GetValueOrDefault<T>(string key, T defaultValue)
    {
        if (TryGetValue<T>(key, out var value))
        {
            return value ?? defaultValue;
        }
        else return defaultValue;
    }

    public bool TryGetValue<T>(string key, [MaybeNullWhen(false)] out T value)
    {
        if (base.TryGetValue(key, out object? raw))
        {
            if (raw is T v)
            {
                value = v;
                return true;
            }
            else value = default;
        }
        else value = default;

        return false;
    }

    public string Serialize()
    {
        var builder = new StringBuilder();
        var groups = this.GroupBy(GetSection);
        var ordered = groups.OrderBy(IsDefaultSection);

        foreach (var section in ordered)
        {
            if (builder.Length > 0)
            {
                builder.AppendLine();
            }
            builder.AppendLine($"[{section}]");

            foreach ((string key, object? value) in section)
            {
                if (value is not null)
                {
                    builder.AppendLine($"{key} = {value}");
                }
                else builder.AppendLine($"{key} = {NULL_KEYWORD}");
            }
        }
        builder.AppendLine();

        return builder.ToString();
    }

    public static Ini Deserialize(string s)
    {
        var ini = new Ini();

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
                comment = Math.Min(semicolon, pound);
            }
            else comment = Math.Max(semicolon, pound);

            string normalized;

            if (comment > -1)
            {
                normalized = line[..comment].Trim();
            }
            else normalized = line.Trim();

            if (normalized.StartsWith('[') && normalized.EndsWith(']'))
            {
                section = normalized.TrimStart('[').TrimEnd(']').Trim();
            }
            else if (!string.IsNullOrWhiteSpace(normalized))
            {
                int separator = normalized.IndexOf('=');

                if (separator > -1)
                {
                    string key = normalized[..separator].Trim();
                    string raw = normalized[(separator + 1)..].Trim();

                    var value = Parse(raw);

                    if (ini.TryGetValue(key, out var existing))
                    {
                        var entry = value as DictionaryEntry?;

                        if (existing is IDictionary dict && entry is not null)
                        {
                            dict.Add(entry.Value.Key, entry.Value.Value);
                        }
                        else if (existing is ICollection<object?> list)
                        {
                            list.Add(value);
                        }
                        else if (entry is not null && existing is DictionaryEntry other)
                        {
                            ini[key] = new OrderedDictionary<object, object?>
                            {
                                { other.Key, other.Value },
                                { entry.Value.Key, entry.Value.Value },
                            };
                        }
                        else ini[key] = new List<object?> { existing, value };
                    }
                    else ini.Add(key, value);
                }
                else ini.Add(section, normalized, null);
            }
        }
        return ini;
    }

    private static object? Parse(string value)
    {
        string raw = value.Trim();

        bool valueIsNull =
            raw.Equals(NULL_KEYWORD, StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(raw);

        if (valueIsNull)
        {
            return null;
        }
        else if (raw.StartsWith('"') || raw.EndsWith('"'))
        {
            return raw.Trim('"');
        }
        else if (raw.StartsWith('[') && raw.EndsWith(']'))
        {
            string trimmed = raw.TrimStart('[').TrimEnd(']');
            string[] parts = trimmed.Split(',', StringSplitOptions.TrimEntries);

            var k = Parse(parts[0]);
            var v = Parse(parts[1]);

            return new DictionaryEntry(k!, v);
        }
        else if (int.TryParse(raw, out int i))
        {
            return i;
        }
        else if (decimal.TryParse(raw, out decimal m))
        {
            return m;
        }
        else if (DateTime.TryParse(raw, out var d))
        {
            return d;
        }
        else return raw;
    }

    private string GetSection(KeyValuePair<string, object?> entry)
    {
        if (_sections.TryGetValue(entry.Key, out string? section))
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
}
