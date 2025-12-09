using CommunityToolkit.Maui.Core;
using WhereToFly.App.ViewModels;
using WhereToFly.Geo.Model;
#if ANDROID
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using System.Diagnostics;
#endif

namespace WhereToFly.App.Pages
{
    /// <summary>
    /// Page to display track details
    /// </summary>
    public partial class TrackDetailsPage : ContentPage
    {
        /// <summary>
        /// Creates new track details page
        /// </summary>
        /// <param name="track">track to display</param>
        public TrackDetailsPage(Track track)
        {
            this.BindingContext = new TrackDetailsViewModel(track);
            this.InitializeComponent();
        }

        /// <summary>
        /// Called when the height profile view's touch action status has changed. During touch
        /// events on Android, disable swiping on the tabbed page, to let the user pan and zoom
        /// the height profile.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnTouchStatusChanged(
            object? sender,
            TouchStatusChangedEventArgs args)
        {
#if ANDROID
            bool enableTabSwiping =
                args.Status != TouchStatus.Started;

            Debug.WriteLine($"setting IsSwipePagingEnabled = {enableTabSwiping}");

            var tabbedPage = this.Parent as Microsoft.Maui.Controls.TabbedPage;

            tabbedPage?.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>()
                .SetIsSwipePagingEnabled(enableTabSwiping);
#endif
        }
    }
}
