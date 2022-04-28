using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WhereToFly.App.Core.Logic;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.UnitTest
{
    /// <summary>
    /// Tests class DataFormatter
    /// </summary>
    [TestClass]
    public class DataFormatterTest
    {
        /// <summary>
        /// Tests method FormatLatLong()
        /// </summary>
        [TestMethod]
        public void TestFormatLatLong()
        {
            // set up
            double latLong = 47.6764385;

            // run
            string text1 = GeoDataFormatter.FormatLatLong(latLong, CoordinateDisplayFormat.Format_dd_dddddd);
            string text2 = GeoDataFormatter.FormatLatLong(latLong, CoordinateDisplayFormat.Format_dd_mm_mmm);
            string text3 = GeoDataFormatter.FormatLatLong(latLong, CoordinateDisplayFormat.Format_dd_mm_sss);

            // check
            Assert.AreEqual("47.676439", text1, "formatted text must match");
            Assert.AreEqual("47° 40.586'", text2, "formatted text must match");
            Assert.AreEqual("47° 40' 35\"", text3, "formatted text must match");
        }

        /// <summary>
        /// Tests method FormatLatLong() with negative value
        /// </summary>
        [TestMethod]
        public void TestFormatNegativeLatLong()
        {
            // set up
            double latLong = -47.6764385;

            // run
            string text1 = GeoDataFormatter.FormatLatLong(latLong, CoordinateDisplayFormat.Format_dd_dddddd);
            string text2 = GeoDataFormatter.FormatLatLong(latLong, CoordinateDisplayFormat.Format_dd_mm_mmm);
            string text3 = GeoDataFormatter.FormatLatLong(latLong, CoordinateDisplayFormat.Format_dd_mm_sss);

            // check
            Assert.AreEqual("-47.676439", text1, "formatted text must match");
            Assert.AreEqual("-47° 40.586'", text2, "formatted text must match");
            Assert.AreEqual("-47° 40' 35\"", text3, "formatted text must match");
        }

        /// <summary>
        /// Tests method FormatMyPositionShareText()
        /// </summary>
        [TestMethod]
        public void TestFormatMyPositionShareText()
        {
            // set up
            var mapPoint = new MapPoint(47.6764385, 11.8710533, 1685.0);

            // run
            string text = DataFormatter.FormatMyPositionShareText(mapPoint, DateTimeOffset.UtcNow);

            // check
            Assert.IsTrue(text.Length > 0, "formatted text must not be empty");
        }

        /// <summary>
        /// Tests method FormatLocationShareText()
        /// </summary>
        [TestMethod]
        public void TestFormatLocationShareText()
        {
            // set up
            var location = UnitTestHelper.GetDefaultLocation();

            // run
            string text = DataFormatter.FormatLocationShareText(location);

            // check
            Assert.IsTrue(text.Length > 0, "formatted text must not be empty");
        }

        /// <summary>
        /// Tests method FormatDistance()
        /// </summary>
        [TestMethod]
        public void TestFormatDistance()
        {
            // set up
            double distance1 = 42.0;
            double distance2 = 4200.0;
            double distance3 = 0.0;

            // run
            string text1 = DataFormatter.FormatDistance(distance1);
            string text2 = DataFormatter.FormatDistance(distance2);
            string text3 = DataFormatter.FormatDistance(distance3);

            // check
            Assert.AreEqual("42 m", text1, "formatted text must match");

            string separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            string expectedText2 = $"4{separator}2 km";
            Assert.AreEqual(expectedText2, text2, "formatted text must match");

            Assert.AreEqual("-", text3, "formatted text must match");
        }

        /// <summary>
        /// Tests method FormatDuration()
        /// </summary>
        [TestMethod]
        public void TestFormatDuration()
        {
            // run
            string text1 = DataFormatter.FormatDuration(TimeSpan.Zero);
            string text2 = DataFormatter.FormatDuration(TimeSpan.FromHours(1.42));
            string text3 = DataFormatter.FormatDuration(TimeSpan.FromDays(1.42));

            // check
            Assert.AreEqual("0:00:00", text1, "formatted text must match");
            Assert.AreEqual("1:25:12", text2, "formatted text must match");
            Assert.AreEqual("1.10:04:48", text3, "formatted text must match");
        }
    }
}
