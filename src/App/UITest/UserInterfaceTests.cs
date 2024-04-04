using NUnit.Framework;
using System.Linq;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace WhereToFly.App.UITest
{
    /// <summary>
    /// User Interface tests for the WhereToFly app
    /// </summary>
    [TestFixture(Platform.Android)]
    public class UserInterfaceTests
    {
        /// <summary>
        /// Platform to run test for
        /// </summary>
        private readonly Platform platform;

        /// <summary>
        /// App under test
        /// </summary>
        private IApp? app;

        /// <summary>
        /// Creates a new UI test object
        /// </summary>
        /// <param name="platform">platform to use</param>
        public UserInterfaceTests(Platform platform)
        {
            this.platform = platform;
        }

        /// <summary>
        /// Taps on the menu button, showing the menu from anywhere in the app
        /// </summary>
        private void TapMenuButton()
        {
            if (this.platform == Platform.Android)
            {
                this.app?.Tap(x => x.Class("AppCompatImageButton"));
            }
        }

        /// <summary>
        /// Sets up test by starting app
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.app = AppInitializer.StartApp(this.platform);
        }

        /// <summary>
        /// A quick smoke test that the app starts up on the device; takes a screenshot as proof.
        /// </summary>
        [Test]
        public void AppStartsUp()
        {
            AppResult[]? results = this.app?.WaitForElement(c => c.Marked("ExploreMapWebView"));
            this.app?.Screenshot("Map screen.");

            Assert.That(
                results != null && results.Any(),
                "map view must have been found");
        }

        /// <summary>
        /// A test that visits all pages by using the menu navigation
        /// </summary>
        [Test]
        public void VisitAllPages()
        {
            // wait for map
            this.app!.WaitForElement(c => c.Marked("ExploreMapWebView"));

            string[] markedList = new string[]
            {
                "Info",
                "Settings",
                "Layers",
                "Locations",
                "Tracks",
                "Current Position",
                "Weather",
                "Map",
            };

            foreach (string markedItem in markedList)
            {
                bool showsMap = markedItem == "Map";

                this.TapMenuButton();
                this.app.Tap(x => x.Marked(markedItem));

                if (!showsMap)
                {
                    this.app.WaitForNoElement(c => c.Marked("ExploreMapWebView"));
                }

#pragma warning disable S2925 // "Thread.Sleep" should not be used in tests
                System.Threading.Thread.Sleep(100);
#pragma warning restore S2925 // "Thread.Sleep" should not be used in tests

                var fileInfo = this.app.Screenshot($"Screen: {markedItem}");
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
}
