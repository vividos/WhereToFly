using System;
using System.Collections.Generic;
using System.Linq;
using WhereToFly.App.Core.Models;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for "add weather link" popup page
    /// </summary>
    public class AddWeatherLinkPopupViewModel : ViewModelBase
    {
        /// <summary>
        /// Weather icon description being edited
        /// </summary>
        public WeatherIconDescription WeatherIconDescription { get; set; }

        #region Binding properties
        /// <summary>
        /// Weather icon description name
        /// </summary>
        public string Name
        {
            get => this.WeatherIconDescription.Name;
            set => this.WeatherIconDescription.Name = value;
        }

        /// <summary>
        /// Web link to edit
        /// </summary>
        public string WebLink
        {
            get => this.WeatherIconDescription.WebLink;
            set
            {
                this.WeatherIconDescription.WebLink = value;
                this.OnPropertyChanged(nameof(this.IsValidWebLink));
            }
        }

        /// <summary>
        /// List of all groups to select from
        /// </summary>
        public IEnumerable<string> GroupsList { get; }

        /// <summary>
        /// Currently selected group
        /// </summary>
        public string SelectedGroup
        {
            get => this.WeatherIconDescription.Group;
            set => this.WeatherIconDescription.Group = value;
        }

        /// <summary>
        /// Returns if the currently input web link is valid
        /// </summary>
        public bool IsValidWebLink
        {
            get => Uri.TryCreate(this.WebLink, UriKind.Absolute, out Uri _);
        }
        #endregion

        /// <summary>
        /// Creates a new view model for the "add weather link" page
        /// </summary>
        public AddWeatherLinkPopupViewModel()
        {
            this.GroupsList = new List<string>
            {
                "Weather forecast",
                "Current weather",
                "Webcams",
            };

            this.WeatherIconDescription = new WeatherIconDescription
            {
                Name = string.Empty,
                Group = this.GroupsList.First(),
                Type = WeatherIconDescription.IconType.IconLink,
                WebLink = string.Empty,
            };
        }
    }
}
