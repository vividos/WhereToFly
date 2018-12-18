using NUnit.Framework;
using System.Linq;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace WhereToFly.App.UITest
{
    /// <summary>
    /// User Interface tests for WhereToFly app
    /// </summary>
    [TestFixture(Platform.Android)]
    ////[TestFixture(Platform.iOS)]
    public class UserInterfaceTests
    {
        /// <summary>
        /// Platform to run test for
        /// </summary>
        private readonly Platform platform;

        /// <summary>
        /// App under test
        /// </summary>
        private IApp app;

        /// <summary>
        /// Creates a new UI test object
        /// </summary>
        /// <param name="platform">platform to use</param>
        public UserInterfaceTests(Platform platform)
        {
            this.platform = platform;
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
        /// A quick smoke test that the app starts up on the device.
        /// </summary>
        [Test]
        public void AppStartsUp()
        {
            AppResult[] results = this.app.WaitForElement(c => c.Marked("ExploreMapWebView"));
            this.app.Screenshot("Map screen.");

            Assert.IsTrue(results.Any());
        }
    }
}
