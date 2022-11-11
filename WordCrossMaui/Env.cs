using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordCrossMaui
{
    internal static class Env
    {
        public static readonly string PathToDictionaryV1 = Path.Join(FileSystem.AppDataDirectory, "dic");
        public static readonly string PathToDictionary = Path.Join(FileSystem.AppDataDirectory, "dicv2");
    }
}
