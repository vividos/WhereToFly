using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WhereToFly.Geo.Airspace;
using WhereToFly.Geo.DataFormats;

namespace WhereToFly.App.UnitTest.Geo
{
    /// <summary>
    /// Tests CzmlAirspaceWriter class
    /// </summary>
    [TestClass]
    public class CzmlAirspaceWriterTest
    {
        /// <summary>
        /// Tests writing a circle airspace to CZML
        /// </summary>
        [TestMethod]
        public void TestWriteCzmlCircle()
        {
            var airspacesList = new List<Airspace>
            {
                new Airspace(AirspaceClass.C)
                {
                    Name = "MyAirspace",
                    AirspaceType = "RMZ",
                    CallSign = "AA",
                    Frequency = "123.000",
                    Floor = new Altitude(AltitudeType.GND),
                    Ceiling = new Altitude(200.0, AltitudeType.AGL),
                    Color = "ff0000",
                    Geometry = new Circle
                    {
                        Center = new Coord { Latitude = 48.137155, Longitude = 11.575416 },
                        Radius = 200.0
                    },
                },
            };

            // run
            string czml = CzmlAirspaceWriter.WriteCzml("my airspaces", airspacesList, Enumerable.Empty<string>());

            // check
            Debug.WriteLine("CZML = " + czml);

            Assert.IsTrue(czml.Contains("MyAirspace"), "CZML must contain generated airspace");
            Assert.IsTrue(czml.Contains("cylinder"), "CZML must contain a cylinder element");
        }

        /// <summary>
        /// Tests converting an airspace file to CZML
        /// </summary>
        [TestMethod]
        public void TestConvertAirspacesToCzml()
        {
            // set up
            string airspacesFolder = Path.Combine(UnitTestHelper.TestAssetsPath, "airspaces");

            string filename = Path.Combine(airspacesFolder, "xcontest-switzerland.txt");
            Debug.WriteLine("parsing OpenAir file: " + filename);
            using (var stream = new FileStream(filename, FileMode.Open))
            {
                var parser = new OpenAirFileParser(stream);

                // run
                string czml = CzmlAirspaceWriter.WriteCzml(
                    Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(filename)),
                    parser.Airspaces,
                    new string[] { "Description line", "Another line" });

                // check
                Debug.WriteLine("CZML = " + czml);

                Assert.IsTrue(czml.Contains("GRUYERES"), "CZML must contain Gruyeres airspace");
            }
        }
    }
}
