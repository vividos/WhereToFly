using WhereToFly.App.Abstractions;
using WhereToFly.App.Services;
using WhereToFly.App.ViewModels;

namespace WhereToFly.App.Pages;

/// <summary>
/// Page to display current position details, such as coordinates, speed, heading and accuracy
/// </summary>
public partial class CurrentPositionDetailsPage : ContentPage
{
    /// <summary>
    /// View model for the current position details page
    /// </summary>
    private readonly CurrentPositionDetailsViewModel viewModel;

    /// <summary>
    /// Creates new current position details page
    /// </summary>
    /// <param name="services">service provider</param>
    public CurrentPositionDetailsPage(IServiceProvider services)
    {
        this.InitializeComponent();

        this.BindingContext = this.viewModel =
            new CurrentPositionDetailsViewModel(
                App.Settings!,
                services.GetRequiredService<IGeolocationService>(),
                services.GetRequiredService<CompassGeoServices>());
    }

    #region Page lifecycle methods
    /// <summary>
    /// Called when page is appearing; start updates
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await this.viewModel.StartUpdates();
    }

    /// <summary>
    /// Called when page is disappearing; stops updates
    /// </summary>
    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        this.viewModel.StopUpdates();
    }
    #endregion
}
