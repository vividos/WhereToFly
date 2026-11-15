using Plugin.Maui.UITestHelpers.Appium;
using Plugin.Maui.UITestHelpers.Core;

namespace WhereToFly.App.UITest.Pages;

/// <summary>
/// Info page; using the UI test Page Object Pattern
/// </summary>
internal class InfoPage : PageBase
{
    /// <summary>
    /// Creates a new info page object
    /// </summary>
    /// <param name="app">app under test</param>
    internal InfoPage(IApp app)
        : base(app)
    {
        this.App.WaitForElement("InfoPage");
    }

    /// <summary>
    /// Navigates back to the map page
    /// </summary>
    public void Back()
    {
        this.App.Back();
    }
}
