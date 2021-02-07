using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using WhereToFly.Shared.Base;

namespace WhereToFly.App.Geo.DataFormats
{
    /// <summary>
    /// Parser for the IGC flight log format specified by the FAI in the document
    /// "IGC GNSS FR Specification" (file tech_spec_gnss.pdf), Appendix A.
    /// Note that the parser doesn't check the exact format of the file, but instead tries to get
    /// a best-effort track representation of the IGC file and its flight(s).
    /// </summary>
    internal class IgcParser
    {
        /// <summary>
        /// Track that was produced while loading and parsing the IGC file
        /// </summary>
        public Track Track { get; set; } = new Track();

        /// <summary>
        /// Header fields, from H records
        /// </summary>
        private readonly Dictionary<string, string> headerFields = new Dictionary<string, string>();

        /// <summary>
        /// Current date part of track
        /// </summary>
        private DateTimeOffset? currentDate = null;

        /// <summary>
        /// Creates a new IGC parser and parses the given stream
        /// </summary>
        /// <param name="stream">IGC data stream</param>
        public IgcParser(Stream stream)
        {
            this.Track.Id = Guid.NewGuid().ToString("B");
            this.Track.IsFlightTrack = true;
            this.Parse(stream);
        }

        /// <summary>
        /// Parses stream as IGC text file
        /// </summary>
        /// <param name="stream">IGC data stream</param>
        private void Parse(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    if (line.Length > 0)
                    {
                        try
                        {
                            this.ParseLine(line);
                        }
                        catch (Exception)
                        {
                            // ignore line parsing errors
                        }
                    }
                }
            }

            this.Track.Name = this.FormatTrackName();
        }

        /// <summary>
        /// Formats a track name consisting of the date of flight and the pilot's name
        /// </summary>
        /// <returns>formatted track name</returns>
        private string FormatTrackName()
        {
            string date = this.currentDate.HasValue ? this.currentDate.Value.ToString("yyyy'-'MM'-'dd") : string.Empty;

            string pilotName = this.headerFields.GetValueOrDefault("PILOT", null);
            if (pilotName == null)
            {
                pilotName = this.headerFields.GetValueOrDefault("PILOTINCHARGE", "???");
            }

            return $"{date} {pilotName}";
        }

        /// <summary>
        /// Parses a single line in the IGC file
        /// </summary>
        /// <param name="line">text line to parse</param>
        private void ParseLine(string line)
        {
            Debug.Assert(line.Length > 0, "line must not be empty");

            switch (line[0])
            {
                case 'H':
                    this.ParseRecordH(line);
                    break;

                case 'B': // Fix
                    var trackPoint = this.ParseRecordB(line);
                    this.Track.TrackPoints.Add(trackPoint);

                    if (trackPoint.Time.HasValue &&
                        trackPoint.Time.Value.Date != this.currentDate)
                    {
                        // next day
                        this.currentDate = trackPoint.Time.Value.Date;
                    }

                    break;

                case 'G': // Security
                    break;

                default:
                    Debug.WriteLine($"unknown IGC record {line[0]}");
                    break;
            }
        }

        /// <summary>
        /// Parses H record, as defined in Annex 1, chapter 3.3
        /// </summary>
        /// <param name="line">line to parse</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "StyleCop.CSharp.ReadabilityRules",
            "SA1113:CommaMustBeOnSameLineAsPreviousParameter",
            Justification = "False positive")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "StyleCop.CSharp.ReadabilityRules",
            "SA1117:ParametersMustBeOnSameLineOrSeparateLines",
            Justification = "False positive")]
        private void ParseRecordH(string line)
        {
            if (line.StartsWith("HFDTE"))
            {
                this.ParseStartDate(line);
            }

            int pos = line.IndexOf(':');
            if (pos == -1 || pos < 5)
            {
                return;
            }

            string key = line.Substring(5, pos - 5).Trim();
            string text = line.Substring(pos + 1).Trim();

            if (pos == 5)
            {
                key = line.Substring(1, 4);
            }

            if (!string.IsNullOrEmpty(key) &&
                !this.headerFields.ContainsKey(key))
            {
                this.headerFields.Add(key, text);
            }
        }

        /// <summary>
        /// Parses the start date, contained in the HFDTE record. Definen in
        ///
        /// Example: HFDTE260898
        /// </summary>
        /// <param name="line">line to parse</param>
        private void ParseStartDate(string line)
        {
            string dateText = line.Substring(5);

            // there are IGC files which have a "HFDTEDATE:" record
            if (dateText.StartsWith("DATE:"))
            {
                dateText = dateText.Substring(5);

                // and some of them have a comma, e.g.:
                // HFDTEDATE:200720,01
                int commaPos = dateText.IndexOf(',');
                if (commaPos != -1)
                {
                    dateText = dateText.Substring(0, commaPos);
                }
            }

            if (DateTimeOffset.TryParseExact(
                dateText.Trim(),
                "ddMMyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal,
                out DateTimeOffset startDate))
            {
                this.currentDate = startDate.Date;
            }
        }

        /// <summary>
        /// Parses B record, as defined in Annex 1, chapter 4.1, and returns a track point.
        /// </summary>
        /// <param name="line">line to parse</param>
        /// <returns>parsed track point</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "StyleCop.CSharp.ReadabilityRules",
            "SA1117:ParametersMustBeOnSameLineOrSeparateLines",
            Justification = "False positive")]
        private TrackPoint ParseRecordB(string line)
        {
#pragma warning disable S125 // Sections of code should not be "commented out"
            // Record type, B
            // |
            // |Time in HHMMSS, index 1
            // ||
            // ||     Latitude in DDMMmmmN/S, 8 bytes, index 7
            // ||     |
            // ||     |       Longitude in DDDMMmmmE/W, 9 bytes, index 15
            // ||     |       |
            // ||     |       |        Fix validity, A or V, index 24
            // ||     |       |        |
            // ||     |       |        |Pressure altitude, PPPPP, 5 bytes, index 25
            // ||     |       |        ||
            // ||     |       |        ||    GNSS altitude, GGGGG, 5 bytes, index 30
            // ||     |       |        ||    |
            // ||     |       |        ||    |    Optional infos
            // ||     |       |        ||    |    |
            // B0903544724034N01052829EA009300100900108000
            // B0859144723871N01053378EA013770146800109000
#pragma warning restore S125

            double latitude = ParseLatLong("0" + line.Substring(7, 8));
            double longitude = ParseLatLong(line.Substring(15, 9));

            double altitude = Convert.ToDouble(line.Substring(30, 5));

            var trackPoint = new TrackPoint(latitude, longitude, altitude, null);

            bool hasTimeOffset = TimeSpan.TryParseExact(
                line.Substring(1, 6),
                "hhmmss",
                System.Globalization.CultureInfo.InvariantCulture,
                out TimeSpan timeOffset);

            if (hasTimeOffset &&
                this.currentDate.HasValue)
            {
                trackPoint.Time = this.currentDate.Value + timeOffset;
            }

            return trackPoint;
        }

        /// <summary>
        /// Parses latitude or longitude string value, in the format DDDMMmmmX, with DDD the
        /// degrees part, MM the minutes part, mmm the fractional minutes part and X the
        /// direction, either N, S, E or W. Specified in Annex 1, chapter 4.1.
        /// As latitude has only 2 digits for DDD, prepend it with digit 0 before passing to this
        /// method.
        /// </summary>
        /// <param name="latLong">latitude or longitude value as text</param>
        /// <returns>parsed latitude or longitude value</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "StyleCop.CSharp.ReadabilityRules",
            "SA1113:CommaMustBeOnSameLineAsPreviousParameter",
            Justification = "False positive")]
        private static double ParseLatLong(string latLong)
        {
            int decimalValue = Convert.ToInt32(latLong.Substring(0, 3));
            int minuteValue = Convert.ToInt32(latLong.Substring(3, 2));
            int minuteFractional = Convert.ToInt32(latLong.Substring(5, 3));

            double value = decimalValue + ((minuteValue + (minuteFractional / 1000.0)) / 60.0);

            char direction = latLong[8];
            if (direction == 'S' || direction == 'W')
            {
                value = -value;
            }

            return value;
        }
    }
}
