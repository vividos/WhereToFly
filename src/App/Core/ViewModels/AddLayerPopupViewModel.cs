using WhereToFly.Geo.Model;

namespace WhereToFly.App.Core.ViewModels
{
    /// <summary>
    /// View model for the "add layer" popup page
    /// </summary>
    public class AddLayerPopupViewModel : ViewModelBase
    {
        /// <summary>
        /// Layer being edited
        /// </summary>
        public Layer Layer { get; private set; }

        #region Binding properties
        /// <summary>
        /// Property containing the layer name
        /// </summary>
        public string LayerName
        {
            get => this.Layer.Name;
            set
            {
                this.Layer.Name = value;
                this.OnPropertyChanged(nameof(this.LayerName));
            }
        }
        #endregion

        /// <summary>
        /// Creates a new "add layer" popup page view model
        /// </summary>
        /// <param name="layer">layer to edit</param>
        public AddLayerPopupViewModel(Layer layer)
        {
            this.Layer = layer;
        }
    }
}
