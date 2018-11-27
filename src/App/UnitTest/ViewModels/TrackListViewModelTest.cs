using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.Core;
using WhereToFly.App.Core.ViewModels;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class TrackListViewModel
    /// </summary>
    [TestClass]
    public class TrackListViewModelTest
    {
        /// <summary>
        /// Sets up tests by initializing Xamarin.Forms.Mocks and DependencyService with
        /// DataService.
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
            DependencyService.Register<IPlatform, UnitTestPlatform>();
        }

        /// <summary>
        /// Tests default ctor of view model
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // run
            var viewModel = new TrackListViewModel();

            // check
            Assert.IsTrue(viewModel.IsListEmpty, "list must be empty");
            Assert.IsFalse(viewModel.IsListRefreshActive, "refresh must not be active");

            // TODO
            Assert.IsNotNull(viewModel.ItemTappedCommand, "item tapped command must not be null");
            Assert.IsNotNull(viewModel.ImportTrackCommand, "import track command must not be null");
            Assert.IsNotNull(viewModel.DeleteTrackListCommand, "delete track list command must not be null");

            viewModel.ItemTappedCommand.Execute(null);
            viewModel.ImportTrackCommand.Execute(null);
            viewModel.DeleteTrackListCommand.Execute(null);
        }
    }
}
