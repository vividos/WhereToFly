using WhereToFly.App.Core.ViewModels;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Settings page to configure app map settings
    /// </summary>
    public partial class MapSettingsPage : ContentPage
    {
        /// <summary>
        /// Creates new map settings page
        /// </summary>
        public MapSettingsPage()
        {
            this.Title = "Map";
            this.IconImageSource = new FileImageSource
            {
                File = Converter.ImagePathConverter.GetDeviceDependentImage("map")
            };

            this.BindingContext = new MapSettingsViewModel();

            this.InitializeComponent();
        }
    }
}
