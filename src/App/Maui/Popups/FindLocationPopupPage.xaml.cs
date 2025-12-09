using System.Windows.Input;

namespace WhereToFly.App.Popups
{
    /// <summary>
    /// Popup page for "Find location" function in order to input a location name.
    /// </summary>
    public partial class FindLocationPopupPage : BasePopupPage<string>
    {
        #region Binding properties

        /// <summary>
        /// Command to carry out when user clicked on the find button
        /// </summary>
        public ICommand FindCommand { get; }
        #endregion

        /// <summary>
        /// Creates a new popup page
        /// </summary>
        public FindLocationPopupPage()
        {
            this.InitializeComponent();

            this.FindCommand = new Command(this.OnFind);

            this.BindingContext = this;
        }

        /// <summary>
        /// Called when user clicked on the "Find" button, starting the search
        /// </summary>
        private void OnFind()
        {
            this.SetResult(this.locationEntry.Text);
        }

        /// <summary>
        /// Called when then entry field has loaded; sets focus to the entry field.
        /// </summary>
        private void OnLoadedLocationEntry(object? sender, EventArgs args)
        {
            this.locationEntry.Focus();
        }
    }
}
