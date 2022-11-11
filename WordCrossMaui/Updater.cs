using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace WordCrossMaui
{
    internal static class Updater
    {
        public static void ConvertDictionaryType()
        {
            if (File.Exists(Env.PathToDictionaryV1))
            {
                var rawData = File.ReadAllText(Env.PathToDictionaryV1);

                var deserialized = JsonSerializer.Deserialize<ObservableCollection<DictionaryInfo>>(rawData);

                if (deserialized != null)
                {
                    File.WriteAllText(Env.PathToDictionary, JsonSerializer.Serialize(new Archive(deserialized)));
                    File.Delete(Env.PathToDictionaryV1);
                }
            }
        }
    }
}
