using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.Core.ViewModels;
using WhereToFly.Logic.Model;

namespace WhereToFly.UnitTest.ViewModels
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
