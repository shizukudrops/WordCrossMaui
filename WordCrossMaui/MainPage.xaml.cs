using System.Collections.ObjectModel;

namespace WordCrossMaui;

[QueryProperty(nameof(NewDictionary), "NewDictionary")]
[QueryProperty(nameof(DictView), "UpdatedDictionaryList")]
public partial class MainPage : ContentPage
{
    ObservableCollection<DictionaryInfo> dictView = new ObservableCollection<DictionaryInfo>();

    public ObservableCollection<DictionaryInfo> DictView
    {
        get => dictView;
        set
        {
            if(value == null) return;

            dictView.Clear();

            foreach(var d in value)
            {
                dictView.Add(d);
            }

            OnPropertyChanged();
        }
    }

    DictionaryInfo newDictionary = new DictionaryInfo();

    public DictionaryInfo NewDictionary
    {
        get => newDictionary;
        set
        {
            newDictionary = value;
            dictView.Add(value);
            OnPropertyChanged();
        }
    }
    
    public MainPage()
	{
		InitializeComponent();

        BindingContext = this;

        dictView.Add(new DictionaryInfo("Longman", "https://www.ldoceonline.com/jp/dictionary/", "-"));
        dictView.Add(new DictionaryInfo("Oxford Learners", "https://www.oxfordlearnersdictionaries.com/definition/english/", "-"));
        dictView.Add(new DictionaryInfo("Collins Thesaurus", "https://www.collinsdictionary.com/dictionary/english-thesaurus/", "-"));
        dictView.Add(new DictionaryInfo("Merriam Webster", "https://www.merriam-webster.com/dictionary/", "%20"));
        dictView.Add(new DictionaryInfo("英辞郎", "https://eow.alc.co.jp/", "+"));
        dictView.Add(new DictionaryInfo("Weblio英和・和英", "https://ejje.weblio.jp/content/", "+"));
        dictView.Add(new DictionaryInfo("goo辞書英和・和英", "https://dictionary.goo.ne.jp/word/en/", "+"));
        dictView.Add(new DictionaryInfo("DictJuggler", "https://www.dictjuggler.net/yakugo/?word=", "%20"));
        dictView.Add(new DictionaryInfo("WordNet 3.1", "http://wordnetweb.princeton.edu/perl/webwn?s=", "+"));
        dictView.Add(new DictionaryInfo("Wikipedia日本語版", "https://ja.wikipedia.org/wiki/", ""));
        dictView.Add(new DictionaryInfo("Wikipedia English", "https://en.wikipedia.org/wiki/", ""));
        dictView.Add(new DictionaryInfo("goo辞書国語", "https://dictionary.goo.ne.jp/word/", "+"));

        dictList.ItemsSource = dictView;
    }

    private void Search(DictionaryInfo dict, string input)
    {
        //どの辞書も選ばれていなかったら一番上の辞書で検索する。辞書が存在しなければ戻る。
        if (dict == null)
        {
            if (dictView.Count > 0) dict = dictView.First();
            else return;
        }

        string[] words;

        if(input != null)
        {
            words = input.Split(" ");
        }
        else
        {
            words = new string[0];
        }
        
        string separator;

        //separatorが存在しなければホワイトスペースで代用する
        if (string.IsNullOrEmpty(dict.Separator))
        {
            separator = " ";
        }
        else
        {
            separator = dict.Separator;
        }

        var searchWords = string.Join(separator, words);
        var targetUriString = dict.BaseUrl + searchWords;

        Uri targetUri;

        if (Uri.TryCreate(targetUriString, UriKind.Absolute, out targetUri))
        {
            webView.Source = targetUri;
        }
    }

    private void SearchBox_Completed(object sender, EventArgs e)
    {
        Search((DictionaryInfo)dictList.SelectedItem, ((Entry)sender).Text);
    }

    private void DictList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        Search((DictionaryInfo)e.SelectedItem, searchBox.Text);
    }

    private void Back_Clicked(object sender, EventArgs e)
    {
        if (webView.CanGoBack) webView.GoBack();
    }

    private void Forward_Clicked(object sender, EventArgs e)
    {
        if (webView.CanGoForward) webView.GoForward();
    }

    private async void AddDictionary_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///AddDictionary");
    }

    private async void ManageDictionary_Clicked(object sender, EventArgs e)
    {
        var param = new Dictionary<string, object>
        {
            {"DictionaryList",  dictView}
        };

        await Shell.Current.GoToAsync("///ManageDictionary", param);
    }

    private async void About_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("///About");
    }



}

