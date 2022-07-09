using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                WaypointIdList = new List<string>
                {
                    "wheretofly-path-bahnhof-neuhaus",
                    "wheretofly-path-rauhkopf",
                },
            };

            var tour = engine.PlanTour(planTourParameters);

            // check
            Assert.IsTrue(tour.Description.Any(), "description must contain text");
            Assert.IsTrue(tour.TotalDuration.TotalMinutes > 0, "total duration must contain value");
            Assert.IsTrue(tour.TourEntriesList.Any(), "tour entries list must be filled");
            Assert.IsTrue(tour.MapPointList.Any(), "map point list must be filled");
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
                WaypointIdList = new List<string>
                {
                    "wheretofly-path-bahnhof-neuhaus",
                },
            };

            Assert.ThrowsException<InvalidOperationException>(
                () => engine.PlanTour(planTourParameters));
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
                WaypointIdList = new List<string>
                {
                    "wheretofly-path-bahnhof-neuhaus",
                    "wheretofly-path-xyz123",
                },
            };

            Assert.ThrowsException<ArgumentException>(
                () => engine.PlanTour(planTourParameters));
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
                WaypointIdList = new List<string>
                {
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
                },
            };

            var tour = engine.PlanTour(planTourParameters);

            // check
            Assert.IsTrue(tour.Description.Any(), "description must contain text");
            Assert.IsTrue(tour.TotalDuration.TotalMinutes > 0, "total duration must contain value");
            Assert.IsTrue(tour.TourEntriesList.Any(), "tour entries list must be filled");
            Assert.IsTrue(tour.MapPointList.Any(), "map point list must be filled");
        }
    }
}
