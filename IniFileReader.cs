using System;
using System.Collections.Generic;
using System.IO;

namespace BrowserSelect
{
    public class IniFileReader
    {
        private readonly Dictionary<string, Dictionary<string, string>> _data;

        public IniFileReader(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("The specified INI file does not exist.", filePath);

            _data = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            ParseFile(filePath);
        }

        private void ParseFile(string filePath)
        {
            string currentSection = string.Empty;

            foreach (var line in ReadLinesShared(filePath))
            {
                var trimmedLine = line.Trim();

                // Skip empty lines or comments
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                    continue;

                // Section header
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    currentSection = trimmedLine[1..^1].Trim();
                    if (!_data.ContainsKey(currentSection))
                        _data[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
                else if (trimmedLine.Contains('='))
                {
                    // Key-value pair
                    var parts = trimmedLine.Split(new[] { '=' }, 2);
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();

                    if (!string.IsNullOrEmpty(currentSection))
                        _data[currentSection][key] = value;
                }
            }
        }

        private static IEnumerable<string> ReadLinesShared(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var sr = new StreamReader(fs);
            string? line;
            while ((line = sr.ReadLine()) != null)
                yield return line;
        }

        public string? GetValue(string section, string key, string? defaultValue = null)
        {
            if (_data.TryGetValue(section, out var sectionData) && sectionData.TryGetValue(key, out var value))
                return value;

            return defaultValue;
        }

        public Dictionary<string, string>? GetSection(string section)
        {
            if (_data.TryGetValue(section, out var sectionData))
                return new Dictionary<string, string>(sectionData);

            return null;
        }
    }
}
