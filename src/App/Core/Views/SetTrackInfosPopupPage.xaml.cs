using System;
using System.Threading.Tasks;
using WhereToFly.App.ViewModels;
using WhereToFly.Geo.Model;

namespace WhereToFly.App.Views
{
    /// <summary>
    /// Popup page for setting track infos.
    /// </summary>
    public partial class SetTrackInfosPopupPage : BasePopupPage<Track>
    {
        /// <summary>
        /// View model for this popup page
        /// </summary>
        private readonly SetTrackInfoPopupViewModel viewModel;

        /// <summary>
        /// Creates a new popup page to edit track infos
        /// </summary>
        /// <param name="track">track to edit</param>
        public SetTrackInfosPopupPage(Track track)
        {
            this.InitializeComponent();

            this.BindingContext = this.viewModel = new SetTrackInfoPopupViewModel(track);
        }

        /// <summary>
        /// Called when user clicked on the "Set track info" button, ending the popup page.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnClickedSetTrackInfoButton(object sender, EventArgs args)
        {
            this.SetResult(this.viewModel.Track);
        }
    }
}
