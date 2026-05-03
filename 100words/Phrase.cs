using System.Collections.Generic;

namespace words100
{
    public class Phrase
    {
        public string Level { get; set; }
        public Dictionary<string, string> Translations { get; set; } // key = language code e.g. "FI", "EN"

        public Phrase()
        {
            Translations = new Dictionary<string, string>();
        }

        public string GetTranslation(string languageCode)
        {
            return Translations.TryGetValue(languageCode, out string word) ? word : "?";
        }
    }
}

