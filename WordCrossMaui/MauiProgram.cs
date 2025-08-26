using Microsoft.Maui.LifecycleEvents;

namespace WordCrossMaui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
        var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        //CollectionViewのMultipleSelection時のハイライトがバグっている問題の回避
        //https://github.com/dotnet/maui/issues/16066
#if WINDOWS

        Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper.AppendToMapping("DisableMultiselectCheckbox",
        (handler, view) =>
        {
            handler.PlatformView.IsMultiSelectCheckBoxEnabled = false;
        });

#endif

        return builder.Build();
	}
}
