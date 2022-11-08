using System.Collections.ObjectModel;

namespace WordCrossMaui;

[QueryProperty(nameof(DictionaryList), "DictionaryList")]
public partial class ManageDictionaryPage : ContentPage
{
    readonly ObservableCollection<DictionaryInfo> dictionaryList = new ObservableCollection<DictionaryInfo>();

    ReadOnlyCollection<DictionaryInfo>? initialDictionaries;

    public ObservableCollection<DictionaryInfo> DictionaryList
    {
        get => dictionaryList;
        set
        {
            if (value == null) return;

            //�ŏ��ɒl���n���Ă����Ƃ��̂�initialDictonaries�ɃR�s�[����i�ύX�̊����߂��p�j
            if(initialDictionaries == null)
            {
                initialDictionaries = new ReadOnlyCollection<DictionaryInfo>(value);
            }

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

    private void dictList_ReorderCompleted(object sender, EventArgs e)
    {
        dictList.SelectedItems = null;
    }

    private void Remove_Clicked(object sender, EventArgs e)
    {
        var selected = new List<object>(dictList.SelectedItems);

        foreach(var d in selected)
        {
            dictionaryList.Remove((DictionaryInfo)d);
        }
    }

    private void Revert_Clicked(object sender, EventArgs e)
    {
        if (initialDictionaries == null) return;

        dictionaryList.Clear();

        foreach(var d in initialDictionaries)
        {
            dictionaryList.Add(d);
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