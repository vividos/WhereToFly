using System;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;

namespace WhereToFly.App.Core.Controls
{
    /// <summary>
    /// Color picker view that lets the user pick a color from a fixed list of colors, e.g. for
    /// coloring tracks.
    /// </summary>
    public partial class ColorPickerView : ContentView
    {
        /// <summary>
        /// All colors to display in the color picker
        /// </summary>
        private static readonly string[] AllColorPickerColors = new string[]
        {
            "C70039", "FF5733", "FF8D1A", "FFC300",
            "EDDD52", "ADD45D", "56C785", "00BAAD",
            "2A7A9B", "3C3D6B", "52184A", "900C3E",
        };

        #region Binding properties
        /// <summary>
        /// Binding property for the border color of the selected color frame
        /// </summary>
        public static readonly BindableProperty SelectionBorderColorProperty =
            BindableProperty.Create(
                propertyName: nameof(SelectionBorderColor),
                returnType: typeof(Color),
                declaringType: typeof(ColorPickerView),
                defaultValue: Color.Default);

        /// <summary>
        /// Binding property for the selected color
        /// </summary>
        public static readonly BindableProperty SelectedColorProperty =
            BindableProperty.Create(
                propertyName: nameof(SelectedColor),
                returnType: typeof(Color),
                declaringType: typeof(ColorPickerView),
                defaultBindingMode: BindingMode.TwoWay,
                defaultValue: Color.Default,
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
            this.InitializeComponent();

            this.SetupColorPicker();
        }

        /// <summary>
        /// Sets up color picker control, which actually is a FlexLayout containing several
        /// buttons with a border.
        /// </summary>
        private void SetupColorPicker()
        {
            foreach (string color in AllColorPickerColors)
            {
                var button = new Button
                {
                    WidthRequest = 40.0,
                    HeightRequest = 40.0,
                    BackgroundColor = Color.FromHex(color),
                };

                button.Clicked += this.OnClicked_ColorPickerButton;

                var innerFrame = new Frame
                {
                    Padding = new Thickness(2, 2, 2.5, 2.5), // distance to button
                    Margin = new Thickness(2), // border width
                    CornerRadius = 3.0f,
                    BackgroundColor = this.BackgroundColor,
                    HasShadow = false,
                    Content = button,
                };

                var outerFrame = new Frame
                {
                    Padding = new Thickness(0),
                    Margin = new Thickness(0),
                    CornerRadius = 5.0f,
                    BackgroundColor = Color.Transparent,
                    HasShadow = false,
                    Content = innerFrame,
                };

                this.colorPickerLayout.Children.Add(outerFrame);
            }
        }

        /// <summary>
        /// Called when the selected color property has changed
        /// </summary>
        /// <param name="bindable">bindable object</param>
        /// <param name="oldValue">old color value</param>
        /// <param name="newValue">new color value</param>
        private static void OnSelectedColorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = bindable as ColorPickerView;
            view.SelectColor((Color)newValue);
        }

        /// <summary>
        /// Called when a color picker button control has been clicked
        /// </summary>
        /// <param name="sender">sender object; the button being clicked</param>
        /// <param name="args">event args</param>
        private void OnClicked_ColorPickerButton(object sender, EventArgs args)
        {
            var button = sender as Button;
            this.SelectColor(button.BackgroundColor);
        }

        /// <summary>
        /// Selects a color frame based on the given color
        /// </summary>
        /// <param name="color">color value</param>
        private void SelectColor(Color color)
        {
            int selectedIndex = color != Color.Default
                ? Array.IndexOf(AllColorPickerColors, color.ToHex().Replace("#FF", string.Empty))
                : -1;

            if (selectedIndex == -1)
            {
                selectedIndex = 0;
            }

            this.SelectColorFrame(
                color,
                this.colorPickerLayout.Children[selectedIndex] as Frame);
        }

        /// <summary>
        /// Marks a new color element (button inside a frame) as the selected element.
        /// </summary>
        /// <param name="color">color to set</param>
        /// <param name="selectedFrame">selected frame</param>
        private void SelectColorFrame(Color color, Frame selectedFrame)
        {
            // set all frame colors to white except the selected one
            foreach (Frame outerFrame in this.colorPickerLayout.Children.Cast<Frame>())
            {
                Debug.Assert(outerFrame != null, "view element must be a Frame");

                outerFrame.BackgroundColor = outerFrame == selectedFrame
                    ? this.SelectionBorderColor
                    : Color.Transparent;
            }

            // update view model
            this.SetValue(SelectedColorProperty, color);
        }

        /// <summary>
        /// Called when a property has changed
        /// </summary>
        /// <param name="propertyName">name of changed property</param>
        protected override void OnPropertyChanged(string propertyName = null)
        {
            Debug.WriteLine($"OnPropertyChanged({propertyName})");
            if (propertyName == nameof(this.BackgroundColor))
            {
                // set all inner frame background colors
                foreach (Frame outerFrame in this.colorPickerLayout.Children.Cast<Frame>())
                {
                    Debug.Assert(outerFrame != null, "view element must be a Frame");

                    var innerFrame = outerFrame.Content as Frame;

                    innerFrame.BackgroundColor = this.BackgroundColor;
                }
            }

            base.OnPropertyChanged(propertyName);
        }
    }
}
