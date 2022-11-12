using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace WordCrossMaui;

[QueryProperty(nameof(ReceivedDictView), "CurrentDictView")]
public partial class AboutPage : ContentPage
{
    public ObservableCollection<DictionaryViewModel> ReceivedDictView { get; set; } = new ObservableCollection<DictionaryViewModel>();
    public ObservableCollection<DictionaryViewModel>? UpdatedDictView { get; set; }

    public AboutPage()
	{
		InitializeComponent();

        dropboxSyncSwitch.IsToggled = Preferences.Get("sync_with_dropbox", false);

        string name = AppInfo.Current.Name;
        string version = AppInfo.Current.VersionString;

		AppNameLabel.Text = name;
		VersionLabel.Text = version;
		AuthorLabel.Text = "作者: Shizukudrops";

#if DEBUG
		debugPanel.IsVisible = true;
#endif
	}

    async void OnDropboxSyncToggled(object sender, ToggledEventArgs e)
    {
        Preferences.Set("sync_with_dropbox", e.Value);

        if (e.Value)
        {  
            var client = new DropboxInterop();

            //アクセスキーがすでにあるか
            if (string.IsNullOrEmpty(Preferences.Get("dropbox_access_token", "")))
            {
                //アクセスキーがなければまず認証する
                if (await client.Authenticate())
                {
                    //認証成功の場合、同期
                    await SyncDictionaryWithDropbox(client);
                }
                else
                {
                    //認証失敗の場合
                    await DisplayAlert("エラー", "Dropboxとの接続に失敗しました", "OK");
                    dropboxSyncSwitch.IsToggled = false;
                }
            }
        }
        else
        {
            Preferences.Default.Remove("dropbox_access_token");
            Preferences.Default.Remove("dropbox_refresh_token");
        }
    }

    private async void Return_Clicked(object sender, EventArgs e)
	{
        if(UpdatedDictView!= null)
        {
            var param = new Dictionary<string, object>
        {
            {"UpdatedDictView",  new ObservableCollection<DictionaryInfo>(UpdatedDictView)}
        };

            await Shell.Current.GoToAsync("///Main", param);
            UpdatedDictView= null;
        }
        else
        {
            await Shell.Current.GoToAsync("///Main");
        }
	}

    void Initialize_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
		Preferences.Set("initialize_on_next_launch", e.Value);
    }

    private async void DirButton_Clicked(object sender, EventArgs e)
    {
        await Clipboard.Default.SetTextAsync(FileSystem.AppDataDirectory);
    }

    private async Task<bool> SyncDictionaryWithDropbox(DropboxInterop client)
    {
        //辞書が存在するか確認
        if (!await client.IsFileExist("", "dic"))
        {
            //存在しなければアップロードする
            await client.Upload("/dic", JsonSerializer.Serialize(new Archive(ReceivedDictView)));
        }
        else
        {
            //クラウドに辞書が存在する場合、クラウドとローカルのどちらを取るか確認
            if (await DisplayAlert("確認", "すでにクラウドに辞書リストが存在します。クラウドのデータで現在のリストを置き換えますか？", "はい、置き換えます", "いいえ、ローカルを保持します"))
            {
                //「はい」ならクラウドからダウンロード
                var dic = await client.Download("/dic");
                if (dic != null)
                {
                    var deserialized = JsonSerializer.Deserialize<Archive>(dic);
                    if (deserialized != null)
                    {
                        UpdatedDictView = new ObservableCollection<DictionaryViewModel>(deserialized.Dictionaries.Select(d => new DictionaryViewModel(d)));
                    }
                }
            }
            else
            {
                //「いいえ」ならローカルをアップロード
                await client.Upload("/dic", JsonSerializer.Serialize(new Archive(ReceivedDictView)));
            }
        }

        return true;
    }
}