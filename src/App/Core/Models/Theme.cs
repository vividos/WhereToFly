namespace WhereToFly.App.Core.Models
{
    /// <summary>
    /// App style theme enumeration
    /// </summary>
    public enum Theme
    {
        /// <summary>
        /// Use same theme as the device; when it doesn't support app themes, uses Light
        /// </summary>
        Device,

        /// <summary>
        /// Light theme
        /// </summary>
        Light,

        /// <summary>
        /// Dark theme
        /// </summary>
        Dark,
    }
}
