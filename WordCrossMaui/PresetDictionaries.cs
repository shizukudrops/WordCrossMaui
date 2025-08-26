using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace WordCrossMaui
{
    internal static class PresetDictionaries
    {
        public static ReadOnlyCollection<DictionaryInfo> DictionaryList { get
            {
                return new ReadOnlyCollection<DictionaryInfo>(new List<DictionaryInfo>
                {
                    new DictionaryInfo("Longman", "https://www.ldoceonline.com/jp/dictionary/", "-", "", true),
                    new DictionaryInfo("Oxford Learners", "https://www.oxfordlearnersdictionaries.com/definition/english/", "-", "", true),
                    new DictionaryInfo("Collins English", "https://www.collinsdictionary.com/dictionary/english/", "-", "", true),
                    new DictionaryInfo("Collins Thesaurus", "https://www.collinsdictionary.com/dictionary/english-thesaurus/", "-", "", true),
                    new DictionaryInfo("Merriam Webster", "https://www.merriam-webster.com/dictionary/", "%20", "", true),
                    new DictionaryInfo("American Heritage", "https://ahdictionary.com/word/search.html?q=", "+", "", false),
                    new DictionaryInfo("英辞郎", "https://eow.alc.co.jp/", "+", "", true),
                    new DictionaryInfo("Weblio英和・和英", "https://ejje.weblio.jp/content/", "+", "", true),
                    new DictionaryInfo("DictJuggler", "https://www.dictjuggler.net/yakugo/?word=", "%20", "", true),
                    new DictionaryInfo("Wikipedia日本語版", "https://ja.wikipedia.org/wiki/", "", "", true),
                    new DictionaryInfo("Wikipedia English", "https://en.wikipedia.org/wiki/", "", "", true),
                });
            } }
    }
}
