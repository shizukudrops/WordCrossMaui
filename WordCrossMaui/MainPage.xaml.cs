using System.Collections.ObjectModel;
using System.Text.Json;

namespace WordCrossMaui;

[QueryProperty(nameof(NewDictionary), "NewDictionary")]
[QueryProperty(nameof(DictView), "UpdatedDictionaryList")]
public partial class MainPage : ContentPage
{
    readonly string pathToDictionary = Path.Join(FileSystem.AppDataDirectory, "dic");

    readonly Stack<Uri> backStack = new Stack<Uri>();
    readonly Stack<Uri> forwardStack = new Stack<Uri>();

    public Uri? CurrentWebViewSource { get; set; }

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

            File.WriteAllText(pathToDictionary, JsonSerializer.Serialize(dictView));

            OnPropertyChanged();
        }
    }

    DictionaryInfo newDictionary = new DictionaryInfo("", "");

    public DictionaryInfo NewDictionary
    {
        get => newDictionary;
        set
        {
            if (value == null) return;

            newDictionary = value;

            dictView.Add(value);

            File.WriteAllText(pathToDictionary, JsonSerializer.Serialize(dictView));

            OnPropertyChanged();
        }
    }
    
    public MainPage()
	{
		InitializeComponent();

        BindingContext = this;

        //「次回起動時に初期化」のチェックがされていたなら辞書と設定をクリア
        var isInitialize = Preferences.Get("initialize_on_next_launch", false);

        if (isInitialize)
        {
            Preferences.Clear();

            if (File.Exists(pathToDictionary))
            {
                File.Delete(pathToDictionary);
            }
        }

        //辞書リストをロード
        if (File.Exists(pathToDictionary))
        {
            var rawData = File.ReadAllText(pathToDictionary);

            var deserialized = JsonSerializer.Deserialize<ObservableCollection<DictionaryInfo>>(rawData);

            if(deserialized != null)
            {
                dictView = deserialized;
            }
        }
        else
        {
            dictView.Add(new DictionaryInfo("Longman", "https://www.ldoceonline.com/jp/dictionary/", "-"));
            dictView.Add(new DictionaryInfo("Oxford Learners", "https://www.oxfordlearnersdictionaries.com/definition/english/", "-"));
            dictView.Add(new DictionaryInfo("Collins English", "https://www.collinsdictionary.com/dictionary/english/", "-"));
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
        }

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
        var targetUriString = dict.BaseUrl + searchWords + dict.Suffix;

        Uri? targetUri;

        if (Uri.TryCreate(targetUriString, UriKind.Absolute, out targetUri))
        {
            if(CurrentWebViewSource != null)
            {
                backStack.Push(CurrentWebViewSource);
                backButton.IsEnabled = true;
            }

            webView.Source = targetUri;

            CurrentWebViewSource = targetUri;

            forwardStack.Clear();
            forwardButton.IsEnabled = false;
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
        forwardStack.Push(CurrentWebViewSource);

        var targetUri = backStack.Pop();
        
        webView.Source = targetUri;

        CurrentWebViewSource = targetUri;
        
        forwardButton.IsEnabled = true;

        if(backStack.Count == 0)
        {
            backButton.IsEnabled = false;
        }
    }

    private void Forward_Clicked(object sender, EventArgs e)
    {
        backStack.Push(CurrentWebViewSource);

        var targetUri = forwardStack.Pop();

        webView.Source = targetUri;
        
        CurrentWebViewSource = targetUri;

        backButton.IsEnabled = true;

        if (forwardStack.Count == 0)
        {
            forwardButton.IsEnabled = false;
        }
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

