using WhereToFly.Core.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace WhereToFly.Core.Views
{
    /// <summary>
    /// Settings page to configure app settings
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        /// <summary>
        /// Creates new settings page
        /// </summary>
        public SettingsPage()
        {
            this.Title = "Settings";
            this.BindingContext = new SettingsViewModel();

            this.InitializeComponent();
        }
    }
}
