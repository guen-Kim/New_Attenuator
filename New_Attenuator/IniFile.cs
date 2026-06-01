using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace New_Attenuator
{
    internal class IniFile
    {
        private readonly Dictionary<string, Dictionary<string, string>> sections = new(StringComparer.OrdinalIgnoreCase);

        public string Read(string section, string key, string defaultValue = "")
        {
            if (sections.TryGetValue(section, out var values) &&
                values.TryGetValue(key, out var value))
            {
                return value;
            }

            return defaultValue;
        }

        public int ReadInt(string section, string key, int defaultValue)
        {
            return int.TryParse(Read(section, key), out var value) ? value : defaultValue;
        }

        public bool ReadBool(string section, string key, bool defaultValue)
        {
            var value = Read(section, key);
            if (bool.TryParse(value, out var boolValue)) return boolValue;
            if (int.TryParse(value, out var intValue)) return intValue != 0;

            return defaultValue;
        }

        public void Write(string section, string key, string value)
        {
            if (!sections.TryGetValue(section, out var values))
            {
                values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                sections[section] = values;
            }

            values[key] = value;
        }

        public void WriteInt(string section, string key, int value)
        {
            Write(section, key, value.ToString());
        }

        public void WriteBool(string section, string key, bool value)
        {
            Write(section, key, value ? "true" : "false");
        }

        public static IniFile Load(string path)
        {
            var ini = new IniFile();
            string currentSection = "";

            foreach (var rawLine in File.ReadAllLines(path))
            {
                var line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";") || line.StartsWith("#"))
                {
                    continue;
                }

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentSection = line[1..^1].Trim();
                    continue;
                }

                int splitIndex = line.IndexOf('=');
                if (splitIndex < 0)
                {
                    continue;
                }

                var key = line[..splitIndex].Trim();
                var value = line[(splitIndex + 1)..].Trim();
                ini.Write(currentSection, key, value);
            }

            return ini;
        }

        public void Save(string path)
        {
            var builder = new StringBuilder();

            foreach (var section in sections)
            {
                if (!string.IsNullOrEmpty(section.Key))
                {
                    builder.Append('[').Append(section.Key).AppendLine("]");
                }

                foreach (var pair in section.Value)
                {
                    builder.Append(pair.Key).Append('=').AppendLine(pair.Value);
                }

                builder.AppendLine();
            }

            File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
        }
    }
}
