using System.Collections.ObjectModel;

namespace WordCrossMaui;

public partial class AddFromPresetPage : ContentPage
{
    ObservableCollection<DictionaryInfo> dictionaryList = new ObservableCollection<DictionaryInfo>(PresetDictionaries.DictionaryList);

    public AddFromPresetPage()
	{
		InitializeComponent();

        dictList.ItemsSource = dictionaryList;
	}

    private async void Add_Clicked(object sender, EventArgs e)
    {
        var selected = new List<DictionaryInfo>();

        if (dictList.SelectedItems != null)
        {
            foreach(var d in dictList.SelectedItems)
            {
                selected.Add((DictionaryInfo)d);
            }
        }

        var param = new Dictionary<string, object>
        {
            {"NewDictionaries", selected}
        };

        ResetDictionaryList();

        await Shell.Current.GoToAsync("///Main", param);
    }

    private async void Return_Clicked(object sender, EventArgs e)
    {
        dictList.SelectedItems = null;

        ResetDictionaryList();

        await Shell.Current.GoToAsync("///Main");
    }

    private void ResetDictionaryList()
    {
        dictionaryList.Clear();

        foreach (var d in PresetDictionaries.DictionaryList)
        {
            dictionaryList.Add(d);
        }
    }
}