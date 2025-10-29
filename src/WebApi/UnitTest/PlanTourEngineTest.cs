using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using WhereToFly.Shared.Model;
using WhereToFly.WebApi.Logic.TourPlanning;

namespace WhereToFly.WebApi.UnitTest
{
    /// <summary>
    /// Tests for PlanTourEngine class
    /// </summary>
    [TestClass]
    public class PlanTourEngineTest
    {
        /// <summary>
        /// Loads PlanTourPaths.kml as stream for use in tests
        /// </summary>
        /// <returns>stream of kml asset file</returns>
        private static Stream GetPlanTourKmlStream()
        {
            var logicAssembly = typeof(PlanTourEngine).Assembly;
            var kmlStream = logicAssembly.GetManifestResourceStream("WhereToFly.WebApi.Logic.Assets.PlanTourPaths.kml");

            if (kmlStream == null)
            {
                throw new InvalidOperationException("PlanTourPaths.kml is not available");
            }

            return kmlStream;
        }

        /// <summary>
        /// Test for loading tracks and waypoints
        /// </summary>
        [TestMethod]
        public void TestLoadingTracksAndWaypoints()
        {
            // set up
            var engine = new PlanTourEngine();

            // run
            var kmlStream = GetPlanTourKmlStream();

            var graphLoader = new TourGraphLoader(engine);
            graphLoader.Load(kmlStream);

            // check
            Assert.IsNotNull(engine.FindWaypointInfo("wheretofly-path-rauhkopf"), "waypoint info must be found after loading");
        }

        /// <summary>
        /// Tests planning a tour with two points
        /// </summary>
        [TestMethod]
        public void TestPlanTour_TwoPoints()
        {
            // set up
            var engine = new PlanTourEngine();
            engine.LoadGraph(GetPlanTourKmlStream());

            // run
            var planTourParameters = new PlanTourParameters
            {
                WaypointIdList =
                [
                    "wheretofly-path-bahnhof-neuhaus",
                    "wheretofly-path-rauhkopf",
                ],
            };

            var tour = engine.PlanTour(planTourParameters);

            // check
            Assert.IsGreaterThan(0, tour.Description.Length, "description must contain text");
            Assert.IsGreaterThan(0, tour.TotalDuration.TotalMinutes, "total duration must contain value");
            Assert.IsNotEmpty(tour.TourEntriesList, "tour entries list must be filled");
            Assert.IsNotEmpty(tour.MapPointList, "map point list must be filled");
        }

        /// <summary>
        /// Tests planning a tour with only one point - should throw an exception
        /// </summary>
        [TestMethod]
        public void TestPlanTour_OnePoint()
        {
            // set up
            var engine = new PlanTourEngine();
            engine.LoadGraph(GetPlanTourKmlStream());

            // run
            var planTourParameters = new PlanTourParameters
            {
                WaypointIdList =
                [
                    "wheretofly-path-bahnhof-neuhaus",
                ],
            };

            Assert.ThrowsExactly<InvalidOperationException>(
                () => engine.PlanTour(planTourParameters),
                "must throw invalid operation exception");
        }

        /// <summary>
        /// Tests planning a tour with invalid waypoint ID - should throw an exception
        /// </summary>
        [TestMethod]
        public void TestPlanTour_InvalidWaypointId()
        {
            // set up
            var engine = new PlanTourEngine();
            engine.LoadGraph(GetPlanTourKmlStream());

            // run
            var planTourParameters = new PlanTourParameters
            {
                WaypointIdList =
                [
                    "wheretofly-path-bahnhof-neuhaus",
                    "wheretofly-path-xyz123",
                ],
            };

            Assert.ThrowsExactly<ArgumentException>(
                () => engine.PlanTour(planTourParameters),
                "must throw argument exception");
        }

        /// <summary>
        /// Tests planning a grand tour with all summits in the graph
        /// </summary>
        [TestMethod]
        public void TestPlanTour_GrandTour()
        {
            // set up
            var engine = new PlanTourEngine();
            engine.LoadGraph(GetPlanTourKmlStream());

            // run
            var planTourParameters = new PlanTourParameters
            {
                WaypointIdList =
                [
                    "wheretofly-path-bahnhof-neuhaus",
                    "wheretofly-path-spitzingsattel",
                    "wheretofly-path-jagerkamp",
                    ////"wheretofly-path-benzingspitz",
                    "wheretofly-path-tanzeck",
                    "wheretofly-path-aiplspitz",
                    "wheretofly-path-rauhkopf",
                    "wheretofly-path-taubensteinhaus",
                    "wheretofly-path-hochmiesing",
                    "wheretofly-path-rotwand",
                    "wheretofly-path-rotwandhaus",
                    "wheretofly-path-lempersberg",
                    "wheretofly-path-taubenstein",
                    ////"wheretofly-path-albert-link-haus",
                    ////"wheretofly-path-stolzenberg",
                    ////"wheretofly-path-rotkopf",
                    ////"wheretofly-path-rosskopf",
                    ////"wheretofly-path-stuempfling",
                    ////"wheretofly-path-bodenschneid",
                    ////"wheretofly-path-brecherspitz",
                    "wheretofly-path-bahnhof-neuhaus",
                ],
            };

            var tour = engine.PlanTour(planTourParameters);

            // check
            Assert.IsGreaterThan(0, tour.Description.Length, "description must contain text");
            Assert.IsGreaterThan(0, tour.TotalDuration.TotalMinutes, "total duration must contain value");
            Assert.IsNotEmpty(tour.TourEntriesList, "tour entries list must be filled");
            Assert.IsNotEmpty(tour.MapPointList, "map point list must be filled");
        }
    }
}
