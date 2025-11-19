using System.ComponentModel;
using WhereToFly.App.Abstractions;

namespace WhereToFly.App.ViewModels
{
    /// <summary>
    /// Common view model base class; supports notifying user interface via
    /// INotifyPropertyChanged
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Service provider
        /// </summary>
        public static IServiceProvider Services
            => IPlatformApplication.Current?.Services
            ?? throw new InvalidOperationException("IServiceProvider is not available");

        /// <summary>
        /// Access to the user interface
        /// </summary>
        public static IUserInterface UserInterface
            => Services.GetRequiredService<IUserInterface>();

        #region INotifyPropertyChanged implementation
        /// <summary>
        /// Event that gets signaled when a property has changed
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Call this method to signal that a property has changed
        /// </summary>
        /// <param name="propertyName">property name; use C# 6 nameof() operator</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
