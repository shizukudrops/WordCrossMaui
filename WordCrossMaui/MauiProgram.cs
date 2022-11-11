using Microsoft.Maui.LifecycleEvents;

#if WINDOWS
using WinUIEx;
#endif

namespace WordCrossMaui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		if(Preferences.Get("client_id", null) == null)
		{
			Preferences.Default.Set("client_id", Guid.NewGuid().ToString());
		}

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureLifecycleEvents(events =>
			{
#if WINDOWS
				events.AddWindows(windows => windows
						.OnWindowCreated(window =>
						{
							//.NET 7が出たら正規の方法に変える
							window.SizeChanged += OnSizeChanged;

                            var windowPosX = Preferences.Get("window_pos_x", -1);
							var windowPosY = Preferences.Get("window_pos_y", -1);
							var windowWidth = Preferences.Get("window_width", 1024.0);
							var windowHeight = Preferences.Get("window_height", 720.0);

							if(windowPosX == -1 || windowPosY == -1)
							{
								window.CenterOnScreen(windowWidth, windowHeight);
							}
							else
							{
                                window.MoveAndResize(windowPosX, windowPosY, windowWidth, windowHeight);
                            }							
						})
						.OnClosed((window, args) =>
						{
							//ウィンドウ位置を保存
							var appWindow = window.GetAppWindow();
                            Preferences.Default.Set("window_pos_x", appWindow.Position.X);
                            Preferences.Default.Set("window_pos_y", appWindow.Position.Y);
                        })
				);
                
				static void OnSizeChanged(object sender, Microsoft.UI.Xaml.WindowSizeChangedEventArgs args)
                {
                    //ウィンドウサイズを保存
					Preferences.Default.Set("window_width", args.Size.Width + 14.5);
					Preferences.Default.Set("window_height", args.Size.Height + 8.0);
                }
#endif
            })
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		return builder.Build();
	}
}
