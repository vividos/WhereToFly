using CommunityToolkit.Maui;
using Microsoft.Maui.Controls.Shapes;
using System.ComponentModel;

namespace WhereToFly.App.Weather.ViewModels;

/// <summary>
/// Common view model base class; supports notifying user interface via
/// INotifyPropertyChanged
/// </summary>
public class ViewModelBase : INotifyPropertyChanged
{
    /// <summary>
    /// Service provider
    /// </summary>
    public static IServiceProvider Services
        => IPlatformApplication.Current?.Services
        ?? throw new InvalidOperationException("IServiceProvider is not available");

    /// <summary>
    /// The current app instance
    /// </summary>
    private static App App
        => App.Current as App
        ?? throw new InvalidOperationException("App.Current is not available");

    /// <summary>
    /// The current main page
    /// </summary>
    protected static Page MainPage
        => App.Current?.Windows.FirstOrDefault()?.Page
        ?? throw new InvalidOperationException("MainPage is not available");

    /// <summary>
    /// Returns if the app is currently using a dark theme. This takes into account when the
    /// app theme is set to "Same as device".
    /// </summary>
    public static bool IsDarkTheme
        => App.UserAppTheme == AppTheme.Dark ||
        (App.UserAppTheme == AppTheme.Unspecified && App.RequestedTheme == AppTheme.Dark);

    /// <summary>
    /// Default popup options
    /// </summary>
    public static readonly PopupOptions DefaultPopupOptions =
        new PopupOptions
        {
            PageOverlayColor = Colors.Gray.WithAlpha(0.5f),
            Shape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(8),
                StrokeThickness = 0,
            },
            Shadow = null,
        };

    #region INotifyPropertyChanged implementation
    /// <summary>
    /// Event that gets signaled when a property has changed
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Call this method to signal that a property has changed
    /// </summary>
    /// <param name="propertyName">property name; use C# 6 nameof() operator</param>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        this.PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}
