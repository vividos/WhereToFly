namespace WhereToFly.App.UWP
{
    /// <summary>
    /// Main page of the UWP app
    /// </summary>
    public sealed partial class MainPage
    {
        /// <summary>
        /// Creates new main page
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            this.LoadApplication(new Core.App());
        }
    }
}
