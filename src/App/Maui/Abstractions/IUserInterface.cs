using WhereToFly.App.Services;

namespace WhereToFly.App.Abstractions
{
    /// <summary>
    /// Interface to the user interface
    /// </summary>
    public interface IUserInterface
    {
        /// <summary>
        /// Gets or sets the user's selected app theme
        /// </summary>
        AppTheme UserAppTheme { get; set; }

        /// <summary>
        /// Returns if the app is currently using a dark theme. This takes into account when the
        /// app theme is set to "Same as device".
        /// </summary>
        bool IsDarkTheme { get; }

        /// <summary>
        /// Returns the navigation service that can be used to navigate to pages and show popup
        /// pages.
        /// </summary>
        INavigationService NavigationService { get; }

        /// <summary>
        /// Displays toast message with given text
        /// </summary>
        /// <param name="message">toast message text</param>
        void DisplayToast(string message);

        /// <summary>
        /// Displays an alert message box that the user can acknowledge
        /// </summary>
        /// <param name="message">message text</param>
        /// <param name="cancel">cancel button text</param>
        /// <returns>task to wait on</returns>
        Task DisplayAlert(string message, string cancel);

        /// <summary>
        /// Displays an alert message box and lets the user accept or cancel the next action
        /// </summary>
        /// <param name="message">message text</param>
        /// <param name="accept">accept button text</param>
        /// <param name="cancel">cancel button text</param>
        /// <returns>true when alert message box was accepted, or false when canceled</returns>
        Task<bool> DisplayAlert(string message, string accept, string cancel);

        /// <summary>
        /// Displays an action sheet with multiple buttons to select from.
        /// </summary>
        /// <param name="title">alert message box title</param>
        /// <param name="cancel">cancel button text</param>
        /// <param name="destruction">
        /// when non-null, specifies a destructive action text to perform
        /// </param>
        /// <param name="buttons">list of action sheet buttons</param>
        /// <returns>
        /// action sheet button text that was selected, or null when action was cancelled
        /// </returns>
        Task<string> DisplayActionSheet(string title, string? cancel, string? destruction, params string[] buttons);
    }
}
