using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using WhereToFly.App.Core.Models;
using WhereToFly.App.MapView;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the "flying range" popup page
    /// </summary>
    public class FlyingRangePopupViewModel : ViewModelBase
    {
        /// <summary>
        /// App settings to use for storing flying parameters
        /// </summary>
        private readonly AppSettings appSettings;

        /// <summary>
        /// Flying range parameters
        /// </summary>
        public FlyingRangeParameters Parameters { get; private set; } = new FlyingRangeParameters();

        #region Binding properties
        /// <summary>
        /// Property that contains the glide ratio value
        /// </summary>
        public double GlideRatio
        {
            get => this.Parameters.GlideRatio;
            set
            {
                this.Parameters.GlideRatio = value;
                this.OnPropertyChanged(nameof(this.GlideRatio));
            }
        }

        /// <summary>
        /// List of possible wind directions
        /// </summary>
        public List<string> WindDirectionList { get; private set; }

        /// <summary>
        /// Property that contains the wind direction as printable wind direction
        /// </summary>
        public string WindDirection
        {
            get
            {
                int index = (int)((this.Parameters.WindDirection % 360.0) / 45.0);
                Debug.Assert(index < this.WindDirectionList.Count, "index must be in array length");
                return this.WindDirectionList[index];
            }

            set
            {
                int index = this.WindDirectionList.IndexOf(value);
                this.Parameters.WindDirection = index * 45.0;
            }
        }

        /// <summary>
        /// Property that contains the wind speed
        /// </summary>
        public string WindSpeed
        {
            get => $"{(int)this.Parameters.WindSpeed} km/h";
            set
            {
                if (int.TryParse(value.Replace(" km/h", string.Empty), out int windSpeed))
                {
                    this.Parameters.WindSpeed = windSpeed;
                }
            }
        }
        #endregion

        /// <summary>
        /// Creates a new "flying range" popup page view model
        /// </summary>
        /// <param name="appSettings">app settings to use</param>
        public FlyingRangePopupViewModel(AppSettings appSettings)
        {
            this.appSettings = appSettings;

            if (appSettings?.LastFlyingRangeParameters != null)
            {
                this.Parameters = appSettings.LastFlyingRangeParameters;
            }

            // the wind direction list is specified so that the direction angle can directly be
            // calculated from the entries index
            this.WindDirectionList = new List<string>
            {
                "N",
                "NE",
                "E",
                "SE",
                "S",
                "SW",
                "W",
                "NW",
            };
        }

        /// <summary>
        /// Stores current filying range parameters in the app settings.
        /// </summary>
        /// <returns>task to wait on</returns>
        public async Task StoreFlyingRangeParameters()
        {
            this.appSettings.LastFlyingRangeParameters = this.Parameters;

            var dataService = DependencyService.Get<IDataService>();
            await dataService.StoreAppSettingsAsync(this.appSettings);
        }
    }
}
