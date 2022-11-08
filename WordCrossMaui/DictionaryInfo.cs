using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordCrossMaui
{
    public class DictionaryInfo {
        //一意のIDが振られていないと同じ内容の辞書が複数登録されているときに選択でバグる
        public readonly string id = Guid.NewGuid().ToString();

        public string Name { get; set; }
        public string BaseUrl { get; set; }
        public string Separator { get; set; }
        public string Suffix { get; set; }
        public bool IsDefault { get; set; }

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
