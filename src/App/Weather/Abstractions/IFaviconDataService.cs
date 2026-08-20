namespace WhereToFly.App.Weather.Abstractions;

/// <summary>
/// Interface to a favicon data service
/// </summary>
internal interface IFaviconDataService
{
    /// <summary>
    /// Retrieves a favicon URL for the given website URL
    /// </summary>
    /// <param name="websiteUrl">website URL</param>
    /// <returns>favicon URL or empty string when none was found</returns>
    Task<string> GetFaviconUrlAsync(string websiteUrl);
}
