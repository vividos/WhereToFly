using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.ViewModels;

namespace WhereToFly.App.UnitTest.ViewModels;

/// <summary>
/// Unit tests for class TrackListEntryViewModel
/// </summary>
[TestClass]
public class TrackListEntryViewModelTest : UserInterfaceTestBase
{
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
        Assert.IsNotEmpty(viewModel.Name, "name must contain value");
        Assert.IsNotEmpty(viewModel.DetailInfos, "detail infos must contain value");
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
