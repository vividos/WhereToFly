using Plugin.Maui.UITestHelpers.Appium;
using Plugin.Maui.UITestHelpers.Core;

namespace WhereToFly.App.UITest;

/// <summary>
/// Map page; using the UI test Page Object Pattern
/// </summary>
internal class MapPage : PageBase
{
    /// <summary>
    /// Creates a new map page from IApp instance that just has started up
    /// </summary>
    /// <param name="app">app under test</param>
    public MapPage(IApp app)
        : base(app)
    {
        this.WaitForCompletePage();
    }

    /// <summary>
    /// Waits for page to complete loading
    /// </summary>
    private void WaitForCompletePage()
    {
        this.App.WaitForElement("MapView");
    }

    /// <summary>
    /// Opens page and returns new page object
    /// </summary>
    /// <param name="page">page to open</param>
    /// <returns>new page object</returns>
    public PageBase OpenPage(PageToOpen page)
    {
        this.App.Tap("AppCompatImageButton");

        // the PageToOpen enum has the same values as the AutomationId
        // attributes on the menu plus a suffix
        this.App.Tap($"{page}Page");

        this.App.WaitForElement($"{page}Page");

        return new PageBase(this.App);
    }
}
