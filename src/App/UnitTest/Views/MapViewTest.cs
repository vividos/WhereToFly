using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.Core;
using WhereToFly.App.Core.Services;
using WhereToFly.App.MapView;
using Xamarin.Forms;

namespace WhereToFly.App.UnitTest.Views
{
    /// <summary>
    /// Unit tests for MapView
    /// </summary>
    [TestClass]
    public class MapViewTest
    {
        /// <summary>
        /// Sets up tests by initializing Xamarin.Forms.Mocks
        /// </summary>
        [TestInitialize]
        public void SetUp()
        {
            Xamarin.Forms.Mocks.MockForms.Init();
            DependencyService.Register<IPlatform, UnitTestPlatform>();
        }

        /// <summary>
        /// Tests CreateAsync() method
        /// </summary>
        /// <returns>task to wait on</returns>
        [TestMethod]
        [Ignore("CreateAsync() can't set MapInitializedTask")]
        [Timeout(1000)]
        public async Task TestMapViewCreate()
        {
            // set up
            var mapView = new MapView.MapView();

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
            var mapView = new MapView.MapView();

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
        [TestMethod]
        [SuppressMessage(
            "Blocker Code Smell",
            "S2699:Tests should include assertions",
            Justification = "There's no way to test actions")]
        public void TestMapViewLocationMethods()
        {
            // set up
            var mapView = new MapView.MapView();
            var locationList = DataServiceHelper.GetDefaultLocationList();

            // run
            mapView.AddLocationList(locationList);

            var location = locationList.First();
            mapView.UpdateLocation(location);
            mapView.ZoomToLocation(location.MapLocation);
            mapView.RemoveLocation(location.Id);

            mapView.ClearLocationList();
        }

        /// <summary>
        /// Tests all Track methods of MapView
        /// </summary>
        [TestMethod]
        [SuppressMessage(
            "Blocker Code Smell",
            "S2699:Tests should include assertions",
            Justification = "There's no way to test actions")]
        public void TestMapViewTrackMethods()
        {
            // set up
            var mapView = new MapView.MapView();
            var track = UnitTestHelper.GetDefaultTrack();

            // run
            mapView.AddTrack(track);
            ////mapView.SampleTrackHeights(track);
            mapView.ZoomToTrack(track);
            mapView.RemoveTrack(track);
            mapView.ClearAllTracks();
        }

        /// <summary>
        /// Tests all Layer methods of MapView
        /// </summary>
        [TestMethod]
        [SuppressMessage(
            "Blocker Code Smell",
            "S2699:Tests should include assertions",
            Justification = "There's no way to test actions")]
        public void TestMapViewLayerMethods()
        {
            // set up
            var mapView = new MapView.MapView();
            var layer = DataServiceHelper.GetInitialLayerList().First();

            // run
            mapView.AddLayer(layer);
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
            Assert.AreEqual("find result", parameters.Name, "deserialized name must match");
            Assert.AreEqual(48.2, parameters.Latitude, 1e-6, "deserialized latitude must match");
            Assert.AreEqual(11.8, parameters.Longitude, 1e-6, "deserialized longitude must match");
        }
    }
}
