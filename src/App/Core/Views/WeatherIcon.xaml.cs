using WhereToFly.App.Core.ViewModels;
using WhereToFly.App.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Weather icon content view, showing a single weather icon with title
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WeatherIcon : ContentView
    {
        /// <summary>
        /// View model for weather icon
        /// </summary>
        private readonly WeatherIconViewModel viewModel;

        /// <summary>
        /// Creates a new weather icon instance
        /// </summary>
        /// <param name="iconDescription">weather icon description</param>
        public WeatherIcon(WeatherIconDescription iconDescription)
        {
            this.BindingContext = this.viewModel = new WeatherIconViewModel(iconDescription);

            this.InitializeComponent();
        }
    }
}
