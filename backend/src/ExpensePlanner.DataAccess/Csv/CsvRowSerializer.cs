using System.Text;

namespace ExpensePlanner.DataAccess.Csv;

public static class CsvRowSerializer
{
    public static string Serialize(IReadOnlyList<string> values)
    {
        return string.Join(",", values.Select(Escape));
    }

    public static IReadOnlyList<string> Parse(string line)
    {
        var values = new List<string>();
        var buffer = new StringBuilder();
        var inQuotes = false;

        for (var index = 0; index < line.Length; index++)
        {
            var current = line[index];

            if (inQuotes)
            {
                if (current == '"')
                {
                    var isEscapedQuote = index + 1 < line.Length && line[index + 1] == '"';
                    if (isEscapedQuote)
                    {
                        buffer.Append('"');
                        index++;
                        continue;
                    }

                    inQuotes = false;
                    continue;
                }

                buffer.Append(current);
                continue;
            }

            if (current == ',')
            {
                values.Add(buffer.ToString());
                buffer.Clear();
                continue;
            }

            if (current == '"')
            {
                inQuotes = true;
                continue;
            }

            buffer.Append(current);
        }

        values.Add(buffer.ToString());
        return values;
    }

    private static string Escape(string value)
    {
        var needsQuoting = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        if (!needsQuoting)
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}