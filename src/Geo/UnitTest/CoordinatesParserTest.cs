using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        [DataRow("49,781231,6,837581")]
        [DataRow("49°46'52.4\"N 6°50'15.3\"E")]
        [DataRow("N 49°46'52.4\", E 6°50'15.3\"")]
        [DataRow("49° 46′ 52.4″N 006° 50′ 15.3″E")]
        [DataRow("49° 46′ N, 06° 50′ O ")]
        [DataRow("49° 46′ S, 06° 50′ E")]
        [DataRow("49° 46', -6° 50'")]
        [DataRow("49° 46, -6° 50")]
        [DataRow("41°32'13\"N   109°32'59\"W")]
        [DataRow("49° 34′ 45.59″ N, 12° 17′ 13.85″ E")]
        [DataRow("49.579331°, 6.837581°")]
        [DataRow("49.579331°N 6.837581°E")]
        [DataRow("S 49° 46.74400' , E 172° 4.50000'  ")]
        [DataRow("S 49° 46' 44.6\" , E 172° 04' 30.0\" ")]
        [DataRow("49.7812 S; 172.83 E  ")]
        [DataRow("49.7812 S; 172.837 E")]
        [DataRow("49,7812120, 26,8375812")]
        [DataRow("49°34′ 45″ N, 12° 17′ 13″ O")]
        [DataRow("49°34′\u202f45″\u202fN, 12°\u202f17′\u202f13″\u202fO")] // U+202F: Narrow No-Break Space
        [DataRow("https://maps.google.com/maps?q=49.781231,6.837581")]
        [DataRow("https://www.google.com/maps/place/47%C2%B038'32.8%22N+12%C2%B002'33.2%22E/@47.642442,12.0399761,17z")]
        [DataRow("https://www.openstreetmap.org/#map=18/49.7812310/11.8375812")]
        [DataRow("geo:49.781231,6.837581")]
        [DataRow("geo:49.781231,11.837581")]
        [DataRow("geo:47.6,-122.3")]
        [DataRow("geo:47.6,-122.3?z=11")]
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
        [DataRow("49°46'52.4\"N 6°50'15.3\"N")]
        [DataRow("49°46'52.4\"E 6°50'15.3\"W")]
        [DataRow("128°46'52.4\"N 26°50'15.3\"E")] // north > 90° is invalid
        [DataRow("128°46'52.4\"S 26°50'15.3\"E")] // south > 90° is invalid
        [DataRow("109.781231°N,6.837581°E")]
        [DataRow("-109.781231°N,6.837581°E")]
        [DataRow("49° 46\" 52.4'N 006° 50\" 15.3'E")]
        [DataRow("https://maps.google.com/maps?abc=49.781231,6.837581")]
        [DataRow("https://www.openstreetmap.org/#abc=18/49.7812310/11.8375812")]
        [DataRow("https://www.openstreetmap.org/#map=18/49.7812310/11.8375812/42")]
        [DataRow("geo:49.781231")]
        [DataRow("geo:0,0?q=34.99,-106.61(Treasure)")]
        [DataRow("geogeo:0,0?q=1600+Amphitheatre+Parkway%2C+CA")]
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
