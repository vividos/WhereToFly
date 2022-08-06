namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the "set compass target direction" popup page
    /// </summary>
    public class SetCompassTargetDirectionPopupViewModel : ViewModelBase
    {
        /// <summary>
        /// Compass direction backing field
        /// </summary>
        private int compassDirection;

        #region Binding properties
        /// <summary>
        /// Property containing the compass direction
        /// </summary>
        public int CompassDirection
        {
            get => this.compassDirection;
            set
            {
                this.compassDirection = value;
                this.OnPropertyChanged(nameof(this.CompassDirection));
            }
        }
        #endregion

        /// <summary>
        /// Creates a new "set compass target direction" popup page view model
        /// </summary>
        /// <param name="compassDirection">compass direction</param>
        public SetCompassTargetDirectionPopupViewModel(int compassDirection)
        {
            this.compassDirection = compassDirection;
        }
    }
}
