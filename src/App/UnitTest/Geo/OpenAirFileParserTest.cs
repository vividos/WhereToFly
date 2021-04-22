using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using WhereToFly.Geo.Airspace;

namespace WhereToFly.App.UnitTest.Geo
{
    /// <summary>
    /// Tests for OpenAirFileParser class
    /// </summary>
    [TestClass]
    public class OpenAirFileParserTest
    {
        /// <summary>
        /// Tests parsing by using all files in the Assets folder
        /// </summary>
        [TestMethod]
        public void TestParseFiles()
        {
            // set up
            string airspacesFolder = Path.Combine(UnitTestHelper.TestAssetsPath, "airspaces");

            foreach (string filename in Directory.GetFiles(airspacesFolder))
            {
                Debug.WriteLine("parsing OpenAir file: " + filename);
                using (var stream = new FileStream(filename, FileMode.Open))
                {
                    // run
                    var parser = new OpenAirFileParser(stream);

                    // check
                    Assert.IsTrue(parser.Airspaces.Any(), "there must be some airspaces in the file");
                    Assert.IsFalse(parser.ParsingErrors.Any(), "there must be no parsing errors");

                    foreach (Airspace airspace in parser.Airspaces)
                    {
                        string airspaceText = airspace.ToString();
                        Debug.WriteLine($"checking airspace: {airspaceText})");

                        Assert.IsTrue(airspace.Class != AirspaceClass.Unknown, "airspace class must not be Unknown");
                        Assert.IsNotNull(airspace.Name, "airspace name must not be null");
                        Assert.IsNotNull(airspace.Floor, "airspace floor must not be null");
                        Assert.IsNotNull(airspace.Ceiling, "airspace ceiling must not be null");
                        Assert.IsNotNull(airspace.Geometry, "airspace geometry must be set");
                    }
                }
            }
        }

        /// <summary>
        /// Parses all variants of altitude values
        /// </summary>
        [TestMethod]
        public void TestParseAltitudeVariants()
        {
            // set up
            string[] altitudeVariants = new string[]
            {
                "GND",
                "12959 ft",
                "3950 m",
                "FL 180",
                "1600ft",
                "SFC",
                "UNLIM",
                "0",
                "100 Agl",
                "2000 GND",
                "UNLIM (Mon-Fri)",
                "UNLIMITED",
                "FL180",
                "500 ft agl",
                "Ask on 122.8",
                "3000 MSL",
            };

            foreach (var altitude in altitudeVariants)
            {
                string openairText = $"AC C\nAN UnitTest\nAL GND\nAH {altitude}\nV X=52:23:00 N 005:50:00 E\nDC 5";

                using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(openairText)))
                {
                    // run
                    var parser = new OpenAirFileParser(stream);

                    // check
                    Assert.IsTrue(parser.Airspaces.Any(), "there must be some airspaces in the file");

                    var ceilingType = parser.Airspaces.First().Ceiling.Type;
                    Debug.WriteLine($"altitude text {altitude} resulted in type {ceilingType}");

                    Assert.IsFalse(parser.ParsingErrors.Any(), "there must be no parsing errors");
                }
            }
        }

        /// <summary>
        /// Parses all variants of coordinates values
        /// </summary>
        [TestMethod]
        public void TestParseCoordinateVariants()
        {
            // set up
            string[] coordinateVariants = new string[]
            {
                "52:23:00 N 005:50:00 E",
                "52:21.30 S 005:52.30 W",
                "52:21:30.123 S 005:52:30.456 W",
            };

            foreach (var coordinate in coordinateVariants)
            {
                string openairText = $"AC C\nAN UnitTest\nAL GND\nAH UNLIMITED\nV X={coordinate}\nDC 5";

                using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(openairText)))
                {
                    // run
                    var parser = new OpenAirFileParser(stream);

                    // check
                    Assert.AreEqual(1, parser.Airspaces.Count(), "there must be one airspace in the file");

                    Assert.IsFalse(parser.ParsingErrors.Any(), "there must be no parsing errors");

                    var circle = parser.Airspaces.First().Geometry as Circle;
                    Assert.IsNotNull(circle.Center, "circle center must not be null");

                    Debug.WriteLine($"coordinates {coordinate} was parsed to: {circle.Center.Latitude}/{circle.Center.Longitude}");
                }
            }
        }

        /// <summary>
        /// Tests parsing error commands
        /// </summary>
        [TestMethod]
        public void TestParseCommandErrors()
        {
            // set up
            string coord = "52:21:30.123 S 005:52:30.456 W";

            string[] errorCommands = new string[]
            {
                "AN Text", // text without AC
                "AL Text", // floor without AC
                "AH Text", // ceiling without AC
                "XYabcText", // command syntax error
                "XY Text", // unknown command
                "AC C\nAL GND\nAH FL xyz", // invalid FL
                "AC C\nAL GND\nAL UNLIMITED\nAC D", // AC without AN
                "AC C\nAC D", // double AC
                "AC C\nAN Text\nAN Text2", // double AN

                $"AC C\nV X={coord}\nDC 5\nDC 5", // double DC
                "AC C\nV XYZ", // invalid variable definition
                $"AC C\nV X={coord}\nDC abc", // invalid DC radius
                "AC C\nV X=abc123\nDC 5", // invalid DC center
                "AC C\nV X=52_21.30.123 N 005_52.30.456 W\nDC 5", // invalid DC center

                $"AC C\nV X={coord}\nV D=abc\nDA 10,270,290", // invalid D variable
                $"AC C\nV X={coord}\nDA 10,270,290\nDA 123,45", // invalid DA format
                $"AC C\nV X={coord}\nDA xyz,270,290", // invalid DA format
                $"AC C\nV X={coord}\nDA abc,de", // invalid DA format
                "AC C\nV X=abc123\nDA 10,270,290", // invalid DA center

                $"AC C\nV X=abc123\nDB {coord},{coord}", // invalid DB center
                $"AC C\nV X={coord}\nDB {coord},{coord},{coord}", // invalid number of DB elements
                $"AC C\nV X={coord}\nDB abc,def", // invalid DB element
                $"AC C\nV X={coord}\nDC 5\nDA 10,270,290", // DC followed by DA
                $"V X={coord}\nDA 10,270,290", // DA without AC

                "SP 1,2,3,4,5", // SP without AC
                "SB 1,2,3", // SB without AC
                "AC C\nSP 1,2,3,4", // SP and wrong number of args
                "AC C\nSB 1,2,3,4", // SB and wrong number of args
            };

            foreach (var openairText in errorCommands)
            {
                Debug.WriteLine("trying to parse: " + openairText);

                using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(openairText)))
                {
                    // run
                    var parser = new OpenAirFileParser(stream);

                    // check
                    Assert.IsTrue(parser.ParsingErrors.Any(), "there must be parsing errors");
                }
            }
        }

        /// <summary>
        /// Parses opening times stored in altitude values
        /// </summary>
        [TestMethod]
        public void TestParseOpeningTimes()
        {
            // set up
            string[] openingTimesVariants = new string[]
            {
                "UNLIM (Mon-Fri)",
                "UNLIM (Mon-Fri) ",
                "UNLIM (on midnight)",
            };

            foreach (var openingTimes in openingTimesVariants)
            {
                string openairText = $"AC C\nAN UnitTest\nAL GND\nAH {openingTimes}\nV X=52:23:00 N 005:50:00 E\nDC 5";

                using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(openairText)))
                {
                    // run
                    var parser = new OpenAirFileParser(stream);

                    // check
                    Assert.IsFalse(parser.ParsingErrors.Any(), "there must be no parsing errors");
                    Assert.IsTrue(parser.Airspaces.Any(), "there must be some airspaces in the file");

                    var ceiling = parser.Airspaces.First().Ceiling;
                    Debug.WriteLine($"altitude text {openingTimes} resulted in type {ceiling.Type} and opening times {ceiling.OpeningTimes}");

                    Assert.IsTrue(ceiling.Type != AltitudeType.Textual, "ceiling must not be a textual value");

                    Assert.IsTrue(ceiling.OpeningTimes.Any(), "there must be an opening times text");
                }
            }
        }
    }
}
