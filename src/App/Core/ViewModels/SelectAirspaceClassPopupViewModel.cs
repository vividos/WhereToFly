using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using WhereToFly.Geo.Airspace;
using Xamarin.Forms;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the "select airspace classes" popup page
    /// </summary>
    public class SelectAirspaceClassPopupViewModel : ViewModelBase
    {
        /// <summary>
        /// View model for a single airspace class collection view item
        /// </summary>
        public class AirspaceClassViewModel : ViewModelBase
        {
            /// <summary>
            /// Airspace class stored in this view model
            /// </summary>
            public AirspaceClass AirspaceClass { get; }

            /// <summary>
            /// Indicates if this airspace class is selected
            /// </summary>
            private bool isSelected;

            #region Binding properties
            /// <summary>
            /// Airspace class as text
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// Indicates if this airspace class is selected
            /// </summary>
            public bool IsSelected
            {
                get => this.isSelected;
                set
                {
                    Debug.WriteLine($"Set Class = {this.AirspaceClass} to " + (value ? "selected" : "unselected"));
                    this.isSelected = value;
                    this.OnPropertyChanged(nameof(this.TextColor));
                    this.OnPropertyChanged(nameof(this.BackgroundColor));
                }
            }

            /// <summary>
            /// Text color for the item
            /// </summary>
            public Color TextColor => this.IsSelected ? Color.White : Color.FromHex("#2f2f2f");

            /// <summary>
            /// Background color for the item
            /// </summary>
            public Color BackgroundColor => this.IsSelected ? Constants.PrimaryColor : Color.FromHex("#cfcfcf");

            /// <summary>
            /// Command that is carried out when weather icon has been tapped.
            /// </summary>
            public ICommand Tapped { get; }
            #endregion

            /// <summary>
            /// Creates a new view model
            /// </summary>
            /// <param name="airspaceClass">airspace class to use</param>
            /// <param name="isSelected">indicates if item is initially selected</param>
            public AirspaceClassViewModel(AirspaceClass airspaceClass, bool isSelected)
            {
                this.AirspaceClass = airspaceClass;
                this.isSelected = isSelected;

                this.Text = AirspaceClassToDisplayText(airspaceClass);
                this.Tapped = new Command(this.OnTapped);
            }

            /// <summary>
            /// Called when the airspace view has been tapped; toggles select state
            /// </summary>
            private void OnTapped()
            {
                this.IsSelected = !this.IsSelected;
            }

            /// <summary>
            /// Converts airspace class to display text string
            /// </summary>
            /// <param name="airspaceClass">airspace class</param>
            /// <returns>display text</returns>
            private static string AirspaceClassToDisplayText(AirspaceClass airspaceClass)
            {
                return airspaceClass.ToString();
            }
        }

        #region Binding properties
        /// <summary>
        /// List of airspace classes to select
        /// </summary>
        public List<AirspaceClassViewModel> AirspaceClassList { get; private set; }
        #endregion

        /// <summary>
        /// Creates a new "select airspace classes" popup page view model
        /// </summary>
        /// <param name="airspaceClassesList">list of airspace classes to select from</param>
        public SelectAirspaceClassPopupViewModel(IEnumerable<AirspaceClass> airspaceClassesList)
        {
            this.AirspaceClassList =
                airspaceClassesList.Select(airspaceClass => new AirspaceClassViewModel(
                    airspaceClass,
                    IsAirspaceClassInitiallySelected(airspaceClass)))
                .ToList();
        }

        /// <summary>
        /// Determines which airspace classes should initially be selected
        /// </summary>
        /// <param name="airspaceClass">airspace class</param>
        /// <returns>true when initially selected, or false when not</returns>
        private static bool IsAirspaceClassInitiallySelected(AirspaceClass airspaceClass)
        {
            return airspaceClass != AirspaceClass.A &&
                airspaceClass != AirspaceClass.B &&
                airspaceClass != AirspaceClass.C &&
                airspaceClass != AirspaceClass.D;
        }

        /// <summary>
        /// Returns set of selected airspace classes
        /// </summary>
        /// <returns>set of airspace classes</returns>
        public ISet<AirspaceClass> GetSelectedAirspaceClasses()
        {
            return new HashSet<AirspaceClass>(
                this.AirspaceClassList
                .Where(viewModel => viewModel.IsSelected)
                .Select(viewModel => viewModel.AirspaceClass));
        }
    }
}
