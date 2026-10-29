using NUnit.Framework;
using Plugin.Maui.UITestHelpers.Appium;
using Plugin.Maui.UITestHelpers.Core;

namespace WhereToFly.App.UITest;

/// <summary>
/// User Interface tests for the WhereToFly app
/// </summary>
[TestFixture]
public class UserInterfaceTests : BaseTest
{
    /// <summary>
    /// A quick smoke test that the app starts up on the device; takes a screenshot as proof.
    /// </summary>
    [Test]
    public void AppStartsUp()
    {
        // setup, run and check
        Assert.That(
            this.App,
            Is.Not.Null,
            "app must have been initialized");

        var mapPage = new MapPage(this.App);

        Assert.That(
            mapPage,
            Is.Not.Null,
            "map page must not be null");

        this.App.Screenshot($"{nameof(this.AppStartsUp)}.png");
    }

    /// <summary>
    /// A test that visits all pages by using the menu navigation
    /// </summary>
    /// <returns>task to wait on</returns>
    [Test]
    public async Task VisitAllPages()
    {
        Assert.That(
            this.App,
            Is.Not.Null,
            "app must have been initialized");

        if (this.App == null)
        {
            return;
        }

        // wait for map
        var mapPage = new MapPage(this.App);

        PageToOpen[] pagesToOpenList =
        [
            PageToOpen.Info,
            PageToOpen.Settings,
            PageToOpen.LayerList,
            PageToOpen.LocationList,
            PageToOpen.TrackList,
            PageToOpen.CurrentPositionDetails,
        ];

        foreach (PageToOpen pageToOpen in pagesToOpenList)
        {
            var newPage = mapPage.OpenPage(pageToOpen);

            Assert.That(
                newPage,
                Is.Not.Null,
                "newly opened page must not be null");

            this.App.WaitForNoElement("MapView");

            await Task.Delay(100);

            var fileInfo = this.App.Screenshot($"Page: {pageToOpen}");
            Assert.That(
                fileInfo,
                Is.Not.Null,
                "screenshot file info must not be null");

            this.App.Back();
        }
    }
}
