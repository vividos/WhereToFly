using WhereToFly.App.Core.Controls;
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

            this.InitializeComponent();

            this.BindingContext = this.viewModel = new WeatherDashboardViewModel();
            this.viewModel.PropertyChanged += this.ViewModel_PropertyChanged;
        }

        /// <summary>
        /// Called when one of the properties of the view model has changed. Adds weather icon
        /// views for all weather icon descriptions when the list has changed
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(this.viewModel.WeatherIconDescriptionList))
            {
                App.RunOnUiThread(() =>
                {
                    this.dashboardFlexLayout.Children.Clear();

                    foreach (var iconDescription in this.viewModel.WeatherIconDescriptionList)
                    {
                        var weatherIcon = new WeatherIcon(iconDescription);
                        this.dashboardFlexLayout.Children.Add(weatherIcon);
                    }
                });
            }
        }
    }
}
