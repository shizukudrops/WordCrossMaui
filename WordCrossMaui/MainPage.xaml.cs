using HtmlAgilityPack;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace WordCrossMaui;

[QueryProperty(nameof(ReceivedNewDictionaries), "NewDictionaries")]
[QueryProperty(nameof(UpdatedDictView), "UpdatedDictView")]
public partial class MainPage : ContentPage
{
    readonly ObservableCollection<DictionaryViewModel> _dictView = new ObservableCollection<DictionaryViewModel>();

    public ObservableCollection<DictionaryViewModel> DictView
    {
        get => _dictView;
        set
        {
            if(value == null) return;
            
            _dictView.Clear();

            foreach(var d in value)
            {
                _dictView.Add(d);
            }

            OnPropertyChanged();
        }
    }

    //クエリ受信用
    public ObservableCollection<DictionaryViewModel> UpdatedDictView
    {
        set
        {
            if(value == null) return;


            File.WriteAllText(Env.PathToDictionary, JsonSerializer.Serialize(new Archive(value)));

            if (Preferences.Get("sync_with_dropbox", false))
            {
                SyncWithDropbox();
            }
            else
            {
                DictView = value;
            }
        }
    }

    public List<DictionaryViewModel> ReceivedNewDictionaries
    {
        set
        {
            if (value == null) return;

            UpdatedDictView = new ObservableCollection<DictionaryViewModel>(DictView.Concat(value));
        }
    }

    private string MessageOnStartPage { get; set; } = "";
    
    public MainPage()
	{
		InitializeComponent();

        BindingContext = this;

        //辞書リストをロード
        if (Preferences.Get("sync_with_dropbox", false))
        {
            SyncWithDropbox();
        }
        else
        {
            if (File.Exists(Env.PathToDictionary))
            {
                LoadLocal();
            }
            else
            {
                LoadDefault();
            }
        }

        dictList.ItemsSource = DictView;
        dictList.ItemTemplate = new DataTemplate(typeof(MyViewCell));

        SetStartPage();

        backButton.SetBinding(IsEnabledProperty, new Binding("CanGoBack", BindingMode.OneWay, source: webView));
        forwardButton.SetBinding(IsEnabledProperty, new Binding("CanGoForward", BindingMode.OneWay, source: webView));
    }

    private async void SetStartPage()
    {
        using (var stream = await FileSystem.OpenAppPackageFileAsync("StartPage.html"))
        {
            var reader = new StreamReader(stream, Encoding.UTF8);

            var html = new HtmlDocument();
            html.Load(reader);

            var div = html.DocumentNode.SelectSingleNode(@"//div[@id=""append_area""]");

            var newNodeString = $"<p>{MessageOnStartPage}</p>";
            var newNode = HtmlNode.CreateNode(newNodeString);
            div.AppendChild(newNode);

            //ファイルとしてwebviewに読み込ませないと戻るボタンがバグる
            var target = Path.Join(FileSystem.AppDataDirectory, "StartPage.html");
            File.WriteAllText(target, html.DocumentNode.OuterHtml);
            webView.Source = target;
        }
    }

    private async void SyncWithDropbox()
    {
        var client = new DropboxInterop();
        var (result, archive) = await client.Sync();

        if(result == SyncResult.CloudDownload)
        {
            UpdatedDictView = new ObservableCollection<DictionaryViewModel>(Extensions.Convert(archive.Dictionaries));
        }
        else if(result == SyncResult.LocalUpload)
        {
            DictView = new ObservableCollection<DictionaryViewModel>(Extensions.Convert(archive.Dictionaries));
        }
        else if(result == SyncResult.Fail)
        {
            if (File.Exists(Env.PathToDictionary))
            {
                LoadLocal();
            }
            else
            {
                LoadDefault();
            }
        }
    }

    private void LoadDefault()
    {
        UpdatedDictView = new ObservableCollection<DictionaryViewModel>(Extensions.Convert(PresetDictionaries.DictionaryList.Where(d => d.IsDefault)));
    }

    private void LoadLocal()
    {
        try
        {
            var rawData = File.ReadAllText(Env.PathToDictionary);

            var deserialized = JsonSerializer.Deserialize<Archive>(rawData);

            if (deserialized != null)
            {
                DictView = new ObservableCollection<DictionaryViewModel>(Extensions.Convert(deserialized.Dictionaries));
            }
        }
        catch (Exception e)
        {
            Debug.Write(e);
            LoadDefault();
        }
    }

    private void Search(DictionaryInfo dict, string input)
    {
        //どの辞書も選ばれていなかったら一番上の辞書で検索する。辞書が存在しなければ戻る。
        if (dict == null)
        {
            if (DictView.Count > 0) dict = DictView.First();
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
            webView.Source = targetUri;
        }
    }

    private void SearchBox_Completed(object sender, EventArgs e)
    {
        Search((DictionaryInfo)dictList.SelectedItem, ((Entry)sender).Text);
    }

    private void DictList_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        var selected = e.SelectedItem as DictionaryViewModel;
        selected.Highlight();

        foreach (var d in DictView.Where(d => d != selected))
        {
            d.UnHighlight();
        }

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
            {"CurrentDictView", Extensions.Clone(DictView)}
        };

        await Shell.Current.GoToAsync("///ManageDictionary", param);
    }

    private async void About_Clicked(object sender, EventArgs e)
    {
        var param = new Dictionary<string, object>
        {
            {"CurrentDictView", Extensions.Clone(DictView)}
        };

        await Shell.Current.GoToAsync("///About", param);
    }
}

