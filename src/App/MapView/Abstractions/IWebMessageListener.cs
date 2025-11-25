namespace WhereToFly.App.MapView.Abstractions
{
    /// <summary>
    /// Interface to a web message listener
    /// </summary>
    internal interface IWebMessageListener
    {
        /// <summary>
        /// Called when the web view receives a web message from JavaScript side
        /// </summary>
        /// <param name="webMessageAsJson">web message as JSON</param>
        void OnReceivedWebMessage(string webMessageAsJson);
    }
}
