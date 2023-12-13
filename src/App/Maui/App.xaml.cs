namespace WhereToFly.App.Maui
{
    /// <summary>
    /// MAUI app
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Creates a new MAUI app object
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            this.MainPage = new MainPage();
        }

        /// <summary>
        /// Called when the app's window is about to be created. Sets the app's title.
        /// </summary>
        /// <param name="activationState">activation state</param>
        /// <returns>window object</returns>
        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = base.CreateWindow(activationState);
            window.Title = "Where-to-fly";

            return window;
        }
    }
}
