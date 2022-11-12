namespace WordCrossMaui;

public partial class AddDictionaryPage : ContentPage
{
	public AddDictionaryPage()
	{
		InitializeComponent();
	}

    private async void Add_Clicked(object sender, EventArgs e)
    {
        //���O���󗓂łȂ����𔻒�
        if (string.IsNullOrWhiteSpace(nameBox.Text))
        {
            await DisplayAlert("���O����ł�", "���O����͂��Ă�������", "OK");
            return;
        }

        //�L����URI���𔻒�
        if (!Uri.IsWellFormedUriString(urlBox.Text, UriKind.Absolute))
        {
            await DisplayAlert("������URL�ł�", "��������{URL����͂��Ă�������", "OK");
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