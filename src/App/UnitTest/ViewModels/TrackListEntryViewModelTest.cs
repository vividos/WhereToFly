using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using WhereToFly.App.Core;
using WhereToFly.App.Core.Services.SqliteDatabase;
using WhereToFly.App.Core.ViewModels;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest.ViewModels
{
    /// <summary>
    /// Unit tests for class TrackListEntryViewModel
    /// </summary>
    [TestClass]
    public class TrackListEntryViewModelTest
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
            DependencyService.Register<IDataService, SqliteDatabaseDataService>();
            DependencyService.Register<SvgImageCache>();

            var imageCache = DependencyService.Get<SvgImageCache>();
            imageCache.AddImage("weblib/images/paragliding.svg", string.Empty);
            imageCache.AddImage("icons/map-marker-distance.svg", string.Empty);
        }

        /// <summary>
        /// Tests default ctor of view model
        /// </summary>
        [TestMethod]
        public void TestDefaultCtor()
        {
            // run
            var parentViewModel = new TrackListViewModel();
            var track = UnitTestHelper.GetDefaultTrack();
            var viewModel = new TrackListEntryViewModel(parentViewModel, track);

            // check
            Assert.IsTrue(viewModel.Name.Any(), "name must contain value");
            Assert.IsTrue(viewModel.DetailInfos.Any(), "detail infos must contain value");
            Assert.IsNotNull(viewModel.Track, "track must be not null");

            Assert.IsNotNull(viewModel.ShowTrackDetailsCommand, "show track details command must not be null");
            Assert.IsNotNull(viewModel.ZoomToTrackCommand, "zoom to track command must not be null");
            Assert.IsNotNull(viewModel.DeleteTrackCommand, "delete track command must not be null");
            Assert.IsNotNull(viewModel.TypeImageSource, "type image source must not be null");

            viewModel.ShowTrackDetailsCommand.Execute(null);
            viewModel.ZoomToTrackCommand.Execute(null);
            viewModel.DeleteTrackCommand.Execute(null);
        }
    }
}
