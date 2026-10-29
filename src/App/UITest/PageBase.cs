using Plugin.Maui.UITestHelpers.Core;

namespace WhereToFly.App.UITest;

/// <summary>
/// UI test page base class; using the UI test Page Object Pattern
/// </summary>
internal class PageBase
{
    /// <summary>
    /// App under test
    /// </summary>
    protected IApp App { get; }

    /// <summary>
    /// Creates a new page base class object; call this from the page classes
    /// </summary>
    /// <param name="app">app under test</param>
    public PageBase(IApp app)
    {
        this.App = app;
    }
}
