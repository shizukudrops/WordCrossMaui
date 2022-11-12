using Dropbox.Api.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordCrossMaui
{
    public class DictionaryViewModel : DictionaryInfo, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private Color? _backgroundColor;

        public Color? BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                _backgroundColor = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("BackgroundColor"));
                }
            }
        }

        public DictionaryViewModel(string name, string baseUrl, string separator = "", string suffix = "", bool isDefault = false)
            : base(name, baseUrl, separator, suffix, isDefault)
        {  }

        public DictionaryViewModel(DictionaryInfo dictionaryInfo)
            :base(dictionaryInfo)
        {  }

        public DictionaryViewModel(DictionaryViewModel dictionaryViewModel)
            :base(dictionaryViewModel as DictionaryInfo)
        {  }

        public void Highlight()
        {
            AppTheme theme = Application.Current.RequestedTheme;

            if(theme == AppTheme.Light)
            {
                BackgroundColor = Color.FromArgb("A7CBF6");
            }
            else if(theme == AppTheme.Dark)
            {
                BackgroundColor = Color.FromArgb("4778b3");
            }
            else
            {
                BackgroundColor = Color.FromArgb("A7CBF6");
            }
            
        }

        public void UnHighlight()
        {
            BackgroundColor = null;
        }
    }
}
