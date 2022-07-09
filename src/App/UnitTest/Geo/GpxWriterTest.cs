using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using WhereToFly.Geo.DataFormats;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.UnitTest.Geo
{
    /// <summary>
    /// Tests for GpxWriter class
    /// </summary>
    [TestClass]
    public class GpxWriterTest
    {
        /// <summary>
        /// Tests writing track
        /// </summary>
        [TestMethod]
        public void TestWriteTrack()
        {
            // set up
            const string TrackName = "Test Track";

            var track = new Track
            {
                Name = TrackName,
                TrackPoints = new System.Collections.Generic.List<TrackPoint>
                {
                    new TrackPoint(47.754076, 12.352277, 1234.0, null)
                    {
                        Time = DateTime.Today + TimeSpan.FromHours(1.0),
                    },
                    new TrackPoint(46.017779, 11.900711, 778.2, null)
                    {
                        Time = DateTime.Today + TimeSpan.FromHours(2.0),
                    },
                },
            };

            // run
            string text = string.Empty;
            using (var stream = new MemoryStream())
            {
                GpxWriter.WriteTrack(stream, track);

                text = Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
            }

            // check
            Debug.WriteLine("xml = " + text);
            Assert.IsTrue(text.Contains(TrackName), "gpx must contain track name");
        }
    }
}
