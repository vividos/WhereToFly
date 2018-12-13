using WhereToFly.App.Core.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Settings page to configure app map settings
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MapSettingsPage : ContentPage
    {
        /// <summary>
        /// Creates new map settings page
        /// </summary>
        public MapSettingsPage()
        {
            this.Title = "Map Settings";
            this.BindingContext = new MapSettingsViewModel();

            this.InitializeComponent();
        }
    }
}
