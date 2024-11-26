using System.Diagnostics;

namespace WhereToFly.App.Controls
{
    /// <summary>
    /// Color picker view that lets the user pick a color from a fixed list of colors, e.g. for
    /// coloring tracks.
    /// </summary>
    public partial class ColorPickerView : ContentView
    {
        #region Binding properties
        /// <summary>
        /// All colors to display in the color picker
        /// </summary>
        public Color[] AllColorPickerColors { get; }

        /// <summary>
        /// Binding property for the border color of the selected color frame
        /// </summary>
        public static readonly BindableProperty SelectionBorderColorProperty =
            BindableProperty.Create(
                propertyName: nameof(SelectionBorderColor),
                returnType: typeof(Color),
                declaringType: typeof(ColorPickerView),
                defaultValue: null);

        /// <summary>
        /// Binding property for the selected color
        /// </summary>
        public static readonly BindableProperty SelectedColorProperty =
            BindableProperty.Create(
                propertyName: nameof(SelectedColor),
                returnType: typeof(Color),
                declaringType: typeof(ColorPickerView),
                defaultBindingMode: BindingMode.TwoWay,
                defaultValue: null,
                propertyChanged: OnSelectedColorPropertyChanged);
        #endregion

        #region View properties
        /// <summary>
        /// Border color property for the selected color frame
        /// </summary>
        public Color SelectionBorderColor
        {
            get => (Color)this.GetValue(SelectionBorderColorProperty);
            set => this.SetValue(SelectionBorderColorProperty, value);
        }

        /// <summary>
        /// Selected color property
        /// </summary>
        public Color SelectedColor
        {
            get => (Color)this.GetValue(SelectedColorProperty);
            set
            {
                this.SetValue(SelectedColorProperty, value);
                this.SelectColor(value);
            }
        }
        #endregion

        /// <summary>
        /// Creates a new color picker view; usually called from placing a XAML element in a page.
        /// </summary>
        public ColorPickerView()
        {
            string[] colors =
            [
                "#C70039", "#FF5733", "#FF8D1A", "#FFC300",
                "#EDDD52", "#ADD45D", "#56C785", "#00BAAD",
                "#2A7A9B", "#3C3D6B", "#52184A", "#900C3E",
            ];

            this.AllColorPickerColors =
                colors.Select(c => Color.FromArgb(c)).ToArray();

            this.InitializeComponent();

            this.colorPickerLayout.BindingContext = this;
        }

        /// <summary>
        /// Called when the selected color property has changed
        /// </summary>
        /// <param name="bindable">bindable object</param>
        /// <param name="oldValue">old color value</param>
        /// <param name="newValue">new color value</param>
        private static void OnSelectedColorPropertyChanged(
            BindableObject bindable,
            object oldValue,
            object newValue)
        {
            var view = bindable as ColorPickerView;
            view?.SelectColor((Color)newValue);
        }

        /// <summary>
        /// Called when a color picker button control has been clicked
        /// </summary>
        /// <param name="sender">sender object; the button being clicked</param>
        /// <param name="args">event args</param>
        private void OnClicked_ColorPickerButton(object? sender, EventArgs args)
        {
            if (sender is Button button)
            {
                this.SelectColor(button.BackgroundColor);
            }
        }

        /// <summary>
        /// Selects a color frame based on the given color
        /// </summary>
        /// <param name="color">color value</param>
        private void SelectColor(Color? color)
        {
            int selectedIndex = color != null
                ? Array.IndexOf(this.AllColorPickerColors, color)
                : -1;

            if (color == null ||
                selectedIndex == -1)
            {
                color = this.AllColorPickerColors[0];
                selectedIndex = 0;
            }

            this.SelectColorFrame(
                color,
                this.colorPickerLayout.Children[selectedIndex] as Border);
        }

        /// <summary>
        /// Marks a new color element (button inside a border) as the selected element.
        /// </summary>
        /// <param name="color">color to set</param>
        /// <param name="selectedBorder">selected border</param>
        private void SelectColorFrame(Color color, Border? selectedBorder)
        {
            // set all frame colors to white except the selected one
            foreach (Border border in this.colorPickerLayout.Children.Cast<Border>())
            {
                border.Stroke = border == selectedBorder
                    ? new SolidColorBrush(this.SelectionBorderColor)
                    : SolidColorBrush.Transparent;
            }

            // update view model
            this.SetValue(SelectedColorProperty, color);
        }
    }
}
