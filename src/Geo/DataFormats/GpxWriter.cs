using System.IO;
using System.Xml;

namespace WhereToFly.Geo.DataFormats
{
    /// <summary>
    /// Writes a track to a stream for saving
    /// </summary>
    public class GpxWriter
    {
        /// <summary>
        /// Stream to write to
        /// </summary>
        private readonly Stream stream;

        /// <summary>
        /// Writes a track to a file
        /// </summary>
        /// <param name="filename">filename to write to</param>
        /// <param name="track">track to write</param>
        public static void WriteTrack(string filename, Model.Track track)
        {
            using var fileStream = new FileStream(filename, FileMode.Create);
            WriteTrack(fileStream, track);
        }

        /// <summary>
        /// Writes a track to a stream
        /// </summary>
        /// <param name="stream">stream to write to</param>
        /// <param name="track">track to write</param>
        public static void WriteTrack(Stream stream, Model.Track track)
        {
            var writer = new GpxWriter(stream);
            writer.Write(track);
        }

        /// <summary>
        /// Creates a new gpx writer
        /// </summary>
        /// <param name="stream">stream to use</param>
        public GpxWriter(Stream stream)
        {
            this.stream = stream;
        }

        /// <summary>
        /// Writes track to stream
        /// </summary>
        /// <param name="track">track to write</param>
        private void Write(Model.Track track)
        {
            var settings = new XmlWriterSettings();
            using var writer = XmlWriter.Create(this.stream, settings);
            writer.WriteStartDocument();

            writer.WriteStartElement("gpx", "http://www.topografix.com/GPX/1/1");
            writer.WriteAttributeString("version", "1.1");
            writer.WriteAttributeString("creator", "Where-to-fly");

            writer.WriteStartElement("trk");
            writer.WriteElementString("name", track.Name);

            writer.WriteStartElement("trkseg");

            var numberFormat = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

            foreach (var trackPoint in track.TrackPoints)
            {
                writer.WriteStartElement("trkpt");

                writer.WriteAttributeString("lat", trackPoint.Latitude.ToString(numberFormat));
                writer.WriteAttributeString("lon", trackPoint.Longitude.ToString(numberFormat));

                if (trackPoint.Altitude.HasValue)
                {
                    writer.WriteElementString("ele", trackPoint.Altitude.Value.ToString(numberFormat));
                }

                if (trackPoint.Time.HasValue)
                {
                    writer.WriteElementString("time", trackPoint.Time.Value.ToString("o"));
                }

                // unfortunately there's no way to store trackPoint.Heading
                writer.WriteEndElement(); // trkpt
            }

            writer.WriteEndElement(); // trkseg
            writer.WriteEndElement(); // trk
            writer.WriteEndElement(); // gpx

            writer.WriteEndDocument();
        }
    }
}
