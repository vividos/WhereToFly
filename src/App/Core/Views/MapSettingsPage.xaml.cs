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
            this.IconImageSource = new FileImageSource
            {
                File = Converter.ImagePathConverter.GetDeviceDependentImage("map"),
            };

            this.InitializeComponent();
        }
    }
}
