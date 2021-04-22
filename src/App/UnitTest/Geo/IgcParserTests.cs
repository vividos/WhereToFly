using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WhereToFly.Geo.DataFormats;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.UnitTest.Geo
{
    /// <summary>
    /// Tests class IgcParser
    /// </summary>
    [TestClass]
    public class IgcParserTests
    {
        /// <summary>
        /// Tests parsing an IGC file to a track
        /// </summary>
        [TestMethod]
        public void TestParseIgcFile()
        {
            // set up
            string filename = Path.Combine(UnitTestHelper.TestAssetsPath, "85QA3ET1.igc");

            // run
            Track track = null;
            IEnumerable<string> parsingErrors;
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                var igcParser = new IgcParser(stream);
                track = igcParser.Track;
                parsingErrors = igcParser.ParsingErrors;
            }

            // check
            Assert.IsNotNull(track, "track must not be null");
            Assert.IsNotNull(track.Name, "track name must be set");
            Assert.IsTrue(track.TrackPoints.Any(), "there must be any track points");
            Assert.IsFalse(parsingErrors.Any(), "there must be no parsing errors");
        }

        /// <summary>
        /// Tests parsing the start date record
        /// </summary>
        [TestMethod]
        public void TestParseStartDate()
        {
            // set up
            var expectedDate = new DateTime(2021, 2, 7);
            var lines = new string[]
            {
                "HFDTE070221",
                "HFDTE 070221",
                "HFDTEDATE:070221",
                "HFDTEDATE:070221,01",
                "HFDTEDATE: 070221,01",
            };

            // run
            foreach (var lineToParse in lines)
            {
                string line = lineToParse + "\r\n" + "B0903544724034N01052829EA009300100900108000";

                Track track = null;
                using (var stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(line)))
                {
                    var igcParser = new IgcParser(stream);
                    track = igcParser.Track;
                }

                // check
                Assert.AreEqual(1, track.TrackPoints.Count, "there must be one track point");

                var trackPoint = track.TrackPoints[0];
                var date = trackPoint.Time.Value.Date;

                Assert.AreEqual(expectedDate, date);
            }
        }

        /// <summary>
        /// Tests parsing IGC file with an invalid B record
        /// </summary>
        [TestMethod]
        public void TestInvalidBRecordCoordinates()
        {
            // set up
            string line =
                "B1746454650892N01205373EV015050000076\n" +
                "B1746460000000N00000000EV014970000000\n" +
                "B1746470000000N00000000EV014880000000\n" +
                "B1746484650918N01205513EV014790000000\n";

            // run
            Track track = null;
            using (var stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(line)))
            {
                var igcParser = new IgcParser(stream);
                track = igcParser.Track;
            }

            // check
            Assert.IsNotNull(track, "track must not be null");
            Assert.AreEqual(2, track.TrackPoints.Count, "there only must be two track points");
        }
    }
}
