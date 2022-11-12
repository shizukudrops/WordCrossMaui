using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordCrossMaui
{
    internal class Archive
    {
        public string ClientId { get; set; }
        public DateTime TimeStamp { get; set; }
        public ObservableCollection<DictionaryInfo> Dictionaries { get; set; }

        public Archive() { }

        public Archive(ObservableCollection<DictionaryInfo> dictionaries)
        {
            ClientId = Preferences.Get("client_id", "");
            TimeStamp = DateTime.Now;
            Dictionaries = dictionaries;
        }

        public Archive(ObservableCollection<DictionaryViewModel> dictionaries)
        {
            ClientId = Preferences.Get("client_id", "");
            TimeStamp = DateTime.Now;
            Dictionaries = new ObservableCollection<DictionaryInfo>(dictionaries.Select(d => (DictionaryInfo)d));
        }
    }
}
