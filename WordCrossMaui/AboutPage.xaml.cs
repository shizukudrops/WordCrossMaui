namespace WordCrossMaui;

public partial class AboutPage : ContentPage
{
	public AboutPage()
	{
		InitializeComponent();

        string name = AppInfo.Current.Name;
        string version = AppInfo.Current.VersionString;

		AppNameLabel.Text = name;
		VersionLabel.Text = version;
		AuthorLabel.Text = "Author: shizukudrops";
    }

	private async void Return_Clicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync("///Main");
	}

    void Initialize_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
		Preferences.Set("initialize_on_next_launch", e.Value);
    }
}