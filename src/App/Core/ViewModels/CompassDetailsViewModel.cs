using System;
using System.Threading.Tasks;
using System.Windows.Input;
using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.Services;
using WhereToFly.Geo;
using WhereToFly.Geo.Model;
using WhereToFly.Geo.SunCalcNet;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

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
        /// Current magnetic-north compass heading; only set if isCompassAvailable is true
        /// </summary>
        private int currentCompassHeading;

        /// <summary>
        /// Current true-north heading
        /// </summary>
        private int? currentTrueNorthHeading;

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
                    ? $"{(int)distanceInMeter} m"
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
        /// Indicates if target direction value is available
        /// </summary>
        public bool IsTargetDirectionAvail =>
            this.CalculateTargetDirection().HasValue;

        /// <summary>
        /// Set compass target direction in degrees
        /// </summary>
        public int? TargetDirectionInDegrees =>
            this.CalculateTargetDirection();

        /// <summary>
        /// Set compass target direction, as text
        /// </summary>
        public string TargetDirectionText =>
            this.CalculateTargetDirection()?.ToString() ?? "N/A";

        /// <summary>
        /// Sunrise direction, in deegrees; may be null
        /// </summary>
        public int? SunriseDirectionInDegrees { get; set; }

        /// <summary>
        /// Sunset direction, in deegrees; may be null
        /// </summary>
        public int? SunsetDirectionInDegrees { get; set; }

        /// <summary>
        /// Command to execute when the user clicks on the "set target direction" toolbar icon
        /// </summary>
        public ICommand SetTargetDirectionCommand { get; set; }

        /// <summary>
        /// Command to execute when the user clicks on the "clear compass target" toolbar icon
        /// </summary>
        public ICommand ClearCompassTargetCommand { get; set; }
        #endregion

        /// <summary>
        /// Creates a new compass details view model
        /// </summary>
        /// <param name="appSettings">app settings to use</param>
        public CompassDetailsViewModel(AppSettings appSettings)
        {
            this.appSettings = appSettings;

            this.SetTargetDirectionCommand = new AsyncCommand(this.SetTargetDirection);
            this.ClearCompassTargetCommand = new AsyncCommand(this.ClearCompassTarget);
        }

        /// <summary>
        /// Called when the user clicked on the "set target direction" toolbar icon
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task SetTargetDirection()
        {
            int initialDirection = this.TargetDirectionInDegrees ?? this.TrueNorthHeadingInDegrees;

            int? direction =
                (await NavigationService.Instance.NavigateToPopupPageAsync<Tuple<int>>(
                    PopupPageKey.SetCompassTargetDirectionPopupPage,
                    true,
                    initialDirection))?.Item1;

            if (direction == null)
            {
                return;
            }

            var compassTarget = new CompassTarget
            {
                Title = $"Fixed direction {direction.Value}°",
                TargetDirection = direction,
            };

            await App.SetCompassTarget(compassTarget);

            this.OnPropertyChanged(nameof(this.Distance));
            this.OnPropertyChanged(nameof(this.HeightDifference));
            this.OnPropertyChanged(nameof(this.IsTargetDirectionAvail));
            this.OnPropertyChanged(nameof(this.TargetDirectionInDegrees));
            this.OnPropertyChanged(nameof(this.TargetDirectionText));
        }

        /// <summary>
        /// Called when the user clicked on the "clear compass target" toolbar icon
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task ClearCompassTarget()
        {
            await App.SetCompassTarget(null);

            this.OnPropertyChanged(nameof(this.Distance));
            this.OnPropertyChanged(nameof(this.HeightDifference));
            this.OnPropertyChanged(nameof(this.IsTargetDirectionAvail));
            this.OnPropertyChanged(nameof(this.TargetDirectionInDegrees));
            this.OnPropertyChanged(nameof(this.TargetDirectionText));
        }

        /// <summary>
        /// Calculates the target direction, if available
        /// </summary>
        /// <returns>target direction angle, in degrees, or null when not set</returns>
        private int? CalculateTargetDirection()
        {
            CompassTarget compassTarget = this.appSettings?.CurrentCompassTarget;
            if (compassTarget == null)
            {
                return null;
            }

            // target location was set?
            if (compassTarget.TargetLocation != null &&
                compassTarget.TargetLocation.Valid &&
                this.position != null)
            {
                var point = new MapPoint(
                    this.position.Latitude,
                    this.position.Longitude,
                    this.position.Altitude);

                double courseAngleInDegrees = point.CourseTo(compassTarget.TargetLocation);
                return (int)(courseAngleInDegrees + 0.5);
            }

            // might be a target direction
            return compassTarget.TargetDirection;
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
            this.OnPropertyChanged(nameof(this.IsTargetDirectionAvail));
            this.OnPropertyChanged(nameof(this.TargetDirectionInDegrees));
            this.OnPropertyChanged(nameof(this.TargetDirectionText));

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
