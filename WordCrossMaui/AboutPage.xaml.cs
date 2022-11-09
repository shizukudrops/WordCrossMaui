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

    void OnDropboxSyncToggled(object sender, ToggledEventArgs e)
    {
        Preferences.Set("sync_with_dropbox", e.Value);

        if (e.Value)
        {
            var client = new DropboxInterlop();
            var result = client.Run();
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