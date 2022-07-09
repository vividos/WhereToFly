using System.Threading.Tasks;
using System.Windows.Input;
using WhereToFly.App.Core.Models;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for a weather icon as a list entry
    /// </summary>
    public class WeatherIconListEntryViewModel : ViewModelBase
    {
        /// <summary>
        /// Parent view model
        /// </summary>
        private readonly SelectWeatherIconViewModel parentViewModel;

        #region Binding properties
        /// <summary>
        /// Title of weather icon
        /// </summary>
        public string Title { get => this.IconDescription.Name; }

        /// <summary>
        /// Image source for weather icon
        /// </summary>
        public ImageSource Icon { get; private set; }

        /// <summary>
        /// The weather icon's group text
        /// </summary>
        public string Group => this.IconDescription.Group;
        #endregion

        /// <summary>
        /// Weather icon description instance
        /// </summary>
        public WeatherIconDescription IconDescription { get; private set; }

        /// <summary>
        /// Command to execute when an item in the weather icon list has been tapped
        /// </summary>
        public ICommand ItemTappedCommand { get; }

        /// <summary>
        /// Creates a new weather icon view model from weather icon description
        /// </summary>
        /// <param name="parentViewModel">parent view model</param>
        /// <param name="iconDescription">weather icon description to use</param>
        public WeatherIconListEntryViewModel(SelectWeatherIconViewModel parentViewModel, WeatherIconDescription iconDescription)
        {
            this.parentViewModel = parentViewModel;

            this.IconDescription = iconDescription;

            this.ItemTappedCommand = new Command(() =>
            {
                this.parentViewModel.ItemTappedCommand.Execute(this);
            });

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
