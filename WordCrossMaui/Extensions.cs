using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordCrossMaui
{
    internal class Extensions
    {
        public static ObservableCollection<DictionaryViewModel> Clone(ObservableCollection<DictionaryViewModel> source)
        {
            return new ObservableCollection<DictionaryViewModel>(source.Select(d => new DictionaryViewModel(d)));
        }

        public static List<DictionaryViewModel> Clone(List<DictionaryViewModel> source)
        {
            return new List<DictionaryViewModel>(source.Select(d => new DictionaryViewModel(d)));
        }

        public static IEnumerable<DictionaryViewModel> Convert(IEnumerable<DictionaryInfo> source)
        {
            return source.Select(d => new DictionaryViewModel(d));
        }

        public static IEnumerable<DictionaryInfo> Convert(IEnumerable<DictionaryViewModel> source)
        {
            return source.Select(d => new DictionaryInfo(d));
        }
    }
}
