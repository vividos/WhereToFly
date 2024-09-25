using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.MapView;
using WhereToFly.App.Services;

namespace WhereToFly.App.UnitTest.MapView
{
    /// <summary>
    /// Unit tests for <see cref="MapView"/>
    /// </summary>
    [TestClass]
    public class MapViewTest : UserInterfaceTestBase
    {
        /// <summary>
        /// Tests CreateAsync() method
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        [Timeout(10000)]
        [Ignore("CreateAsync() can't set MapInitializedTask")]
        public async Task TestMapViewCreate()
        {
            // set up
            var mapView = new WhereToFly.App.MapView.MapView();

            // run
            await mapView.CreateAsync(
                Constants.InitialCenterPoint,
                5000,
                true);

            // check
            Assert.IsTrue(
                mapView.MapInitializedTask.IsCompleted,
                "map initialized task must be completed");
        }

        /// <summary>
        /// Tests setting map view settings properties
        /// </summary>
        [TestMethod]
        public void TestMapViewSettings()
        {
            // set up
            var mapView = new WhereToFly.App.MapView.MapView();

            Assert.IsTrue(mapView.UseEntityClustering, "initial settings value must be correct");

            // run
            mapView.MapImageryType = MapImageryType.BingMapsAerialWithLabels;
            mapView.MapOverlayType = MapOverlayType.ContourLines;
            mapView.MapShadingMode = MapShadingMode.Fixed10Am;
            mapView.UseEntityClustering = false;

            // check
            Assert.AreEqual(MapShadingMode.Fixed10Am, mapView.MapShadingMode, "settings value must be correct");
            Assert.AreEqual(MapOverlayType.ContourLines, mapView.MapOverlayType, "settings value must be correct");
            Assert.AreEqual(MapImageryType.BingMapsAerialWithLabels, mapView.MapImageryType, "settings value must be correct");
            Assert.IsFalse(mapView.UseEntityClustering, "settings value must be correct");
        }

        /// <summary>
        /// Tests all Location methods of MapView
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        [SuppressMessage(
            "Blocker Code Smell",
            "S2699:Tests should include assertions",
            Justification = "There's no way to test actions")]
        [Timeout(10000)]
        [Ignore("disabled since calling AddLocationList() would block indefinitely")]
        public async Task TestMapViewLocationMethods()
        {
            // set up
            var mapView = new WhereToFly.App.MapView.MapView();

            await mapView.CreateAsync(
                Constants.InitialCenterPoint,
                5000,
                true);

            var locationList = DataServiceHelper.GetDefaultLocationList();

            // run
            await mapView.AddLocationList(locationList);

            var location = locationList.First();
            mapView.UpdateLocation(location);
            mapView.ZoomToLocation(location.MapLocation);
            mapView.RemoveLocation(location.Id);

            mapView.ClearLocationList();
        }

        /// <summary>
        /// Tests all Track methods of MapView
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        [SuppressMessage(
            "Blocker Code Smell",
            "S2699:Tests should include assertions",
            Justification = "There's no way to test actions")]
        [Timeout(10000)]
        [Ignore("disabled since calling AddTrack() would block indefinitely")]
        public async Task TestMapViewTrackMethods()
        {
            // set up
            var mapView = new WhereToFly.App.MapView.MapView();

            await mapView.CreateAsync(
                Constants.InitialCenterPoint,
                5000,
                true);

            var track = UnitTestHelper.GetDefaultTrack();

            // run
            await mapView.AddTrack(track);
            ////mapView.SampleTrackHeights(track);
            mapView.ZoomToTrack(track);
            mapView.RemoveTrack(track);
            mapView.ClearAllTracks();
        }

        /// <summary>
        /// Tests all Layer methods of MapView
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        [SuppressMessage(
            "Blocker Code Smell",
            "S2699:Tests should include assertions",
            Justification = "There's no way to test actions")]
        [Timeout(10000)]
        [Ignore("disabled since calling AddLayer() would block indefinitely")]
        public async Task TestMapViewLayerMethods()
        {
            // set up
            var mapView = new WhereToFly.App.MapView.MapView();

            await mapView.CreateAsync(
                Constants.InitialCenterPoint,
                5000,
                true);

            var layer = (await DataServiceHelper.GetInitialLayerList()).First();

            // run
            await mapView.AddLayer(layer);
            mapView.SetLayerVisibility(layer);
            ////mapView.ExportLayerAsync(layer);
            mapView.ZoomToLayer(layer);
            mapView.RemoveLayer(layer);
            mapView.ClearLayerList();
        }

        /// <summary>
        /// Tests deserializing MapView.AddFindResultParameter from JSON
        /// </summary>
        [TestMethod]
        public void TestDeserialize_AddFindResultParameter()
        {
            // set up
            string jsonParameters = "{ name: 'find result', latitude: 48.2, longitude: 11.8 }";

            // run
            var parameters = JsonConvert.DeserializeObject<AddFindResultParameter>(jsonParameters);

            // check
            Assert.IsNotNull(parameters, "returned parameters must be non-null");
            Assert.AreEqual("find result", parameters.Name, "deserialized name must match");
            Assert.AreEqual(48.2, parameters.Latitude, 1e-6, "deserialized latitude must match");
            Assert.AreEqual(11.8, parameters.Longitude, 1e-6, "deserialized longitude must match");
        }
    }
}
