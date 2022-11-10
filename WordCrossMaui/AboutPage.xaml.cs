namespace WordCrossMaui;

public partial class AboutPage : ContentPage
{
	public AboutPage()
	{
		InitializeComponent();

        dropboxSyncSwitch.IsToggled = Preferences.Get("sync_with_dropbox", false);

        string name = AppInfo.Current.Name;
        string version = AppInfo.Current.VersionString;

		AppNameLabel.Text = name;
		VersionLabel.Text = version;
		AuthorLabel.Text = "çÏé“: Shizukudrops";

#if DEBUG
		debugPanel.IsVisible = true;
#endif
	}

    async void OnDropboxSyncToggled(object sender, ToggledEventArgs e)
    {
        Preferences.Set("sync_with_dropbox", e.Value);

        if (e.Value)
        {
            if (string.IsNullOrEmpty(Preferences.Get("dropbox_access_token", "")))
            {
                var client = new DropboxInterop();

                if (await client.Authenticate())
                {
                    await client.Download();
                    await client.Upload();
                }
                else
                {
                    await DisplayAlert("ÉGÉâÅ[", "DropboxÇ∆ÇÃê⁄ë±Ç…é∏îsÇµÇ‹ÇµÇΩ", "OK");
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
		await Shell.Current.GoToAsync("///Main");
	}

    void Initialize_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
		Preferences.Set("initialize_on_next_launch", e.Value);
    }

    private async void DirButton_Clicked(object sender, EventArgs e)
    {
        await Clipboard.Default.SetTextAsync(FileSystem.AppDataDirectory);
    }
}