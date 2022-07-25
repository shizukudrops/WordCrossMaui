using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordCrossMaui
{
    public class DictionaryInfo
    {
        public string Name { get; set; }
        public string BaseUrl { get; set; }
        public string Separator { get; set; }
        public string Suffix { get; set; }

        public DictionaryInfo() { }

        public DictionaryInfo(string name, string baseUrl, string separator = "", string suffix = "")
        {
            Name = name;
            BaseUrl = baseUrl;
            Separator = separator;
            Suffix = suffix;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
