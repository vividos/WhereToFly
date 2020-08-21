using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WhereToFly.App.Core.ViewModels;
using WhereToFly.App.Geo;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Popup page for adding a newly loaded track and edit its properties.
    /// </summary>
    public partial class AddTrackPopupPage : BasePopupPage
    {
        /// <summary>
        /// View model for this popup page
        /// </summary>
        private readonly AddTrackPopupViewModel viewModel;

        /// <summary>
        /// All colors to display in the color picker for the track color
        /// </summary>
        private static readonly string[] AllColorPickerColors = new string[]
        {
            "C70039", "FF5733", "FF8D1A", "FFC300",
            "EDDD52", "ADD45D", "56C785", "00BAAD",
            "2A7A9B", "3C3D6B", "52184A", "900C3E",
        };

        /// <summary>
        /// Task completion source to report back edited track
        /// </summary>
        private TaskCompletionSource<Track> tcs;

        /// <summary>
        /// Creates a new popup page to edit track properties
        /// </summary>
        /// <param name="track">track to edit</param>
        public AddTrackPopupPage(Track track)
        {
            this.CloseWhenBackgroundIsClicked = true;

            this.InitializeComponent();

            this.BindingContext = this.viewModel = new AddTrackPopupViewModel(track);

            this.SetupColorPicker();
        }

        /// <summary>
        /// Shows "add track" popup page and lets the user edit the track properties.
        /// </summary>
        /// <param name="track">track to edit</param>
        /// <returns>entered text, or null when user canceled the popup dialog</returns>
        public static async Task<Track> ShowAsync(Track track)
        {
            var popupPage = new AddTrackPopupPage(track)
            {
                tcs = new TaskCompletionSource<Track>()
            };

            await popupPage.Navigation.PushPopupAsync(popupPage);

            return await popupPage.tcs.Task;
        }

        /// <summary>
        /// Sets up color picker control, which actually is a FlexLayout containing several
        /// buttons with a border.
        /// </summary>
        private void SetupColorPicker()
        {
            foreach (string color in AllColorPickerColors)
            {
                var button = new Button
                {
                    WidthRequest = 40.0,
                    HeightRequest = 40.0,
                    BackgroundColor = Color.FromHex(color),
                };

                button.Clicked += this.OnClicked_ColorPickerButton;

                var frame = new Frame
                {
                    Padding = new Thickness(5),
                    BorderColor = Color.White,
                    CornerRadius = 5.0f,
                    HasShadow = false,
                    Content = button
                };

                this.colorPickerLayout.Children.Add(frame);
            }

            this.SelectColorFrame(this.colorPickerLayout.Children.First() as Frame);
        }

        /// <summary>
        /// Called when a color picker button control has been clicked
        /// </summary>
        /// <param name="sender">sender object; the button being clicked</param>
        /// <param name="args">event args</param>
        private void OnClicked_ColorPickerButton(object sender, EventArgs args)
        {
            var button = sender as Button;
            var selectedFrame = button.Parent as Frame;

            this.SelectColorFrame(selectedFrame);
        }

        /// <summary>
        /// Marks a new color element (button inside a frame) as the selected element.
        /// </summary>
        /// <param name="selectedFrame">selected frame</param>
        private void SelectColorFrame(Frame selectedFrame)
        {
            // set all frame colors to white except the selected one
            foreach (Frame frame in this.colorPickerLayout.Children.Cast<Frame>())
            {
                Debug.Assert(frame != null, "view element must be a Frame");

                frame.BorderColor = frame == selectedFrame ? Color.Black : Color.White;
            }

            // update view model
            int colorIndex = this.colorPickerLayout.Children.IndexOf(selectedFrame);

            this.viewModel.SelectedTrackColor = AllColorPickerColors[colorIndex];
        }

        /// <summary>
        /// Called when user clicked on the background, dismissing the popup page.
        /// </summary>
        /// <returns>whatever the base class returns</returns>
        protected override bool OnBackgroundClicked()
        {
            if (!this.tcs.Task.IsCompleted)
            {
                this.tcs.SetResult(null);
            }

            return base.OnBackgroundClicked();
        }

        /// <summary>
        /// Called when user naviaged back with the back button, dismissing the popup page.
        /// </summary>
        /// <returns>whatever the base class returns</returns>
        protected override bool OnBackButtonPressed()
        {
            if (!this.tcs.Task.IsCompleted)
            {
                this.tcs.SetResult(null);
            }

            return base.OnBackButtonPressed();
        }

        /// <summary>
        /// Called when user clicked on the "Add track" button, ending the popup page.
        /// </summary>
        /// <param name="sender">sender object</param>
        /// <param name="args">event args</param>
        private async void OnClickedAddTrackButton(object sender, EventArgs args)
        {
            this.viewModel.UpdateTrack();

            if (!this.tcs.Task.IsCompleted)
            {
                this.tcs.SetResult(this.viewModel.Track);
            }

            await this.Navigation.PopPopupAsync();
        }
    }
}
