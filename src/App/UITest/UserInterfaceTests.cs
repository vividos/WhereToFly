using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using Xamarin.UITest;

namespace WhereToFly.App.UITest;

/// <summary>
/// User Interface tests for the WhereToFly app
/// </summary>
[TestFixture]
public class UserInterfaceTests
{
    /// <summary>
    /// App under test
    /// </summary>
    private IApp? app;

    /// <summary>
    /// Sets up test by starting app
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        string apkFilename = Path.Combine(
            Path.GetDirectoryName(typeof(UserInterfaceTests).Assembly.Location)!,
            "de.vividos.app.wheretofly.android.apk");

        this.app =
            ConfigureApp.Android
            .ApkFile(apkFilename)
            .EnableLocalScreenshots()
            .StartApp();
    }

    /// <summary>
    /// A quick smoke test that the app starts up on the device; takes a screenshot as proof.
    /// </summary>
    [Test]
    public void AppStartsUp()
    {
        Assert.That(
            this.app,
            Is.Not.Null,
            "app must have been initialized");

        var mapPage = new MapPage(this.app!);

        Assert.That(
            mapPage,
            Is.Not.Null,
            "map page must not be null");

        this.app?.Screenshot("Map screen.");
    }

    /// <summary>
    /// A test that visits all pages by using the menu navigation
    /// </summary>
    /// <returns>task to wait on</returns>
    [Test]
    public async Task VisitAllPages()
    {
        Assert.That(
            this.app,
            Is.Not.Null,
            "app must have been initialized");

        if (this.app == null)
        {
            return;
        }

        // wait for map
        var mapPage = new MapPage(this.app);

        PageToOpen[] pagesToOpenList =
        [
            PageToOpen.Info,
            PageToOpen.Settings,
            PageToOpen.LayerList,
            PageToOpen.LocationList,
            PageToOpen.TrackList,
            PageToOpen.CurrentPositionDetails,
            PageToOpen.Map,
        ];

        foreach (PageToOpen pageToOpen in pagesToOpenList)
        {
            bool showsMap = pageToOpen == PageToOpen.Map;

            var newPage = mapPage.OpenPage(pageToOpen);

            Assert.That(
                newPage,
                Is.Not.Null,
                "newly opened page must not be null");

            if (!showsMap)
            {
                this.app.WaitForNoElement(c => c.Marked("ExploreMapWebView"));
            }

            await Task.Delay(100);

            var fileInfo = this.app.Screenshot($"Page: {pageToOpen}");
            Assert.That(
                fileInfo,
                Is.Not.Null,
                "screenshot file info must not be null");

            if (!showsMap)
            {
                this.app.Back();
            }
        }
    }
}
