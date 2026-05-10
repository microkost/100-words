using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace words100
{
    /// <summary>
    /// Lightweight file-based settings store that mimics ApplicationDataContainer.Values
    /// for use in unpackaged WinUI 3 apps (no package identity required).
    /// Settings are persisted as JSON in %LOCALAPPDATA%\100words\settings.json.
    /// </summary>
    internal class LocalSettingsHelper
    {
        private readonly string _filePath;
        private readonly Dictionary<string, object?> _values;

        public LocalSettingsValues Values { get; }

        public LocalSettingsHelper(string appFolderName = "100words")
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appFolderName);
            Directory.CreateDirectory(folder);
            _filePath = Path.Combine(folder, "settings.json");

            _values = Load();
            Values = new LocalSettingsValues(_values, Save);
        }

        private Dictionary<string, object?> Load()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    var raw = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                    if (raw != null)
                    {
                        var result = new Dictionary<string, object?>();
                        foreach (var kvp in raw)
                            result[kvp.Key] = DeserializeElement(kvp.Value);
                        return result;
                    }
                }
            }
            catch { /* corrupt file – start fresh */ }
            return new Dictionary<string, object?>();
        }

        private void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(_values, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch { }
        }

        private static object? DeserializeElement(JsonElement element) => element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number when element.TryGetInt64(out long l) => l,
            JsonValueKind.Number => element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => DeserializeArray(element),
            _ => null
        };

        private static string[] DeserializeArray(JsonElement element)
        {
            var list = new System.Collections.Generic.List<string>();
            foreach (var item in element.EnumerateArray())
                list.Add(item.GetString() ?? string.Empty);
            return list.ToArray();
        }
    }

    /// <summary>
    /// Dictionary-like accessor that auto-saves on write,
    /// matching the ApplicationDataContainer.Values usage pattern.
    /// </summary>
    internal class LocalSettingsValues
    {
        private readonly Dictionary<string, object?> _data;
        private readonly Action _save;

        internal LocalSettingsValues(Dictionary<string, object?> data, Action save)
        {
            _data = data;
            _save = save;
        }

        public object? this[string key]
        {
            get => _data.TryGetValue(key, out var val) ? val : null;
            set
            {
                _data[key] = value;
                _save();
            }
        }
    }
}
