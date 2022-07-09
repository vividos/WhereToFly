using WhereToFly.App.Core.Models;
using WhereToFly.App.Core.ViewModels;

namespace WhereToFly.App.Core.Views
{
    /// <summary>
    /// Popup page for "select weather icon" function.
    /// </summary>
    public partial class SelectWeatherIconPopupPage : BasePopupPage<WeatherIconDescription>
    {
        /// <summary>
        /// Creates a new web link selection popup page, without using grouping.
        /// </summary>
        public SelectWeatherIconPopupPage()
            : this(null)
        {
        }

        /// <summary>
        /// Creates a new web link selection popup page
        /// </summary>
        /// <param name="group">weather icon group to filter by; may be null to show all groups</param>
        public SelectWeatherIconPopupPage(string group)
        {
            this.BindingContext =
                new SelectWeatherIconViewModel(this.SetResult, group);

            this.InitializeComponent();
        }
    }
}
