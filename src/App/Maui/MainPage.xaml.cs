namespace WhereToFly.App
{
    /// <summary>
    /// Main page
    /// </summary>
    public partial class MainPage : ContentPage
    {
        /// <summary>
        /// Creates a new main page
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            this.Dispatcher.DispatchAsync(
                async () =>
                {
                    await this.mapView.CreateAsync(
                        new Geo.Model.MapPoint(47.67, 11.88),
                        5000,
                        false);
                });
        }
    }
}
