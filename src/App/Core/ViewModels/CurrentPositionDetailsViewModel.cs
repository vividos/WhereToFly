using System;
using System.Threading.Tasks;
using System.Timers;
using WhereToFly.App.Core.Logic;
using WhereToFly.App.Core.Models;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;
using WhereToFly.Geo.SunCalcNet;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the current position details page
    /// </summary>
    public class CurrentPositionDetailsViewModel : ViewModelBase
    {
        /// <summary>
        /// App settings object
        /// </summary>
        private readonly AppSettings appSettings;

        /// <summary>
        /// Timer to update LastPositionFix property
        /// </summary>
        private readonly Timer timerUpdateLastPositionFix = new();

        /// <summary>
        /// Current position
        /// </summary>
        private Xamarin.Essentials.Location position;

        /// <summary>
        /// Indicates if the device has a compass that is available
        /// </summary>
        private bool isCompassAvailable;

        /// <summary>
        /// Current magnetic-north compass heading; only set if isCompassAvailable is true
        /// </summary>
        private int currentCompassHeading;

        /// <summary>
        /// Current true-north heading
        /// </summary>
        private int? currentTrueNorthHeading;

        /// <summary>
        /// Current solar times data
        /// </summary>
        private SolarTimes currentSolarTimes;

        /// <summary>
        /// Sunrise direction, in deegrees; may be null
        /// </summary>
        private int? sunriseDirectionInDegrees;

        /// <summary>
        /// Sunset direction, in deegrees; may be null
        /// </summary>
        private int? sunsetDirectionInDegrees;

        #region Binding properties
        /// <summary>
        /// Longitude value of position
        /// </summary>
        public string Longitude
        {
            get
            {
                return this.position == null
                    ? string.Empty
                    : GeoDataFormatter.FormatLatLong(this.position.Longitude, this.appSettings.CoordinateDisplayFormat);
            }
        }

        /// <summary>
        /// Latitude value of position
        /// </summary>
        public string Latitude
        {
            get
            {
                return this.position == null
                    ? string.Empty
                    : GeoDataFormatter.FormatLatLong(this.position.Latitude, this.appSettings.CoordinateDisplayFormat);
            }
        }

        /// <summary>
        /// Altitude of position, in meter
        /// </summary>
        public string Altitude
        {
            get
            {
                return this.position == null || this.position.Altitude == null
                    ? string.Empty
                    : ((int)this.position.Altitude.Value).ToString();
            }
        }

        /// <summary>
        /// Accuracy of position, in meter
        /// </summary>
        public string Accuracy
        {
            get
            {
                return this.position == null || this.position.Accuracy == null
                    ? string.Empty
                    : ((int)this.position.Accuracy.Value).ToString();
            }
        }

        /// <summary>
        /// Color for position accuracy
        /// </summary>
        public Color PositionAccuracyColor
        {
            get
            {
                return this.position == null || this.position.Accuracy == null
                    ? Color.Black
                    : Color.FromHex(ColorFromPositionAccuracy((int)this.position.Accuracy.Value));
            }
        }

        /// <summary>
        /// Last position fix
        /// </summary>
        public string LastPositionFix
        {
            get
            {
                if (this.position == null)
                {
                    return "Unknown";
                }
                else
                {
                    var localTime = this.position.Timestamp.ToLocalTime();

                    var diff = DateTime.Now - localTime;
                    if (diff < TimeSpan.FromHours(1))
                    {
                        return string.Format("{0:mm\\:ss} ago", diff);
                    }
                    else
                    {
                        return string.Format("{0:yyyy-MM-dd HH:mm}", localTime);
                    }
                }
            }
        }

        /// <summary>
        /// Speed in km/h
        /// </summary>
        public int SpeedInKmh
        {
            get
            {
                // the Geolocator plugin reports speed in m/s
                return this.position == null
                    ? 0
                    : (int)((this.position.Speed ?? 0.0) * Geo.Constants.FactorMeterPerSecondToKilometerPerHour);
            }
        }

        /// <summary>
        /// Indicates if magnetic-north heading value is available
        /// </summary>
        public bool IsMagneticNorthHeadingAvail =>
            this.isCompassAvailable;

        /// <summary>
        /// Magnetic-north heading in degrees
        /// </summary>
        public int MagneticNorthHeadingInDegrees =>
            this.isCompassAvailable ? this.currentCompassHeading : 0;

        /// <summary>
        /// Indicates if true-north heading value is available
        /// </summary>
        public bool IsTrueNorthHeadingAvail =>
            this.currentTrueNorthHeading.HasValue || this.position?.Course != null;

        /// <summary>
        /// True-north heading in degrees
        /// </summary>
        public int TrueNorthHeadingInDegrees
        {
            get
            {
                if (this.currentTrueNorthHeading.HasValue)
                {
                    return this.currentTrueNorthHeading.Value;
                }

                return this.position?.Course == null
                    ? 0
                    : (int)this.position.Course.Value;
            }
        }

        /// <summary>
        /// Indicates if sunrise and sunset times are available
        /// </summary>
        public bool IsSunriseSunsetAvail
        {
            get
            {
                return this.position != null &&
                    this.position.Accuracy.HasValue &&
                    this.position.Accuracy < 100.0;
            }
        }

        /// <summary>
        /// Sunrise time text
        /// </summary>
        public string SunriseTime
        {
            get
            {
                if (this.currentSolarTimes == null)
                {
                    return "N/A";
                }

                return this.currentSolarTimes.Sunrise.HasValue
                  ? DataFormatter.FormatDuration(this.currentSolarTimes.Sunrise.Value.ToLocalTime().TimeOfDay)
                  : "No sunrise today";
            }
        }

        /// <summary>
        /// Sunrise time text
        /// </summary>
        public string SunsetTime
        {
            get
            {
                if (this.currentSolarTimes == null)
                {
                    return "N/A";
                }

                return this.currentSolarTimes.Sunset.HasValue
                  ? DataFormatter.FormatDuration(this.currentSolarTimes.Sunset.Value.ToLocalTime().TimeOfDay)
                  : "No sunset today";
            }
        }

        /// <summary>
        /// Indicates if the sunrise direction is available
        /// </summary>
        public bool IsSunriseDirectionAvail => this.sunriseDirectionInDegrees.HasValue;

        /// <summary>
        /// Sunrise direction text
        /// </summary>
        public string SunriseDirectionText =>
            this.sunriseDirectionInDegrees.HasValue
            ? $"{this.sunriseDirectionInDegrees.Value}° " +
                GetTextDirectionFromAngle(this.sunriseDirectionInDegrees.Value)
            : "N/A";

        /// <summary>
        /// Indicates if the sunset direction is available
        /// </summary>
        public bool IsSunsetDirectionAvail => this.sunsetDirectionInDegrees.HasValue;

        /// <summary>
        /// Sunset direction text
        /// </summary>
        public string SunsetDirectionText =>
            this.sunsetDirectionInDegrees.HasValue
            ? $"{this.sunsetDirectionInDegrees.Value}° " +
                GetTextDirectionFromAngle(this.sunsetDirectionInDegrees.Value)
            : "N/A";
        #endregion

        /// <summary>
        /// Creates a new current position details view model
        /// </summary>
        /// <param name="appSettings">app settings to use</param>
        public CurrentPositionDetailsViewModel(AppSettings appSettings)
        {
            this.appSettings = appSettings;

            this.SetupTimer();
        }

        /// <summary>
        /// Sets up timer for updating LastPositionFix; the timer takes care of continuously
        /// displaying the elapsed time since last position
        /// </summary>
        private void SetupTimer()
        {
            this.timerUpdateLastPositionFix.Interval = TimeSpan.FromSeconds(0.2).TotalMilliseconds;
            this.timerUpdateLastPositionFix.Elapsed += this.OnElapsed_TimerUpdateLastPositionFix;
            this.timerUpdateLastPositionFix.Start();
        }

        /// <summary>
        /// Called when the timer to update LastPositionFix has elapsed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnElapsed_TimerUpdateLastPositionFix(object sender, ElapsedEventArgs args)
        {
            this.OnPropertyChanged(nameof(this.LastPositionFix));
        }

        /// <summary>
        /// Called when position has changed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args, including position</param>
        public void OnPositionChanged(object sender, GeolocationEventArgs args)
        {
            this.position = args.Position;

            this.OnPropertyChanged(nameof(this.Longitude));
            this.OnPropertyChanged(nameof(this.Latitude));
            this.OnPropertyChanged(nameof(this.Altitude));
            this.OnPropertyChanged(nameof(this.Accuracy));
            this.OnPropertyChanged(nameof(this.PositionAccuracyColor));
            this.OnPropertyChanged(nameof(this.LastPositionFix));
            this.OnPropertyChanged(nameof(this.SpeedInKmh));

            this.currentSolarTimes = SunCalc.GetTimes(
                this.position.Timestamp,
                this.position.Latitude,
                this.position.Longitude,
                this.position.Altitude ?? 0.0);

            this.OnPropertyChanged(nameof(this.IsSunriseSunsetAvail));
            this.OnPropertyChanged(nameof(this.SunriseTime));
            this.OnPropertyChanged(nameof(this.SunsetTime));

            this.UpdateSunAngles();

            var point = new MapPoint(this.position.Latitude, this.position.Longitude, this.position.Altitude);
            Task.Run(async () => await App.UpdateLastShownPositionAsync(point));
        }

        /// <summary>
        /// Updates sunrise/sunset angles
        /// </summary>
        private void UpdateSunAngles()
        {
            if (this.currentSolarTimes.Sunrise.HasValue)
            {
                SunPosition sunrisePosition = SunCalc.GetPosition(
                    this.currentSolarTimes.Sunrise.Value,
                    this.position.Latitude,
                    this.position.Longitude);

                this.sunriseDirectionInDegrees = (int)sunrisePosition.Azimuth.ToDegrees();
            }
            else
            {
                this.sunriseDirectionInDegrees = null;
            }

            if (this.currentSolarTimes.Sunset.HasValue)
            {
                SunPosition sunsetPosition = SunCalc.GetPosition(
                    this.currentSolarTimes.Sunset.Value,
                    this.position.Latitude,
                    this.position.Longitude);

                this.sunsetDirectionInDegrees = (int)sunsetPosition.Azimuth.ToDegrees();
            }
            else
            {
                this.sunsetDirectionInDegrees = null;
            }

            this.OnPropertyChanged(nameof(this.IsSunriseDirectionAvail));
            this.OnPropertyChanged(nameof(this.IsSunsetDirectionAvail));
            this.OnPropertyChanged(nameof(this.SunriseDirectionText));
            this.OnPropertyChanged(nameof(this.SunsetDirectionText));
        }

        /// <summary>
        /// Returns an HTML color from a position accuracy value.
        /// </summary>
        /// <param name="positionAccuracyInMeter">position accuracy, in meter</param>
        /// <returns>HTML color in format #rrggbb</returns>
        private static string ColorFromPositionAccuracy(int positionAccuracyInMeter)
        {
            if (positionAccuracyInMeter < 40)
            {
                return "#00c000"; // green
            }
            else if (positionAccuracyInMeter < 120)
            {
                return "#e0e000"; // yellow
            }
            else if (positionAccuracyInMeter < 200)
            {
                return "#ff8000"; // orange
            }
            else
            {
                return "#c00000"; // red
            }
        }

        /// <summary>
        /// Cardinal direction names
        /// </summary>
        private static readonly string[] DirectionNames = new string[16]
        {
            "N", "NNE", "NE", "ENE",
            "E", "ESE", "SE", "SSE",
            "S", "SSW", "SW", "WSW",
            "W", "WNW", "NW", "NNW",
        };

        /// <summary>
        /// Gets a textual direction name from a given angle
        /// </summary>
        /// <param name="angleInDegrees">direction angle, in degrees</param>
        /// <returns>textual direction name</returns>
        private static string GetTextDirectionFromAngle(int angleInDegrees)
        {
            int index = (int)((((angleInDegrees - 11.25) / 22.5) + 16.0) % 16.0);

            return DirectionNames[index];
        }

        /// <summary>
        /// Starts compass monitoring, if compass is available
        /// </summary>
        public void StartCompass()
        {
            this.isCompassAvailable = false;

            try
            {
                if (Compass.IsMonitoring)
                {
                    return;
                }

                Compass.ReadingChanged += this.OnCompassReadingChanged;
                Compass.Start(SensorSpeed.UI);
            }
            catch (FeatureNotSupportedException)
            {
                // Feature not supported on device
            }
            catch (Exception ex)
            {
                // Some other exception has occurred
                App.LogError(ex);
            }
        }

        /// <summary>
        /// Stops compass monitoring
        /// </summary>
        public void StopCompass()
        {
            this.isCompassAvailable = false;

            try
            {
                if (!Compass.IsMonitoring)
                {
                    return;
                }

                Compass.ReadingChanged -= this.OnCompassReadingChanged;
                Compass.Stop();
            }
            catch (FeatureNotSupportedException)
            {
                // Feature not supported on device
            }
            catch (Exception ex)
            {
                // Some other exception has occurred
                App.LogError(ex);
            }
        }

        /// <summary>
        /// Called when compass reading has changed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnCompassReadingChanged(object sender, CompassChangedEventArgs args)
        {
            this.currentCompassHeading = (int)(args.Reading.HeadingMagneticNorth + 0.5);
            this.isCompassAvailable = true;

            this.OnPropertyChanged(nameof(this.IsMagneticNorthHeadingAvail));
            this.OnPropertyChanged(nameof(this.MagneticNorthHeadingInDegrees));

            // try to translate magnetic north heading to true north
            var platform = DependencyService.Get<IPlatform>();

            int headingTrueNorth = 0;

            bool translateSuccessful =
                this.position != null &&
                platform.TranslateCompassMagneticNorthToTrueNorth(
                    this.currentCompassHeading,
                    this.position.Latitude,
                    this.position.Longitude,
                    this.position.Altitude ?? 0.0,
                    out headingTrueNorth);

            this.currentTrueNorthHeading = translateSuccessful
                ? headingTrueNorth
                : null;

            this.OnPropertyChanged(nameof(this.IsTrueNorthHeadingAvail));
            this.OnPropertyChanged(nameof(this.TrueNorthHeadingInDegrees));
        }
    }
}
