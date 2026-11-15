using Plugin.Maui.UITestHelpers.Appium;
using Plugin.Maui.UITestHelpers.Core;

namespace WhereToFly.App.UITest.Pages;

/// <summary>
/// Settings page; using the UI test Page Object Pattern
/// </summary>
internal class SettingsPage : PageBase
{
    /// <summary>
    /// Creates a new settings page object
    /// </summary>
    /// <param name="app">app under test</param>
    internal SettingsPage(IApp app)
        : base(app)
    {
        this.App.WaitForElement("SettingsPage");
    }

    /// <summary>
    /// Navigates back to the map page
    /// </summary>
    public void Back()
    {
        this.App.Back();
    }
}
