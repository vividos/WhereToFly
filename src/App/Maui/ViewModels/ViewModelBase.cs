using System.ComponentModel;
using Xamarin.Forms;

namespace WhereToFly.App.ViewModels
{
    /// <summary>
    /// Common view model base class; supports notifying xaml via
    /// INotifyPropertyChanged
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Access to the user interface
        /// </summary>
        public IUserInterface UserInterface
            => DependencyService.Get<IUserInterface>();

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
