using System.Text;

namespace NivBot.Features.Shared
{
    public static class Csv
    {
        // Minimal CSV reader: comma separated, fields may be quoted with "" as an escaped quote.
        // Empty lines are skipped. Returns the rows as lists of trimmed fields.
        public static List<List<string>> Parse(string text)
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var field = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (inQuotes)
                {
                    if (c == '"' && i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                    else if (c == '"') { inQuotes = false; }
                    else { field.Append(c); }
                    continue;
                }

                switch (c)
                {
                    case '"':
                        inQuotes = true;
                        break;
                    case ',':
                        row.Add(field.ToString().Trim());
                        field.Clear();
                        break;
                    case '\r':
                        break;
                    case '\n':
                        row.Add(field.ToString().Trim());
                        field.Clear();
                        if (row.Any(x => x.Length > 0)) { rows.Add(row); }
                        row = new List<string>();
                        break;
                    default:
                        field.Append(c);
                        break;
                }
            }

            row.Add(field.ToString().Trim());
            if (row.Any(x => x.Length > 0)) { rows.Add(row); }
            return rows;
        }
    }
}
