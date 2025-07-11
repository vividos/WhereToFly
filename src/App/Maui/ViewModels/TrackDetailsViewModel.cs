﻿using System.Windows.Input;
using WhereToFly.App.Logic;
using WhereToFly.App.Services;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.ViewModels
{
    /// <summary>
    /// View model for the track details page
    /// </summary>
    public class TrackDetailsViewModel : ViewModelBase
    {
        #region Binding properties
        /// <summary>
        /// Property containing track name
        /// </summary>
        public string Name
        {
            get
            {
                return this.Track.Name;
            }
        }

        /// <summary>
        /// Returns image source for SvgImage in order to display the type image
        /// </summary>
        public ImageSource TypeImageSource { get; }

        /// <summary>
        /// Property that specifies if the color box is visible
        /// </summary>
        public bool IsColorBoxVisible => !this.Track.IsFlightTrack;

        /// <summary>
        /// Property that contains the track's color
        /// </summary>
        public Color TrackColor
            => this.Track.IsFlightTrack
            ? Colors.Transparent
            : Color.FromArgb(this.Track.Color);

        /// <summary>
        /// Command that is called when the user taps on the color box
        /// </summary>
        public ICommand ColorBoxTappedCommand { get; set; }

        /// <summary>
        /// Command that is called when the user taps on the track name box
        /// </summary>
        public ICommand TrackNameTappedCommand { get; set; }

        /// <summary>
        /// Property containing distance
        /// </summary>
        public string Distance
        {
            get
            {
                return DataFormatter.FormatDistance(this.Track.LengthInMeter);
            }
        }

        /// <summary>
        /// Property containing duration
        /// </summary>
        public string Duration
        {
            get
            {
                return DataFormatter.FormatDuration(this.Track.Duration);
            }
        }

        /// <summary>
        /// Track to display height profile for
        /// </summary>
        public Track Track { get; private set; }

        /// <summary>
        /// Command that is called when height profile should be opened in a new page
        /// </summary>
        public ICommand OpenHeightProfileCommand { get; set; }

        /// <summary>
        /// Property containing WebViewSource of location description
        /// </summary>
        public WebViewSource DescriptionWebViewSource
        {
            get; private set;
        }

        /// <summary>
        /// Indicates if the height profile view is displayed using a dark theme
        /// </summary>
        public bool UseDarkTheme
        {
            get
            {
                var userInterface = DependencyService.Get<IUserInterface>();
                return userInterface.IsDarkTheme;
            }
        }
        #endregion

        /// <summary>
        /// Creates a new view model object based on the given track object
        /// </summary>
        /// <param name="track">track object</param>
        public TrackDetailsViewModel(Track track)
        {
            this.Track = track;

            this.TypeImageSource = SvgImageCache.GetImageSource(track);

            this.ColorBoxTappedCommand =
                this.TrackNameTappedCommand = new AsyncRelayCommand(
                    this.EditTrackInfos);

            this.OpenHeightProfileCommand = new AsyncRelayCommand(
                this.OpenHeightProfilePage);

            this.DescriptionWebViewSource = new HtmlWebViewSource
            {
                Html = FormatTrackDescription(this.Track),
                BaseUrl = "about:blank",
            };
        }

        /// <summary>
        /// Called to edit track infos
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task EditTrackInfos()
        {
            var editedTrack = await NavigationService.Instance.NavigateToPopupPageAsync<Track?>(
                PopupPageKey.SetTrackInfosPopupPage,
                true,
                this.Track);

            if (editedTrack != null)
            {
                this.Track = editedTrack;

                this.OnPropertyChanged(nameof(this.Name));
                this.OnPropertyChanged(nameof(this.TrackColor));

                var dataService = DependencyService.Get<IDataService>();
                var trackDataService = dataService.GetTrackDataService();

                await trackDataService.Update(this.Track);

                var appMapService = DependencyService.Get<IAppMapService>();
                appMapService.MapView.UpdateTrack(this.Track);
            }
        }

        /// <summary>
        /// Opens height profile page
        /// </summary>
        /// <returns>task to wait on</returns>
        private async Task OpenHeightProfilePage()
        {
            await NavigationService.Instance.NavigateAsync(
                PageKey.TrackHeightProfilePage,
                true,
                this.Track);
        }

        /// <summary>
        /// Formats track description
        /// </summary>
        /// <param name="track">track to format description</param>
        /// <returns>formatted description text</returns>
        private static string FormatTrackDescription(Track track)
        {
            string desc = HtmlConverter.FromHtmlOrMarkdown(track.Description);
            desc = desc.Replace("\n", "<br/>");

            return HtmlConverter.AddTextColorStyles(
                desc,
                App.GetResourceColor("ElementTextColor", true),
                App.GetResourceColor("PageBackgroundColor", true),
                App.GetResourceColor("AccentColor", true));
        }
    }
}
