using Microsoft.Maui.LifecycleEvents;

namespace WordCrossMaui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
        //「次回起動時に初期化」のチェックがされていたなら辞書と設定をクリア
        if (Preferences.Get("initialize_on_next_launch", false))
        {
            Preferences.Clear();

            if (File.Exists(Env.PathToDictionary))
            {
                File.Delete(Env.PathToDictionary);
            }
        }

        //初回起動時のみIDを割り振る
        if (Preferences.Get("client_id", null) == null)
        {
            Preferences.Default.Set("client_id", Guid.NewGuid().ToString());
        }

        var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		return builder.Build();
	}
}
