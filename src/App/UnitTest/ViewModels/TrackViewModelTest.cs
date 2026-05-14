using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.App.ViewModels;
using WhereToFly.Geo;

namespace WhereToFly.App.UnitTest.ViewModels;

/// <summary>
/// Unit tests for view model classs TrackDetailsViewModel, TrackStatisticsViewModel and
/// TrackTabViewModel.
/// </summary>
[TestClass]
public class TrackViewModelTest : UserInterfaceTestBase
{
    /// <summary>
    /// Tests default ctor of TrackTabViewModel
    /// </summary>
    [TestMethod]
    public void TestTrackTabViewModel()
    {
        // set up
        var track = UnitTestHelper.GetDefaultTrack();
        track.CalculateStatistics();

        // run
        var viewModel = new TrackTabViewModel(track);

        // check
        Assert.IsNotNull(viewModel.ZoomToTrackCommand, "zoom command must not be null");
        Assert.IsNotNull(viewModel.DeleteTrackCommand, "delete command must not be null");

        viewModel.ZoomToTrackCommand.Execute(null);
        viewModel.DeleteTrackCommand.Execute(null);
    }

    /// <summary>
    /// Tests default ctor of TrackDetailsViewModel
    /// </summary>
    [TestMethod]
    public void TestTrackDetailsViewModel()
    {
        // set up
        var track = UnitTestHelper.GetDefaultTrack();
        track.CalculateStatistics();

        // run
        var viewModel = new TrackDetailsViewModel(track);

        // check
        Assert.IsNotEmpty(viewModel.Name, "name must contain value");
        Assert.IsNotNull(viewModel.TypeImageSource, "type image source must not be null");
    }

    /// <summary>
    /// Tests default ctor of TrackStatisticsViewModel
    /// </summary>
    [TestMethod]
    public void TestDefaultCtor()
    {
        // set up
        var track = UnitTestHelper.GetDefaultTrack();
        track.CalculateStatistics();

        // run
        var viewModel = new TrackStatisticsViewModel(track);

        // check
        Assert.IsGreaterThan(0, viewModel.NumTrackPoints, "there must be some track points");
        Assert.IsNotEmpty(viewModel.Distance, "distance must contain value");
        Assert.IsNotEmpty(viewModel.Duration, "duration must contain value");

        Assert.IsNotEmpty(viewModel.HeightGain, "height gain must contain value");
        Assert.IsNotEmpty(viewModel.HeightLoss, "height loss must contain value");
        Assert.IsNotEmpty(viewModel.MaxHeight, "max. height must contain value");
        Assert.IsNotEmpty(viewModel.MinHeight, "min. height must contain value");
        Assert.IsNotEmpty(viewModel.MaxClimbRate, "max. climb rate must contain value");
        Assert.IsNotEmpty(viewModel.MaxSinkRate, "max. sink rate must contain value");
        Assert.IsNotEmpty(viewModel.MaxSpeed, "max. speed must contain value");
        Assert.IsNotEmpty(viewModel.AverageSpeed, "average speed must contain value");
    }
}
