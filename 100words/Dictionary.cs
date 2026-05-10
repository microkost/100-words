using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

namespace words100
{
    public static class Dictionary
    {
        private static DictionaryData _cache = null;

        public static async Task<DictionaryData> LoadAsync()
        {
            if (_cache != null)
                return _cache;

            var path = Path.Combine(AppContext.BaseDirectory, "Assets", "dictionary.json");
            var json = await File.ReadAllTextAsync(path);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var languages = new List<LanguageDefinition>();
            foreach (var lang in root.GetProperty("languages").EnumerateArray())
            {
                languages.Add(new LanguageDefinition
                {
                    Code = lang.GetProperty("code").GetString(),
                    Name = lang.GetProperty("name").GetString(),
                    Flag = lang.GetProperty("flag").GetString()
                });
            }

            var phrases = new List<Phrase>();
            foreach (var p in root.GetProperty("phrases").EnumerateArray())
            {
                var phrase = new Phrase { Level = p.GetProperty("level").GetString() };
                foreach (var lang in languages)
                    phrase.Translations[lang.Code] = p.GetProperty(lang.Code).GetString();
                phrases.Add(phrase);
            }

            _cache = new DictionaryData { Languages = languages, Phrases = phrases };
            return _cache;
        }

        public static async Task<List<LanguageDefinition>> GetListOfLanguagesAsync()
        {
            return (await LoadAsync()).Languages;
        }

        public static async Task<List<Phrase>> GetListOfWordsAsync(bool includeAdvanced = false)
        {
            var phrases = (await LoadAsync()).Phrases;
            return includeAdvanced
                ? phrases.ToList()
                : phrases.Where(p => p.Level == "basic").ToList();
        }
    }

    public class DictionaryData
    {
        public List<LanguageDefinition> Languages { get; set; } = new();
        public List<Phrase> Phrases { get; set; } = new();
    }

    public class LanguageDefinition
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Flag { get; set; } = string.Empty;
    }
}

