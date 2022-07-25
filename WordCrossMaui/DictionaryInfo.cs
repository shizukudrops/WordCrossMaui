using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordCrossMaui
{
    public record DictionaryInfo(
        string Name, 
        string BaseUrl, 
        string Separator = "", 
        string Suffix = ""
        );
}
