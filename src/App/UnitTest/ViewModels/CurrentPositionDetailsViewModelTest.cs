using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.App.Logic.Model;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Tests CurrentPositionDetailsViewModel class
    /// </summary>
    [TestClass]
    public class CurrentPositionDetailsViewModelTest
    {
        /// <summary>
        /// Sets up tests by initializing Xamarin.Forms.Mocks
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
        }

        /// <summary>
        /// Tests ctor
        /// </summary>
        [TestMethod]
        public void TestCtor()
        {
            // set up
            var appSettings = new AppSettings();
            var viewModel = new CurrentPositionDetailsViewModel(appSettings);

            // check
            Assert.IsFalse(viewModel.IsHeadingAvail, "initially heading is not available");
        }
    }
}
