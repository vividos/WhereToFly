using Plugin.Geolocator.Abstractions;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using WhereToFly.App.Logic;
using WhereToFly.App.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the current position details page
    /// </summary>
    public class CurrentPositionDetailsViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// App settings object
        /// </summary>
        private readonly AppSettings appSettings;

        /// <summary>
        /// Current position
        /// </summary>
        private Position position;

        #region Binding properties
        /// <summary>
        /// Longitude value of position
        /// </summary>
        public string Longitude
        {
            get
            {
                return this.position == null ? string.Empty :
                    DataFormatter.FormatLatLong(this.position.Longitude, this.appSettings.CoordinateDisplayFormat);
            }
        }

        /// <summary>
        /// Latitude value of position
        /// </summary>
        public string Latitude
        {
            get
            {
                return this.position == null ? string.Empty :
                    DataFormatter.FormatLatLong(this.position.Latitude, this.appSettings.CoordinateDisplayFormat);
            }
        }

        /// <summary>
        /// Altitude of position, in meter
        /// </summary>
        public string Altitude
        {
            get
            {
                return this.position == null ? string.Empty : ((int)this.position.Altitude).ToString();
            }
        }

        /// <summary>
        /// Accuracy of position, in meter
        /// </summary>
        public string Accuracy
        {
            get
            {
                return this.position == null ? string.Empty : ((int)this.position.Accuracy).ToString();
            }
        }

        /// <summary>
        /// Color for position accuracy
        /// </summary>
        public Color PositionAccuracyColor
        {
            get
            {
                return this.position == null ? Color.Black : Color.FromHex(ColorFromPositionAccuracy((int)this.position.Accuracy));
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
                return this.position == null ? 0 :
                    (int)(this.position.Speed * Geo.Spatial.Constants.FactorMeterPerSecondToKilometerPerHour);
            }
        }

        /// <summary>
        /// Indicates if heading value is available
        /// </summary>
        public bool IsHeadingAvail
        {
            get
            {
                return this.position != null;
            }
        }

        /// <summary>
        /// Heading in degrees
        /// </summary>
        public int HeadingInDegrees
        {
            get
            {
                return this.position == null ? 0 : (int)this.position.Heading;
            }
        }
        #endregion

        /// <summary>
        /// Creates a new current position details view model
        /// </summary>
        /// <param name="appSettings">app settings to use</param>
        public CurrentPositionDetailsViewModel(AppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        /// <summary>
        /// Called when position has changed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args, including position</param>
        public void OnPositionChanged(object sender, PositionEventArgs args)
        {
            this.position = args.Position;

            this.OnPropertyChanged(nameof(this.Longitude));
            this.OnPropertyChanged(nameof(this.Latitude));
            this.OnPropertyChanged(nameof(this.Altitude));
            this.OnPropertyChanged(nameof(this.Accuracy));
            this.OnPropertyChanged(nameof(this.PositionAccuracyColor));
            this.OnPropertyChanged(nameof(this.LastPositionFix));
            this.OnPropertyChanged(nameof(this.SpeedInKmh));
            this.OnPropertyChanged(nameof(this.IsHeadingAvail));
            this.OnPropertyChanged(nameof(this.HeadingInDegrees));

            Task.Run(async () => await this.UpdateLastKnownPositionAsync(this.position));
        }

        /// <summary>
        /// Updates last known position in data service
        /// </summary>
        /// <param name="position">current position</param>
        /// <returns>task to wait on</returns>
        private async Task UpdateLastKnownPositionAsync(Position position)
        {
            var point = new MapPoint(position.Latitude, position.Longitude);

            if (point.Valid)
            {
                this.appSettings.LastKnownPosition = point;

                var dataService = DependencyService.Get<IDataService>();
                await dataService.StoreAppSettingsAsync(this.appSettings);
            }
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

        #region INotifyPropertyChanged implementation
        /// <summary>
        /// Event that gets signaled when a property has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Call this method to signal that a property has changed
        /// </summary>
        /// <param name="propertyName">property name; use C# 6 nameof() operator</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
