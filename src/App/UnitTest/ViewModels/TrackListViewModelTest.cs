using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
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

            Assert.IsNotNull(viewModel.ItemTappedCommand, "item tapped command must not be null");
            Assert.IsNotNull(viewModel.ImportTrackCommand, "import track command must not be null");
            Assert.IsNotNull(viewModel.DeleteTrackListCommand, "delete track list command must not be null");
        }

        /// <summary>
        /// Tests executing actions
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        [Ignore("this test would use NavigationService, which can't currently be mocked")]
        public async Task TestExecuteActions()
        {
            // run
            var viewModel = new TrackListViewModel();

            // check
            await viewModel.ItemTappedCommand.ExecuteAsync(null);
            ////viewModel.ImportTrackCommand.Execute(null); // don't execute import; it opens a file picker
            await viewModel.DeleteTrackListCommand.ExecuteAsync();
        }
    }
}
