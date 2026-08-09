using WhereToFly.App.ViewModels;

namespace WhereToFly.App.Pages;

/// <summary>
/// Settings page to configure general app settings
/// </summary>
public partial class GeneralSettingsPage : ContentPage
{
    /// <summary>
    /// Creates new general settings page
    /// </summary>
    public GeneralSettingsPage()
    {
        this.BindingContext = new GeneralSettingsViewModel();

        this.InitializeComponent();
    }
}
