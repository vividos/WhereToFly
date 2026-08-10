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
        return new Window(new MainPage());
    }
}
