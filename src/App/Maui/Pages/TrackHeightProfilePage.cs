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
        /// <param name="userInterface">user interface</param>
        /// <param name="track">track to display</param>
        public TrackHeightProfilePage(
            IUserInterface userInterface,
            Track track)
        {
            this.Title = "Track height profile";

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
