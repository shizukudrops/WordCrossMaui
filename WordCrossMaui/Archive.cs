using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WordCrossMaui
{
    internal class Archive
    {
        public string ClientId { get; set; }
        public DateTime TimeStamp { get; set; }
        public List<DictionaryInfo> Dictionaries { get; set; }

        public Archive() { }

        public Archive(IEnumerable<DictionaryInfo> dictionaries)
        {
            ClientId = Preferences.Get("client_id", "");
            TimeStamp = DateTime.Now;
            Dictionaries = dictionaries.ToList();
        }

        public Archive(IEnumerable<DictionaryViewModel> dictionaries)
        {
            ClientId = Preferences.Get("client_id", "");
            TimeStamp = DateTime.Now;
            Dictionaries = Extensions.Convert(dictionaries).ToList();
        }
    }
}
