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
    }
}
