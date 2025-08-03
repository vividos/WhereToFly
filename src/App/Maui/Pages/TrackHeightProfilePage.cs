using WhereToFly.App.Abstractions;
using WhereToFly.App.MapView;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Pages
{
    /// <summary>
    /// Page showing a track height profile
    /// </summary>
    public class TrackHeightProfilePage : ContentPage
    {
        /// <summary>
        /// Creates track height profile page
        /// </summary>
        /// <param name="track">track to display</param>
        public TrackHeightProfilePage(Track track)
        {
            this.Title = "Track height profile";

            var userInterface = DependencyService.Get<IUserInterface>();

            this.Content = new HeightProfileView
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                Track = track,
                UseDarkTheme = userInterface.IsDarkTheme,
            };

            this.SetDynamicResource(BackgroundColorProperty, "PageBackgroundColor");

            this.AutomationId = "TrackHeightProfile";
        }
    }
}
