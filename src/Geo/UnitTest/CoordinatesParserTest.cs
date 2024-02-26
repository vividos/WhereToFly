#nullable enable
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;

namespace WhereToFly.Geo.UnitTest
{
    /// <summary>
    /// Unit tests for GeoJSON converter classes
    /// </summary>
    [TestClass]
    public class CoordinatesParserTest
    {
        /// <summary>
        /// Tests parsing all valid coordinates text strings
        /// </summary>
        /// <param name="text">coordinates text to parse</param>
        [TestMethod]
        [DataRow("49.781231, 6.837581")] // takeoff Mehring, Mosel
        [DataRow("49.781231 6.837581")]
        [DataRow("49.781231,6.837581")]
        [DataRow("49.781231,   6.837581")]
        [DataRow("+49.781231 +6.837581")]
        [DataRow("+49.781231 -6.837581")]
        [DataRow("-49.781231 +6.837581")]
        [DataRow("-49.781231 -6.837581")]
        [DataRow("49°46'52.4\"N 6°50'15.3\"E")]
        [DataRow("49° 46′ 52.4″N 006° 50′ 15.3″E")]
        [DataRow("49° 46′ N, 06° 50′ O ")]
        [DataRow("49° 46′ S, 06° 50′ E")]
        [DataRow("49° 46', -6° 50'")]
        [DataRow("41°32'13\"N   109°32'59\"W")]
        [DataRow("49° 34′ 45.59″ N, 12° 17′ 13.85″ E")]
        [DataRow("49.579331°, 6.837581°")]
        [DataRow("49.579331°N 6.837581°E")]
        [DataRow("https://maps.google.com/maps?q=49.781231,6.837581")]
        [DataRow("https://www.google.com/maps/place/47%C2%B038'32.8%22N+12%C2%B002'33.2%22E/@47.642442,12.0399761,17z")]
        [DataRow("geo:49.781231,6.837581")]
        public void TestParseValidCoordinates(string text)
        {
            // run
            bool result = CoordinatesParser.TryParse(text, out MapPoint? mapPoint);

            // check
            Assert.IsTrue(result, "TryParse() must return true");
            Assert.IsNotNull(mapPoint, "map point must not be null");
            Assert.IsTrue(mapPoint.Valid, "map point must be valid");
        }

        /// <summary>
        /// Tests parsing all invalid coordinates text strings
        /// </summary>
        /// <param name="text">coordinates text to parse</param>
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("\t\t\n\r\t")]
        [DataRow("49,781231,6,837581")]
        [DataRow("https://maps.google.com/maps?abc=49.781231,6.837581")]
        [DataRow("geo:49.781231")]
        public void TestParseInvalidCoordinatesText(string? text)
        {
            // run
            bool result = CoordinatesParser.TryParse(text, out MapPoint? mapPoint);

            // check
            Assert.IsFalse(result, "TryParse() must return false");
            Assert.IsNull(mapPoint, "map point must be null");
        }
    }
}
