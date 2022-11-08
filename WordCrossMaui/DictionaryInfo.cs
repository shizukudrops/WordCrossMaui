using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordCrossMaui
{
    public class DictionaryInfo {
        public readonly string id = Guid.NewGuid().ToString();

        public string Name { get; set; } = "";
        public string BaseUrl { get; set; } = "";
        public string Separator { get; set; } = "";
        public string Suffix { get; set; } = "";
        public bool IsDefault { get; set; } = false;

        public DictionaryInfo() { }

        public DictionaryInfo(string name, string baseUrl, string separator = "", string suffix = "", bool isDefault = false)
        {
            Name = name;
            BaseUrl = baseUrl;
            Separator = separator;
            Suffix = suffix;
            IsDefault = isDefault;
        }

    };
}
