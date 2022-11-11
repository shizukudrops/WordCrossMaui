﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace WordCrossMaui;

[QueryProperty(nameof(ReceivedNewDictionaries), "NewDictionaries")]
[QueryProperty(nameof(DictView), "UpdatedDictView")]
public partial class MainPage : ContentPage
{
    readonly string pathToDictionary = Path.Join(FileSystem.AppDataDirectory, "dic");

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

        SetStartPage();

        backButton.SetBinding(IsEnabledProperty, new Binding("CanGoBack", BindingMode.OneWay, source: webView));
        forwardButton.SetBinding(IsEnabledProperty, new Binding("CanGoForward", BindingMode.OneWay, source: webView));
    }

    private async void SetStartPage()
    {
        using (var stream = await FileSystem.OpenAppPackageFileAsync("StartPage.html"))
        {
            var reader = new StreamReader(stream, Encoding.UTF8);

            //ファイルとしてwebviewに読み込ませないと戻るボタンがバグる
            var target = Path.Join(FileSystem.AppDataDirectory, "StartPage.html");
            File.WriteAllText(target, reader.ReadToEnd());

            webView.Source = target;
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

