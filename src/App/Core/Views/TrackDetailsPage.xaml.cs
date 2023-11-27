using System.Diagnostics;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.Geo.Model;
using Xamarin.CommunityToolkit.Effects;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using TabbedPage = Xamarin.Forms.TabbedPage;

namespace WhereToFly.App.Core.Views
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
            object sender,
            TouchStatusChangedEventArgs args)
        {
            if (DeviceInfo.Platform != DevicePlatform.Android)
            {
                return;
            }

            bool enableTabSwiping =
                args.Status != TouchStatus.Started;

            Debug.WriteLine($"setting IsSwipePagingEnabled = {enableTabSwiping}");

            var tabbedPage = this.Parent as TabbedPage;

            tabbedPage?
                .On<Xamarin.Forms.PlatformConfiguration.Android>()?
                .SetIsSwipePagingEnabled(enableTabSwiping);
        }
    }
}
