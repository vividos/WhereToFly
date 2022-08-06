using System;
using System.Threading.Tasks;
using WhereToFly.App.Core.Models;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;
using WhereToFly.Geo.SunCalcNet;
using Xamarin.Essentials;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the compass details page
    /// </summary>
    public class CompassDetailsViewModel : ViewModelBase
    {
        /// <summary>
        /// App settings object
        /// </summary>
        private readonly AppSettings appSettings;

        /// <summary>
        /// Current position
        /// </summary>
        private Xamarin.Essentials.Location position;

        /// <summary>
        /// Indicates if the device has a compass that is available
        /// </summary>
        private bool isCompassAvailable;

        /// <summary>
        /// Current compass heading; only set if isCompassAvailable is true
        /// </summary>
        private int currentCompassHeading;

        #region Binding properties
        /// <summary>
        /// Distance text
        /// </summary>
        public string Distance
        {
            get
            {
                if (this.position == null ||
                    this.appSettings?.CurrentCompassTarget?.TargetLocation == null ||
                    !this.appSettings.CurrentCompassTarget.TargetLocation.Valid)
                {
                    return "-";
                }

                var point = new MapPoint(
                    this.position.Latitude,
                    this.position.Longitude,
                    this.position.Altitude);

                double distanceInMeter = point.DistanceTo(
                    this.appSettings.CurrentCompassTarget.TargetLocation);

                return distanceInMeter < 500
                    ? $"{distanceInMeter} m"
                    : $"{distanceInMeter / 1000.0:F1} km";
            }
        }

        /// <summary>
        /// Height difference, in meter or km
        /// </summary>
        public string HeightDifference
        {
            get
            {
                if (this.position == null ||
                    !this.position.Altitude.HasValue ||
                    this.appSettings?.CurrentCompassTarget?.TargetLocation == null ||
                    !this.appSettings.CurrentCompassTarget.TargetLocation.Valid ||
                    !this.appSettings.CurrentCompassTarget.TargetLocation.Altitude.HasValue)
                {
                    return "-";
                }

                double heightDifferenceInMeter =
                    this.appSettings.CurrentCompassTarget.TargetLocation.Altitude.Value -
                    this.position.Altitude.Value;

                return $"{heightDifferenceInMeter:F0} m";
            }
        }

        /// <summary>
        /// Indicates if heading value is available
        /// </summary>
        public bool IsHeadingAvail =>
            this.isCompassAvailable || this.position?.Course == null;

        /// <summary>
        /// Heading in degrees
        /// </summary>
        public int HeadingInDegrees
        {
            get
            {
                if (this.isCompassAvailable)
                {
                    return this.currentCompassHeading;
                }

                return this.position?.Course == null
                    ? 0
                    : (int)this.position.Course.Value;
            }
        }

        /// <summary>
        /// Indicates if direction value is available
        /// </summary>
        public bool IsDirectionAvail =>
            this.appSettings?.CurrentCompassTarget?.TargetDirection.HasValue ?? false;

        /// <summary>
        /// Set compass target direction in degrees
        /// </summary>
        public int? TargetDirectionInDegrees =>
            this.appSettings?.CurrentCompassTarget?.TargetDirection;

        /// <summary>
        /// Sunrise direction, in deegrees; may be null
        /// </summary>
        public int? SunriseDirectionInDegrees { get; set; }

        /// <summary>
        /// Sunset direction, in deegrees; may be null
        /// </summary>
        public int? SunsetDirectionInDegrees { get; set; }
        #endregion

        /// <summary>
        /// Creates a new compass details view model
        /// </summary>
        /// <param name="appSettings">app settings to use</param>
        public CompassDetailsViewModel(AppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        /// <summary>
        /// Called when position has changed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args, including position</param>
        public void OnPositionChanged(object sender, GeolocationEventArgs args)
        {
            this.position = args.Position;

            this.OnPropertyChanged(nameof(this.Distance));
            this.OnPropertyChanged(nameof(this.HeightDifference));
            this.OnPropertyChanged(nameof(this.IsHeadingAvail));
            this.OnPropertyChanged(nameof(this.HeadingInDegrees));
            this.OnPropertyChanged(nameof(this.IsDirectionAvail));
            this.OnPropertyChanged(nameof(this.TargetDirectionInDegrees));

            this.UpdateSunAngles();

            var point = new MapPoint(this.position.Latitude, this.position.Longitude, this.position.Altitude);
            Task.Run(async () => await App.UpdateLastShownPositionAsync(point));
        }

        /// <summary>
        /// Updates sunrise/sunset angles
        /// </summary>
        private void UpdateSunAngles()
        {
            SolarTimes currentSolarTimes = SunCalc.GetTimes(
                this.position.Timestamp,
                this.position.Latitude,
                this.position.Longitude,
                this.position.Altitude ?? 0.0);

            if (currentSolarTimes.Sunrise.HasValue)
            {
                SunPosition sunrisePosition = SunCalc.GetPosition(
                    currentSolarTimes.Sunrise.Value,
                    this.position.Latitude,
                    this.position.Longitude);

                this.SunriseDirectionInDegrees = (int)sunrisePosition.Azimuth.ToDegrees();
            }
            else
            {
                this.SunriseDirectionInDegrees = null;
            }

            if (currentSolarTimes.Sunset.HasValue)
            {
                SunPosition sunsetPosition = SunCalc.GetPosition(
                    currentSolarTimes.Sunset.Value,
                    this.position.Latitude,
                    this.position.Longitude);

                this.SunsetDirectionInDegrees = (int)sunsetPosition.Azimuth.ToDegrees();
            }
            else
            {
                this.SunsetDirectionInDegrees = null;
            }

            this.OnPropertyChanged(nameof(this.SunriseDirectionInDegrees));
            this.OnPropertyChanged(nameof(this.SunsetDirectionInDegrees));
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

            this.OnPropertyChanged(nameof(this.HeadingInDegrees));
        }
    }
}
