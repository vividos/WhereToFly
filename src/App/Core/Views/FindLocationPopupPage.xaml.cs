using System;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Popup page for "Find location" function in order to input a location name.
    /// </summary>
    public partial class FindLocationPopupPage : BasePopupPage<string>
    {
        /// <summary>
        /// Creates a new popup page
        /// </summary>
        public FindLocationPopupPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Called when user clicked on the "Find" button, starting the search
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private void OnClickedFindButton(object sender, EventArgs args)
        {
            this.SetResult(this.locationEntry.Text);
        }

        /// <summary>
        /// Called when popup page is about to appear; sets focus to the entry field.
        /// </summary>
        protected override void OnAppearing()
        {
            base.OnAppearing();

            this.locationEntry.Focus();
        }
    }
}
