using CommunityToolkit.Maui.Behaviors;

namespace WhereToFly.App.Controls
{
    /// <summary>
    /// Image that can be tinted with a color
    /// </summary>
    public class TintedImage : Image
    {
        /// <summary>
        /// Bindable property to set tint color
        /// </summary>
        public static readonly BindableProperty TintColorProperty =
            BindableProperty.Create(
                nameof(TintedImage.TintColor),
                typeof(Color),
                typeof(TintedImage),
                null,
                propertyChanged: OnTintColorPropertyChanged);

        /// <summary>
        /// Tint color; when null or transparent, the image is not tinted
        /// </summary>
        public Color? TintColor
        {
            get => (Color?)this.GetValue(TintColorProperty);
            set => this.SetValue(TintColorProperty, value);
        }

        /// <summary>
        /// Called when the tint color has changed; static verison
        /// </summary>
        /// <param name="bindable">bindable object</param>
        /// <param name="oldValue">old value</param>
        /// <param name="newValue">new value</param>
        private static void OnTintColorPropertyChanged(
            BindableObject bindable,
            object oldValue,
            object newValue)
        {
            ((TintedImage)bindable).OnTintColorPropertyChanged(
                (Color?)oldValue,
                (Color?)newValue);
        }

        /// <summary>
        /// Called when the tint color has changed
        /// </summary>
        /// <param name="oldValue">old color</param>
        /// <param name="newValue">new color</param>
        private void OnTintColorPropertyChanged(
            Color? oldValue,
            Color? newValue)
        {
            if (oldValue == newValue)
            {
                return;
            }

            this.Behaviors.Clear();

            if (newValue != null &&
                newValue != Colors.Transparent)
            {
                var behavior = new IconTintColorBehavior
                {
                    TintColor = newValue,
                };

                this.Behaviors.Add(behavior);
            }
        }
    }
}
