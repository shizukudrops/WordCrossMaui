using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace WordCrossMaui;

[QueryProperty(nameof(ReceivedDictView), "CurrentDictView")]
public partial class ManageDictionaryPage : ContentPage
{
    readonly ObservableCollection<DictionaryViewModel> dictionaryList = new ObservableCollection<DictionaryViewModel>();

    ReadOnlyCollection<DictionaryViewModel>? initialDictionaries;

    //クエリ受信用
    public ObservableCollection<DictionaryViewModel> ReceivedDictView
    {
        set
        {
            if (value == null) return;

            //値が渡ってきたときにinitialDictonariesにコピーする（変更の巻き戻し用）
            initialDictionaries = new ReadOnlyCollection<DictionaryViewModel>(value.ToList());

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
        dictList.ItemTemplate = new DataTemplate(() =>
        {
            var label = new Label();
            label.SetBinding(Label.TextProperty, new Binding("Name"));

            var layout = new StackLayout()
            {
                Children =
                {
                    label
                },
                Margin = new Thickness(-24, 0, 4, 0),
                Padding = new Thickness(40, 8, 8, 8)
            };
            layout.SetBinding(BackgroundColorProperty, new Binding("BackgroundColor"));

            return layout;
        });
    }

    private void dictList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        foreach (var d in e.CurrentSelection)
        {
            ((DictionaryViewModel)d).Highlight();
        }

        foreach (var d in dictionaryList.Where(d => !e.CurrentSelection.Contains(d)))
        {
            d.UnHighlight();
        }
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
            dictionaryList.Remove((DictionaryViewModel)d);
        }
    }

    private void Revert_Clicked(object sender, EventArgs e)
    {
        if (initialDictionaries == null) return;

        dictionaryList.Clear();

        foreach(var d in initialDictionaries)
        {
            dictionaryList.Add(new DictionaryViewModel(d));
        }
    }

    private async void Return_Clicked(object sender, EventArgs e)
    {
        var param = new Dictionary<string, object>
        {
            {"UpdatedDictView",  new ObservableCollection<DictionaryViewModel>(dictionaryList.Select(d => new DictionaryViewModel(d)))}
        };

        await Shell.Current.GoToAsync("///Main", param);
    }
}