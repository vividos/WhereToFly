using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using WhereToFly.App.Services.SqliteDatabase;
using WhereToFly.App.ViewModels;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class TrackListViewModel
    /// </summary>
    [TestClass]
    public class TrackListViewModelTest
    {
        /// <summary>
        /// Sets up tests
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            DependencyService.Register<IPlatform, UnitTestPlatform>();
            DependencyService.Register<IDataService, SqliteDatabaseDataService>();
        }

        /// <summary>
        /// Tests default ctor of view model
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // set up
            var viewModel = new TrackListViewModel();

            // run
            viewModel.WaitForPropertyChange(nameof(viewModel.TrackList), TimeSpan.FromSeconds(10));

            // check
            Assert.IsTrue(viewModel.IsListEmpty, "list must be empty");
            Assert.IsFalse(viewModel.IsListRefreshActive, "refresh must not be active");

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
            ////viewModel.ImportTrackCommand.Execute(null); // don't execute import; it opens a file picker
            await viewModel.DeleteTrackListCommand.ExecuteAsync();
        }
    }
}
