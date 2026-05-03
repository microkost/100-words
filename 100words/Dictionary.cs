using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace words100
{
    public static class Dictionary
    {
        private static DictionaryData _cache = null;

        public static DictionaryData Load()
        {
            if (_cache != null)
                return _cache;

            var uri = new System.Uri("ms-appx:///Assets/dictionary.json");
            var file = Task.Run(async () => await StorageFile.GetFileFromApplicationUriAsync(uri)).Result;
            var json = Task.Run(async () => await FileIO.ReadTextAsync(file)).Result;

            var root = JsonObject.Parse(json);

            var languages = new List<LanguageDefinition>();
            foreach (var lang in root["languages"].GetArray())
            {
                var o = lang.GetObject();
                languages.Add(new LanguageDefinition
                {
                    Code = o["code"].GetString(),
                    Name = o["name"].GetString(),
                    Flag = o["flag"].GetString()
                });
            }

            var phrases = new List<Phrase>();
            foreach (var p in root["phrases"].GetArray())
            {
                var o = p.GetObject();
                var phrase = new Phrase { Level = o["level"].GetString() };
                foreach (var lang in languages)
                    phrase.Translations[lang.Code] = o[lang.Code].GetString();
                phrases.Add(phrase);
            }

            _cache = new DictionaryData { Languages = languages, Phrases = phrases };
            return _cache;
        }

        public static List<LanguageDefinition> GetListOfLanguages()
        {
            return Load().Languages;
        }

        public static List<Phrase> GetListOfWords(bool includeAdvanced = false)
        {
            var phrases = Load().Phrases;
            return includeAdvanced
                ? phrases.ToList()
                : phrases.Where(p => p.Level == "basic").ToList();
        }
    }

    public class DictionaryData
    {
        public List<LanguageDefinition> Languages { get; set; }
        public List<Phrase> Phrases { get; set; }
    }

    public class LanguageDefinition
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Flag { get; set; }
    }
}
