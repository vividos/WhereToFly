﻿using WhereToFly.App.MapView;
using WhereToFly.Geo.Model;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace WhereToFly.App.Views
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

            this.Content = new HeightProfileView
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Track = track,
                UseDarkTheme = Styles.ThemeHelper.CurrentTheme == AppTheme.Dark,
            };

            this.SetDynamicResource(BackgroundColorProperty, "PageBackgroundColor");

            this.AutomationId = "TrackHeightProfile";
        }
    }
}
