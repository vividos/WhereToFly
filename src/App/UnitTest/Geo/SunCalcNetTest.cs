using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using WhereToFly.Geo.SunCalcNet;

namespace WhereToFly.App.UnitTest.Geo
{
    /// <summary>
    /// Unit tests for the SunCalcNet library
    /// </summary>
    [TestClass]
    public class SunCalcNetTest
    {
        /// <summary>
        /// Checks if the time of day matches the given actual value
        /// </summary>
        /// <param name="actualValue">actual value to check</param>
        /// <param name="hour">hour of time of day</param>
        /// <param name="minute">minute of time of day</param>
        /// <returns>true when the time of day matches (with a margin of error)</returns>
        private static bool CheckMatchingTimeOfDay(DateTimeOffset actualValue, int hour, int minute)
        {
            var expectedValue = new DateTimeOffset(
                actualValue.Year,
                actualValue.Month,
                actualValue.Day,
                hour,
                minute,
                0,
                actualValue.Offset);

            var delta = actualValue - expectedValue;
            return Math.Abs(delta.Minutes) < 3.0;
        }

        /// <summary>
        /// Checks solar times of a positive latitude and longitude value.
        /// https://www.timeanddate.com/sun/@48.137222,11.575556?month=7&year=2020
        /// </summary>
        [TestMethod]
        public void TestSolarTimes_PositiveLatLong()
        {
            // set up
            double latitude = 48.137222; // munich
            double longitude = 11.575556;

            var date1 = new DateTimeOffset(2020, 7, 31, 12, 0, 0, TimeSpan.FromHours(2.0)); // MESZ
            var date2 = new DateTimeOffset(2020, 12, 5, 12, 0, 0, TimeSpan.FromHours(1.0)); // MEZ

            // run
            var times1 = SunCalc.GetTimes(date1, latitude, longitude);
            var times2 = SunCalc.GetTimes(date2, latitude, longitude);

            // check
            // 2020-07-31: 05:48 - 20:50
            Assert.IsTrue(times1.Sunrise.HasValue, "sunrise must have been set");
            Assert.IsTrue(CheckMatchingTimeOfDay(times1.Sunrise.Value, 5, 48), "sunrise time must match");

            Assert.IsTrue(times1.Sunset.HasValue, "sunset must have been set");
            Assert.IsTrue(CheckMatchingTimeOfDay(times1.Sunset.Value, 20, 50), "sunset time must match");

            // 2020-12-05: 07:48 - 16:20
            Assert.IsTrue(times2.Sunrise.HasValue, "sunrise must have been set");
            Assert.IsTrue(CheckMatchingTimeOfDay(times2.Sunrise.Value, 7, 48), "sunrise time must match");

            Assert.IsTrue(times2.Sunset.HasValue, "sunset must have been set");
            Assert.IsTrue(CheckMatchingTimeOfDay(times2.Sunset.Value, 16, 20), "sunset time must match");
        }

        /// <summary>
        /// Checks specifying a relative height above the horizon
        /// </summary>
        [TestMethod]
        public void TestSolarTimes_HeightAboveHorizon()
        {
            // set up
            double latitude = 48.137222;
            double longitude = 11.575556;
            var date = new DateTimeOffset(2020, 7, 31, 12, 0, 0, TimeSpan.FromHours(2.0));

            // run
            var times1 = SunCalc.GetTimes(date, latitude, longitude);
            var times2 = SunCalc.GetTimes(date, latitude, longitude, 100.0);

            // check
            TimeSpan deltaSunrise = times2.Sunrise.Value - times1.Sunrise.Value;
            TimeSpan deltaSunset = times2.Sunset.Value - times1.Sunset.Value;

            Assert.AreEqual(-2.3, deltaSunrise.TotalMinutes, 0.1, "sunrise 100m above must be 2 minutes earlier");
            Assert.AreEqual(2.3, deltaSunset.TotalMinutes, 0.1, "sunset 100m above must be 2 minutes later");
        }

        /// <summary>
        /// Checks solar times before and after a timezone change
        /// </summary>
        [TestMethod]
        public void TestSolarTimes_BeforeAndAfterTimezoneChange()
        {
            // set up
            double latitude = 48.137222; // munich
            double longitude = 11.575556;

            var date1 = new DateTimeOffset(2020, 10, 24, 12, 0, 0, TimeSpan.FromHours(2.0)); // MESZ
            var date2 = new DateTimeOffset(2020, 10, 25, 12, 0, 0, TimeSpan.FromHours(1.0)); // MEZ

            // run
            var times1 = SunCalc.GetTimes(date1, latitude, longitude);
            var times2 = SunCalc.GetTimes(date2, latitude, longitude);

            // check
            // 2020-10-24: 07:47 - 18:07
            Assert.IsTrue(times1.Sunrise.HasValue, "sunrise must have been set");
            Assert.IsTrue(CheckMatchingTimeOfDay(times1.Sunrise.Value, 7, 47), "sunrise time must match");

            Assert.IsTrue(times1.Sunset.HasValue, "sunset must have been set");
            Assert.IsTrue(CheckMatchingTimeOfDay(times1.Sunset.Value, 18, 7), "sunset time must match");

            // 2020-10-25: 06:48 - 17:06
            Assert.IsTrue(times2.Sunrise.HasValue, "sunrise must have been set");
            Assert.IsTrue(CheckMatchingTimeOfDay(times2.Sunrise.Value, 6, 48), "sunrise time must match");

            Assert.IsTrue(times2.Sunset.HasValue, "sunset must have been set");
            Assert.IsTrue(CheckMatchingTimeOfDay(times2.Sunset.Value, 17, 06), "sunset time must match");

            // difference must be (about) one hour backwards
            Assert.AreEqual(
                -1.0,
                (times2.Sunrise.Value.TimeOfDay - times1.Sunrise.Value.TimeOfDay).TotalHours,
                0.1,
                "sunrise time zone difference must be 1h");

            Assert.AreEqual(
                -1.0,
                (times2.Sunset.Value.TimeOfDay - times1.Sunset.Value.TimeOfDay).TotalHours,
                0.1,
                "sunset time zone difference must be 1h");
        }

        /// <summary>
        /// Tests solar times for a location that has sun up all day or down all day
        /// https://www.timeanddate.com/sun/antarctica/neumayer-station-iii?month=6&year=2020
        /// </summary>
        [TestMethod]
        public void TestSolarTimes_AllDayUp_AllDayDown()
        {
            // set up
            double latitude = -70.674444; // Neumayer-Station III
            double longitude = -8.274167;

            var date1 = new DateTimeOffset(2020, 6, 21, 12, 0, 0, TimeSpan.Zero); // UTC
            var date2 = new DateTimeOffset(2020, 12, 21, 12, 0, 0, TimeSpan.Zero);

            // run
            var times1 = SunCalc.GetTimes(date1, latitude, longitude);
            var times2 = SunCalc.GetTimes(date2, latitude, longitude);

            // check
            // midwinter
            Assert.IsFalse(times1.Sunrise.HasValue, "sunrise must not have been set");
            Assert.IsFalse(times1.Sunset.HasValue, "sunset must not have been set");

            // midsummer
            Assert.IsFalse(times2.Sunrise.HasValue, "sunrise must not have been set");
            Assert.IsFalse(times2.Sunset.HasValue, "sunset must not have been set");

            // the difference between midwinter and midsummer is that midwinter has no Night times set
            Assert.IsTrue(times1.SunriseSunsetTimes.ContainsKey(SunTimeType.Night), "midwinter has a Night time set");
            Assert.IsFalse(times2.SunriseSunsetTimes.ContainsKey(SunTimeType.Night), "midsummer has no Night time set");
        }

        /// <summary>
        /// Some places on earth have a sunset that only occurs after midnight.
        /// https://www.timeanddate.com/sun/@64.475528,-20.253140?month=6&year=2020
        /// </summary>
        [TestMethod]
        public void TestSolarTimes_SunsetOnTheNextDay()
        {
            // set up
            double latitude = 64.475528; // somewhere in iceland
            double longitude = -20.253140;

            var date = new DateTimeOffset(2020, 6, 21, 12, 0, 0, TimeSpan.Zero); // GMT

            // run
            var times = SunCalc.GetTimes(date, latitude, longitude);

            // check
            // 2020-07-31: 02:39 - 00:05 (+1 day)
            Assert.IsTrue(times.Sunrise.HasValue, "sunrise must have been set");
            Assert.IsTrue(CheckMatchingTimeOfDay(times.Sunrise.Value, 2, 39), "sunrise time must match");

            Assert.IsTrue(times.Sunset.HasValue, "sunset must have been set");
            Assert.AreEqual(date.DayOfYear + 1, times.Sunset.Value.DayOfYear, "sunset must be one day ahead");
            Assert.IsTrue(CheckMatchingTimeOfDay(times.Sunset.Value, 0, 5), "sunset time must match");
        }

        /// <summary>
        /// Checks the solar times above the arctic circle on the day with the last sunrise, but
        /// no sunset.
        /// https://www.timeanddate.com/sun/canada/eureka?month=4&year=2020
        /// </summary>
        [TestMethod]
        public void TestSolarTimes_DayWithLastSunrise()
        {
            // set up
            double latitude = 79.988889; // Eureka, Nunavut, Canada
            double longitude = -85.940833;

            var date = new DateTimeOffset(2020, 4, 12, 12, 0, 0, TimeSpan.FromHours(-5));

            // run
            var times = SunCalc.GetTimes(date, latitude, longitude);

            // check
            // 2020-04-12: 01:41 - no sunset
            Assert.IsTrue(times.Sunrise.HasValue, "sunrise must have been set");
            Assert.IsTrue(CheckMatchingTimeOfDay(times.Sunrise.Value, 1, 41), "sunrise time must match");

            //// strange, timeanddate has no sunset value; might be a bug?
            ////Assert.IsFalse(times.Sunset.HasValue, "sunset must have been set");
        }

        /// <summary>
        /// Checks solar times near the equator
        /// https://www.timeanddate.com/sun/@64.475528,-20.253140?month=6&year=2020
        /// </summary>
        [TestMethod]
        public void TestSolarTimes_NearEquator()
        {
            // set up
            double latitude = -8.506810; // Ubud, Bali, Indonesia
            double longitude = 115.262482;

            var date = new DateTimeOffset(2020, 7, 1, 12, 0, 0, TimeSpan.FromHours(8));

            // run
            var times = SunCalc.GetTimes(date, latitude, longitude);

            // check
            // 2020-07-01: 06:33 - 18:11
            Assert.IsTrue(times.Sunrise.HasValue, "sunrise must have been set");
            Assert.IsTrue(CheckMatchingTimeOfDay(times.Sunrise.Value, 6, 33), "sunrise time must match");

            Assert.IsTrue(times.Sunset.HasValue, "sunset must have been set");
            Assert.IsTrue(CheckMatchingTimeOfDay(times.Sunset.Value, 18, 11), "sunset time must match");

            TimeSpan sunriseDuration = times.SunriseSunsetTimes[SunTimeType.SunriseEnd] - times.Sunrise.Value;
            TimeSpan sunsetDuration = times.Sunset.Value - times.SunriseSunsetTimes[SunTimeType.SunsetStart];

            Assert.AreEqual(2.3, sunriseDuration.TotalMinutes, 0.1, "sunrise duration must be 2.3 minutes");
            Assert.AreEqual(2.3, sunsetDuration.TotalMinutes, 0.1, "sunset duration must be 2.3 minutes");
        }
    }
}
