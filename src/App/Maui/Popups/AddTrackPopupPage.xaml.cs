using WhereToFly.App.ViewModels;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Popups
{
    /// <summary>
    /// Popup page for adding a newly loaded track and edit its properties.
    /// </summary>
    public partial class AddTrackPopupPage : BasePopupPage<Track>
    {
        /// <summary>
        /// View model for this popup page
        /// </summary>
        private readonly AddTrackPopupViewModel viewModel;

        /// <summary>
        /// Creates a new popup page to edit track properties
        /// </summary>
        /// <param name="track">track to edit</param>
        public AddTrackPopupPage(Track track)
        {
            this.InitializeComponent();

            this.BindingContext = this.viewModel = new AddTrackPopupViewModel(track);
        }

        /// <summary>
        /// Called when user clicked on the "Add track" button, ending the popup page.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnClickedAddTrackButton(object sender, EventArgs args)
        {
            this.viewModel.UpdateTrack();

            this.SetResult(this.viewModel.Track);
        }
    }
}
