using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace WordCrossMaui
{
    public class DictionaryInfo {
        //一意のIDが振られていないと同じ内容の辞書が複数登録されているときに選択でバグる
        public readonly string id = Guid.NewGuid().ToString();

        public string Name { get; }
        public string BaseUrl { get; }
        public string Separator { get; }
        public string Suffix { get; }

        [JsonIgnore]
        public bool IsDefault { get; }

        [JsonConstructor]
        public DictionaryInfo(string name, string baseUrl, string separator = "", string suffix = "", bool isDefault = false)
        {
            Name = name;
            BaseUrl = baseUrl;
            Separator = separator;
            Suffix = suffix;
            IsDefault = isDefault;
        }

        public DictionaryInfo(DictionaryInfo dictionaryInfo)
        {
            Name = dictionaryInfo.Name;
            BaseUrl = dictionaryInfo.BaseUrl;
            Separator = dictionaryInfo.Separator;
            Suffix = dictionaryInfo.Suffix;
            IsDefault = dictionaryInfo.IsDefault;
        }

    };
}
