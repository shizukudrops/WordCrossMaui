namespace WordCrossMaui;

public partial class App : Application
{
    public App()
	{
		InitializeComponent();

        MainPage = new AppShell();

        AppSetup();
    }

    private void AppSetup()
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

        //ver1.9.2の辞書データを変換
        Updater.ConvertDictionaryType();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        Window window = base.CreateWindow(activationState);

        window.Title = "WordCross";

        //ウィンドウサイズと位置
        var windowPosX = Preferences.Get("window_pos_x", -1.0);
        var windowPosY = Preferences.Get("window_pos_y", -1.0);
        var windowWidth = Preferences.Get("window_width", 1024.0);
        var windowHeight = Preferences.Get("window_height", 720.0);

        if (windowPosX != -1.0 && windowPosY != -1.0)
        {
            window.X = windowPosX;
            window.Y = windowPosY;
        }

        window.Width = windowWidth;
        window.Height = windowHeight;

        window.SizeChanged += (s, e) =>
        {
            Preferences.Default.Set("window_width", window.Width);
            Preferences.Default.Set("window_height", window.Height);
        };

        window.Destroying += (s, e) =>
        {
            Preferences.Default.Set("window_pos_x", window.X);
            Preferences.Default.Set("window_pos_y", window.Y);
        };

        return window;
    }
}
