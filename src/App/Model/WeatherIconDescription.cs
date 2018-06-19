namespace WhereToFly.App.Model
{
    /// <summary>
    /// Description for a single weather icon
    /// </summary>
    public class WeatherIconDescription
    {
        /// <summary>
        /// Name of weather icon to display
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Group of weather icon; can be used to group icons
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Weather icon type
        /// </summary>
        public enum IconType
        {
            /// <summary>
            /// Icon is a link to a web page
            /// </summary>
            IconLink,

            /// <summary>
            /// Icon opens another app on the device
            /// </summary>
            IconApp,

            /// <summary>
            /// Icon is a placeholder icon to add more weather icons
            /// </summary>
            IconPlaceholder,
        }

        /// <summary>
        /// Weather icon type
        /// </summary>
        public IconType Type { get; set; }

        /// <summary>
        /// Icon text, e.g. filename
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Web link or app link
        /// </summary>
        public string WebLink { get; set; }
    }
}
