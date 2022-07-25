using System.Collections.ObjectModel;

namespace WordCrossMaui;

[QueryProperty(nameof(DictionaryList), "DictionaryList")]
public partial class ManageDictionaryPage : ContentPage
{
    ObservableCollection<DictionaryInfo> dictionaryList = new ObservableCollection<DictionaryInfo>();

    public ObservableCollection<DictionaryInfo> DictionaryList
    {
        get => dictionaryList;
        set
        {
            if (value == null) return;

            dictionaryList.Clear();
            
            foreach(var d in value)
            {
                dictionaryList.Add(d);
            };

            OnPropertyChanged();
        }
    }

    public ManageDictionaryPage()
	{
		InitializeComponent();

        dictList.ItemsSource = DictionaryList;
	}

    private void Remove_Clicked(object sender, EventArgs e)
    {
        var selected = new List<object>(dictList.SelectedItems);

        foreach(var d in selected)
        {
            dictionaryList.Remove((DictionaryInfo)d);
        }
    }

    private async void Return_Clicked(object sender, EventArgs e)
    {
        var param = new Dictionary<string, object>
        {
            {"UpdatedDictionaryList",  dictionaryList}
        };

        await Shell.Current.GoToAsync("///Main", param);
    }
}