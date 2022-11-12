namespace WordCrossMaui;

public partial class AddDictionaryPage : ContentPage
{
	public AddDictionaryPage()
	{
		InitializeComponent();
	}

    private async void Add_Clicked(object sender, EventArgs e)
    {
        //名前が空欄でないかを判定
        if (string.IsNullOrWhiteSpace(nameBox.Text))
        {
            await DisplayAlert("名前が空です", "名前を入力してください", "OK");
            return;
        }

        //有効なURIかを判定
        if (!Uri.IsWellFormedUriString(urlBox.Text, UriKind.Absolute))
        {
            await DisplayAlert("無効なURLです", "正しい基本URLを入力してください", "OK");
            return;
        }

        var param = new Dictionary<string, object>
        {
            {"NewDictionaries",  new List<DictionaryViewModel>{ new DictionaryViewModel(nameBox.Text, urlBox.Text, separatorBox.Text, suffixBox.Text) } }
        };

        ClearTextBox();

        await Shell.Current.GoToAsync("///Main", param);
    }

    private async void Add_From_Preset_Clicked(object sender, EventArgs e)
    {
        ClearTextBox();

        await Shell.Current.GoToAsync("///AddFromPreset");
    }

    private async void Cancel_Clicked(object sender, EventArgs e)
    {
        ClearTextBox();

        await Shell.Current.GoToAsync("///Main");
    }

    private void ClearTextBox()
    {
        nameBox.Text = "";
        urlBox.Text = "";
        separatorBox.Text = "";
        suffixBox.Text = "";
    }
}