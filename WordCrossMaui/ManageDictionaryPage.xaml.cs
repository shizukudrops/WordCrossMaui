using System.Collections.ObjectModel;

namespace WordCrossMaui;

[QueryProperty(nameof(ReceivedDictView), "CurrentDictView")]
public partial class ManageDictionaryPage : ContentPage
{
    readonly ObservableCollection<DictionaryInfo> dictionaryList = new ObservableCollection<DictionaryInfo>();

    ReadOnlyCollection<DictionaryInfo>? initialDictionaries;

    //クエリ受信用
    public ObservableCollection<DictionaryInfo> ReceivedDictView
    {
        set
        {
            if (value == null) return;

            //値が渡ってきたときにinitialDictonariesにコピーする（変更の巻き戻し用）
            initialDictionaries = new ReadOnlyCollection<DictionaryInfo>(value.ToList());

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

        dictList.ItemsSource = dictionaryList;        
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
            {"UpdatedDictView",  dictionaryList}
        };

        await Shell.Current.GoToAsync("///Main", param);
    }
}