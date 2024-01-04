using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Avrahamy.Utils {
    // (avrahamy) Moved by me from Editor assembly to here.
    public static class CSVUtils {
        // Use this regular expression to parse .csv files.
        private const string CSV_REGEX_PATTERN =
            // Matches beginning of string or right after a comma (the beginning of a cell)
            "((^|(?<=,))" +
            // Matches anything that doesn't start with double quotes
            "(([^\\\",][^,]*?)|([^\\\",]*?))" +
            // Matches ending of string or right before a comma (the end of a cell)
            "((?=,)|$))|" +
            // Matches beginning of cell that starts with double quotes
            "((^\\\"|(?<=,\\\"))" +
            // Doesn't allow single 'double quotes'
            "(([^\\\"]*\\\"\\\")*[^\\\"]*)" +
            // Matches ending of a cell that ends with double quotes
            "(?=((\\\",)|(\\\"$))))";

        private static readonly Regex CSV_REGEX = new (CSV_REGEX_PATTERN);

        public class CSVContainer {
            public string[] attributes;
            public string[][] csvRows;

            public override string ToString() {
                var sb = new StringBuilder();
                for (int i = 0; i < attributes.Length; i++) {
                    sb.Append(EscapeForCSV(attributes[i]));
                    if (i != attributes.Length - 1) {
                        sb.Append(',');
                    }
                }
                foreach (var row in csvRows) {
                    sb.AppendLine();
                    for (int i = 0; i < row.Length; i++) {
                        if (!string.IsNullOrEmpty(row[i])) {
                            sb.Append(EscapeForCSV(row[i]));
                        }
                        if (i != row.Length - 1) {
                            sb.Append(',');
                        }
                    }
                }
                return sb.ToString();
            }

            public bool Contains(string value, int columnIndex) {
                foreach (var row in csvRows) {
                    if (row[columnIndex] == value) return true;
                }
                return false;
            }

            public string[] VLookup(string value, int searchColumnIndex) {
                foreach (var row in csvRows) {
                    if (row[searchColumnIndex] == value) return row;
                }
                return null;
            }

            public string VLookup(string value, int searchColumnIndex, int returnColumnIndex) {
                foreach (var row in csvRows) {
                    DebugAssert.Assert(searchColumnIndex >= 0 && searchColumnIndex < row.Length, $"Search column index is {searchColumnIndex} while row has {row.Length} cells");
                    if (row[searchColumnIndex] != value) continue;
                    if (returnColumnIndex >= 0 && returnColumnIndex < row.Length) {
                        return row[returnColumnIndex];
                    }
                    break;
                }
                return null;
            }

            public List<string> GetCSVRows() {
                var rows = new List<string>();
                var sb = new StringBuilder();
                for (int i = 0; i < attributes.Length; i++) {
                    sb.Append(EscapeForCSV(attributes[i]));
                    if (i != attributes.Length - 1) {
                        sb.Append(',');
                    }
                }
                rows.Add(sb.ToString());
                foreach (var row in csvRows) {
                    sb = new StringBuilder();
                    for (int i = 0; i < row.Length; i++) {
                        sb.Append(EscapeForCSV(row[i]));
                        if (i != row.Length - 1) {
                            sb.Append(',');
                        }
                    }
                    rows.Add(sb.ToString());
                }
                return rows;
            }

            private static string EscapeForCSV(string text) {
                if (string.IsNullOrEmpty(text)) return text;
                if (text.Contains("\n")) {
                    text = text.Replace("\n", "\\n");
                }
                if (text.Contains(",") || text.Contains("\"")) {
                    text = $"\"{text.Replace("\"", "\"\"")}\"";
                }
                return text;
            }
        }

        public static CSVContainer LoadCSV(string assetPath) {
            if (!File.Exists(assetPath)) {
                DebugLog.LogError($"File {assetPath} does not exist");
                return null;
            }
            using (var reader = new StreamReader(assetPath)) {
                var text = reader.ReadToEnd();
                reader.Close();
                var csv = text.ParseCSV();
                return csv;
            }
        }

        public static CSVContainer ParseCSV(this string text) {
            var reader = new StringReader(text);
            string line = reader.ReadLine();

            if (line == null) {
                DebugLog.LogError("Trying to read empty CSV file");
                return null;
            }

            var attributes = CSV_REGEX.Matches(line).ToStringArray();

            var lines = new List<string[]>();
            while ((line = reader.ReadLine()) != null) {
                lines.Add(CSV_REGEX.Matches(line).ToStringArray());
            }

            reader.Close();

            return new CSVContainer {
                attributes = attributes,
                csvRows = lines.ToArray(),
            };
        }

        public static string[] ToStringArray(this MatchCollection collection) {
            var result = new string[collection.Count];

            for (int i = 0; i < collection.Count; i++) {
                result[i] = collection[i].Value.Replace("\"\"", "\"").Replace("\\n", "\n");
            }

            return result;
        }
    }
}
