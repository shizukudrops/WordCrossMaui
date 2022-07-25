using Microsoft.Maui.LifecycleEvents;

#if WINDOWS
using WinUIEx;
#endif

namespace WordCrossMaui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureLifecycleEvents(events =>
			{
#if WINDOWS
				events.AddWindows(windows => windows
						.OnWindowCreated(window =>
						{
							window.SizeChanged += OnSizeChanged;

                            var windowPosX = Preferences.Get("window_pos_x", -1);
							var windowPosY = Preferences.Get("window_pos_y", -1);
							var windowWidth = Preferences.Get("window_width", 1024);
							var windowHeight = Preferences.Get("window_height", 720);

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
					Preferences.Default.Set("window_width", (int)args.Size.Width);
					Preferences.Default.Set("window_height", (int)args.Size.Height);
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
