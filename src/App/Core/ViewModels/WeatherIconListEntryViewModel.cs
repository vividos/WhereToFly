using System.Threading.Tasks;
using WhereToFly.App.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for a weather icon as a list entry
    /// </summary>
    public class WeatherIconListEntryViewModel : ViewModelBase
    {
        #region Binding properties
        /// <summary>
        /// Title of weather icon
        /// </summary>
        public string Title { get => this.IconDescription.Name; }

        /// <summary>
        /// Image source for weather icon
        /// </summary>
        public ImageSource Icon { get; private set; }
        #endregion

        /// <summary>
        /// Weather icon description instance
        /// </summary>
        public WeatherIconDescription IconDescription { get; private set; }

        /// <summary>
        /// Creates a new weather icon view model from weather icon description
        /// </summary>
        /// <param name="iconDescription">weather icon description to use</param>
        public WeatherIconListEntryViewModel(WeatherIconDescription iconDescription)
        {
            this.IconDescription = iconDescription;

            this.SetupBindings();
        }

        /// <summary>
        /// Sets up bindings properties
        /// </summary>
        private void SetupBindings()
        {
            Task.Run(async () =>
            {
                this.Icon = await WeatherImageCache.GetImageAsync(this.IconDescription);

                this.OnPropertyChanged(nameof(this.Icon));
            });
        }
    }
}
