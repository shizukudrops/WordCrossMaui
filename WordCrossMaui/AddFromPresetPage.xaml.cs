using System.Collections.ObjectModel;

namespace WordCrossMaui;

public partial class AddFromPresetPage : ContentPage
{
    ObservableCollection<DictionaryViewModel> dictionaryList = new ObservableCollection<DictionaryViewModel>(PresetDictionaries.DictionaryList.Select(d => new DictionaryViewModel(d)));

    public AddFromPresetPage()
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

    private async void Add_Clicked(object sender, EventArgs e)
    {
        var selected = new List<DictionaryViewModel>();

        if (dictList.SelectedItems != null)
        {
            foreach(var d in dictList.SelectedItems)
            {
                selected.Add((DictionaryViewModel)d);
            }
        }

        var param = new Dictionary<string, object>
        {
            {"NewDictionaries", new List<DictionaryViewModel>(selected.Select(d => new DictionaryViewModel(d)))}
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
            dictionaryList.Add(new DictionaryViewModel(d));
        }
    }
}