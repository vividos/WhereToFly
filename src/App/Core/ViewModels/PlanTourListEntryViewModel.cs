using System;
using System.Windows.Input;
using WhereToFly.Geo.Model;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for list entries for the tour planning list
    /// </summary>
    public class PlanTourListEntryViewModel : ViewModelBase
    {
        /// <summary>
        /// Parent view model
        /// </summary>
        private readonly PlanTourPopupViewModel parent;

        /// <summary>
        /// Unique ID for view model, in order to find and compare them
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Location for tour planning
        /// </summary>
        public Location Location { get; private set; }

        #region Binding properties
        /// <summary>
        /// Returns image source for SvgImage in order to display the type image
        /// </summary>
        public ImageSource TypeImageSource { get; }

        /// <summary>
        /// Location name
        /// </summary>
        public string Name { get => this.Location.Name; }

        /// <summary>
        /// Command to move up location in the list
        /// </summary>
        public Xamarin.Forms.Command MoveUpCommand { get; set; }

        /// <summary>
        /// Command to move down location in the list
        /// </summary>
        public Xamarin.Forms.Command MoveDownCommand { get; set; }

        /// <summary>
        /// Command to remove location from the list
        /// </summary>
        public ICommand RemoveCommand { get; set; }
        #endregion

        /// <summary>
        /// Creates a new view model for a list entry
        /// </summary>
        /// <param name="location">location object</param>
        /// <param name="parent">parent view model</param>
        public PlanTourListEntryViewModel(Location location, PlanTourPopupViewModel parent)
        {
            this.Id = Guid.NewGuid().ToString("B");
            this.Location = location;
            this.parent = parent;

            this.TypeImageSource =
                SvgImageCache.GetImageSource(this.Location);

            this.MoveUpCommand = new Xamarin.Forms.Command(
                (obj) => this.parent.MoveUpLocation(this),
                (obj) => !this.parent.IsFirstLocation(this));

            this.MoveDownCommand = new Xamarin.Forms.Command(
                (obj) => this.parent.MoveDownLocation(this),
                (obj) => !this.parent.IsLastLocation(this));

            this.RemoveCommand = new Xamarin.Forms.Command(
                (obj) => this.parent.RemoveLocation(this));
        }

        /// <summary>
        /// Updates binding properties, e.g. when list position has changed
        /// </summary>
        public void Update()
        {
            this.OnPropertyChanged(nameof(this.MoveUpCommand));
            this.OnPropertyChanged(nameof(this.MoveDownCommand));
            this.OnPropertyChanged(nameof(this.RemoveCommand));

            this.MoveUpCommand.ChangeCanExecute();
            this.MoveDownCommand.ChangeCanExecute();
        }
    }
}
