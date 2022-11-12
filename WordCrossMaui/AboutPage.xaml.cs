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
		AuthorLabel.Text = "���: Shizukudrops";

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

            //�A�N�Z�X�L�[�����łɂ��邩
            if (string.IsNullOrEmpty(Preferences.Get("dropbox_access_token", "")))
            {
                //�A�N�Z�X�L�[���Ȃ���΂܂��F�؂���
                if (await client.Authenticate())
                {
                    //�F�ؐ����̏ꍇ�A����
                    await SyncDictionaryWithDropbox(client);
                }
                else
                {
                    //�F�؎��s�̏ꍇ
                    await DisplayAlert("�G���[", "Dropbox�Ƃ̐ڑ��Ɏ��s���܂���", "OK");
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
        //���������݂��邩�m�F
        if (!await client.IsFileExist("", "dic"))
        {
            //���݂��Ȃ���΃A�b�v���[�h����
            await client.Upload("/dic", JsonSerializer.Serialize(new Archive(ReceivedDictView)));
        }
        else
        {
            //�N���E�h�Ɏ��������݂���ꍇ�A�N���E�h�ƃ��[�J���̂ǂ������邩�m�F
            if (await DisplayAlert("�m�F", "���łɃN���E�h�Ɏ������X�g�����݂��܂��B�N���E�h�̃f�[�^�Ō��݂̃��X�g��u�������܂����H", "�͂��A�u�������܂�", "�������A���[�J����ێ����܂�"))
            {
                //�u�͂��v�Ȃ�N���E�h����_�E�����[�h
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
                //�u�������v�Ȃ烍�[�J�����A�b�v���[�h
                await client.Upload("/dic", JsonSerializer.Serialize(new Archive(ReceivedDictView)));
            }
        }

        return true;
    }
}