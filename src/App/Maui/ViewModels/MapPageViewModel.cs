namespace WhereToFly.App.ViewModels
{
    /// <summary>
    /// View model for the map page
    /// </summary>
    internal class MapPageViewModel : ViewModelBase
    {
        /// <summary>
        /// App map service
        /// </summary>
        private readonly IAppMapService appMapService;

        /// <summary>
        /// Creates a new view model
        /// </summary>
        /// <param name="appMapService">app map service</param>
        public MapPageViewModel(IAppMapService appMapService)
        {
            this.appMapService = appMapService;
        }
    }
}
