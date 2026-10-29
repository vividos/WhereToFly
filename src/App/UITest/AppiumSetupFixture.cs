using Plugin.Maui.UITestHelpers.Appium;

namespace WhereToFly.App.UITest;

/// <summary>
/// Initializes Appium and starts server (the same as running "appium" command
/// in an npm console).
/// </summary>
public class AppiumSetupFixture : UITestContextSetupFixture
{
    /// <summary>
    /// Appium server context; set when running tests
    /// </summary>
    private AppiumServerContext? appiumServerContext;

    /// <summary>
    /// Initialize Appium server; called once for all test inside the same
    /// namespace in this assembly.
    /// </summary>
    public override void Initialize()
    {
        this.appiumServerContext = new AppiumServerContext();
        this.appiumServerContext.CreateAndStartServer();
        _serverContext = this.appiumServerContext;
    }
}
