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
