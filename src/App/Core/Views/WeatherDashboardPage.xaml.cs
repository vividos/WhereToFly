using WhereToFly.App.Core.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Dashboard page showing one or more weather icon controls. Tapping the icons opens the
    /// associated web page, app or page.
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WeatherDashboardPage : ContentPage
    {
        /// <summary>
        /// View model for the weather dashboard
        /// </summary>
        private readonly WeatherDashboardViewModel viewModel;

        /// <summary>
        /// Creates a new weather dashboard page
        /// </summary>
        public WeatherDashboardPage()
        {
            this.Title = "Weather";

            this.BindingContext = this.viewModel = new WeatherDashboardViewModel();

            this.InitializeComponent();

            this.SetupBindings();
        }

        /// <summary>
        /// Sets up property bindings; adds weather icon views for all weather icon descriptions.
        /// </summary>
        private void SetupBindings()
        {
            foreach (var iconDescription in this.viewModel.WeatherIconDescriptionList)
            {
                var weatherIcon = new WeatherIcon(iconDescription);
                this.dashboardFlexLayout.Children.Add(weatherIcon);
            }
        }
    }
}
