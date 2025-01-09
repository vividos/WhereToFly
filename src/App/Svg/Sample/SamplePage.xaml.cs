namespace WhereToFly.App.Svg.Sample
{
    /// <summary>
    /// Sample page to show sample images
    /// </summary>
    public partial class SamplePage : ContentPage
    {
        /// <summary>
        /// Creates a new sample page
        /// </summary>
        public SamplePage()
        {
            this.InitializeComponent();

            this.layoutToAddImage.Children.Add(
                new SvgImage
                {
                    Source = ImageSource.FromResource(
                        "WhereToFly.App.Svg.Sample.Assets.colibri.svg",
                        typeof(SamplePage).Assembly),
                    WidthRequest = 64,
                    HeightRequest = 64,
                    BackgroundColor = Colors.LightSalmon,
                });
        }
    }
}
