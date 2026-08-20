using WhereToFly.App.Weather.Pages;

namespace WhereToFly.App.Weather;

/// <summary>
/// WhereToFly weather app
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Creates a new app object
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Called when the app's window is about to be created.
    /// </summary>
    /// <param name="activationState">activation state</param>
    /// <returns>window object</returns>
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var titleBar = DeviceInfo.Platform == DevicePlatform.WinUI
            ? new TitleBar
            {
                Title = Constants.AppTitle,
                BackgroundColor = Constants.PrimaryColor,
                ForegroundColor = Colors.White,
            }
            : null;

        return new Window
        {
            Title = Constants.AppTitle,
            TitleBar = titleBar,
            Page = new NavigationPage(
                new WeatherDashboardPage()),
        };
    }
}
