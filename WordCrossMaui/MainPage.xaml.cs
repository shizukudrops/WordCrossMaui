using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;

namespace WordCrossMaui;

[QueryProperty(nameof(ReceivedNewDictionaries), "NewDictionaries")]
[QueryProperty(nameof(DictView), "UpdatedDictView")]
public partial class MainPage : ContentPage
{
    readonly string pathToDictionary = Path.Join(FileSystem.AppDataDirectory, "dic");

    readonly Stack<Uri> backStack = new Stack<Uri>();
    readonly Stack<Uri> forwardStack = new Stack<Uri>();

    public Uri? CurrentWebViewSource { get; set; }

    readonly ObservableCollection<DictionaryInfo> _dictView = new ObservableCollection<DictionaryInfo>();

    public ObservableCollection<DictionaryInfo> DictView
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

            File.WriteAllText(pathToDictionary, JsonSerializer.Serialize(_dictView));

            if (Preferences.Get("sync_with_dropbox", false))
            {
                SyncWithDropbox();
            }

            OnPropertyChanged();
        }
    }

    //クエリ受信用
    public List<DictionaryInfo> ReceivedNewDictionaries
    {
        set
        {
            if (value == null) return;

            DictView = new ObservableCollection<DictionaryInfo>(DictView.Concat(value));
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
                DictView = deserialized;
            }
        }
        else
        {
            DictView = new ObservableCollection<DictionaryInfo>(PresetDictionaries.DictionaryList.Where(d => d.IsDefault));
        }

        dictList.ItemsSource = DictView;
    }

    private async void Search(DictionaryInfo dict, string input)
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
            if (CurrentWebViewSource != null)
            {
                backStack.Push(CurrentWebViewSource);
                backButton.IsEnabled = true;
            }

            webView.Source = targetUri;

            CurrentWebViewSource = targetUri;

            forwardStack.Clear();

            await Task.Yield();
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

    private async void Back_Clicked(object sender, EventArgs e)
    {
        forwardStack.Push(CurrentWebViewSource);

        var targetUri = backStack.Pop();

        webView.Source = targetUri;

        CurrentWebViewSource = targetUri;

        await Task.Yield();
        forwardButton.IsEnabled = true;

        if (backStack.Count == 0)
        {
            backButton.IsEnabled = false;
        }
    }

    private async void Forward_Clicked(object sender, EventArgs e)
    {
        backStack.Push(CurrentWebViewSource);

        var targetUri = forwardStack.Pop();

        webView.Source = targetUri;

        CurrentWebViewSource = targetUri;

        await Task.Yield();
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
            {"CurrentDictView", DictView}
        };

        await Shell.Current.GoToAsync("///ManageDictionary", param);
    }

    private async void About_Clicked(object sender, EventArgs e)
    {
        var param = new Dictionary<string, object>
        {
            {"CurrentDictView", DictView}
        };

        await Shell.Current.GoToAsync("///About", param);
    }

    private async Task SyncWithDropbox()
    {
        try
        {
            var client = new DropboxInterop();

            //辞書が存在するか確認
            if (!await client.IsFileExist("", "dic"))
            {
                //存在しなければアップロードする
                await client.Upload("/dic", JsonSerializer.Serialize(DictView));
            }
            else
            {
                if (Preferences.Get("is_sync_successful", false))
                {
                    await client.Upload("/dic", JsonSerializer.Serialize(DictView));
                }
                else
                {
                    if (await DisplayAlert("確認", "クラウドに別のバージョンの辞書リストが存在します。クラウドのデータで現在のリストを置き換えますか？", "はい、置き換えます", "いいえ、ローカルを保持します"))
                    {
                        //「はい」ならクラウドからダウンロード
                        var dic = await client.Download("/dic");
                        if (dic != null)
                        {
                            var deserialized = JsonSerializer.Deserialize<ObservableCollection<DictionaryInfo>>(dic);
                            if (deserialized != null)
                            {
                                DictView = deserialized;
                            }
                        }
                    }
                    else
                    {
                        //「いいえ」ならローカルをアップロード
                        await client.Upload("/dic", JsonSerializer.Serialize(DictView));
                    }
                }
            }
            Preferences.Default.Set("is_sync_successful", true);
        }
        catch(Exception e)
        {
            Debug.Write(e);
            Preferences.Default.Set("is_sync_successful", false);
        }
    }
}

