namespace WhereToFly.App.Svg.Sample
{
    /// <summary>
    /// Sample app
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Creates a new app object
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Creates a new window
        /// </summary>
        /// <param name="activationState">activation state</param>
        /// <returns>newly created window</returns>
        protected override Window CreateWindow(
            IActivationState? activationState)
        {
            return new Window(new SamplePage())
            {
                Title = "SvgImage sample app",
            };
        }
    }
}
