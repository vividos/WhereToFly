using System.Diagnostics;
using System.Text.Json;
using WhereToFly.App.Weather.Abstractions;
using WhereToFly.App.Weather.Serializers;

namespace WhereToFly.App.Weather.Services;

/// <summary>
/// Data service for favicons.
/// </summary>
public class FaviconDataService : IFaviconDataService
{
    /// <summary>
    /// Filename of the default favicon URL cache in the Assets folder
    /// </summary>
    public const string FaviconUrlCacheFilename = "defaultFaviconUrlCache.json";

    /// <summary>
    /// Base URL for the WebApi REST web service
    /// </summary>
#pragma warning disable S1075 // URIs should not be hardcoded
    private const string BaseUrl = "https://wheretoflywebapi.azurewebsites.net";
#pragma warning restore S1075 // URIs should not be hardcoded

    /// <summary>
    /// Filename of the JSON file that stores the favicon cache
    /// </summary>
    private readonly string filename;

    /// <summary>
    /// Task that is completed when the service is initialized
    /// </summary>
    private readonly Task initCompleteTask;

    /// <summary>
    /// HTTP client
    /// </summary>
    private readonly HttpClient client;

    /// <summary>
    /// Favicon cache
    /// </summary>
    private Dictionary<string, string> faviconCache = [];

    /// <summary>
    /// Creates a new favicon backend data service object
    /// </summary>
    public FaviconDataService()
    {
        this.filename = Path.Combine(
            FileSystem.AppDataDirectory,
            "faviconCache.json");

        this.client = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
        };

        this.initCompleteTask = Task.Run(this.Load);
    }

    /// <summary>
    /// Retrieves a favicon URL for the given website URL
    /// </summary>
    /// <param name="websiteUrl">website URL</param>
    /// <returns>favicon URL or empty string when none was found</returns>
    public async Task<string> GetFaviconUrlAsync(string websiteUrl)
    {
        await this.initCompleteTask;

        var uri = new Uri(websiteUrl);
        string baseUri = $"{uri.Scheme}://{uri.Host}/";

        if (uri.Host.ToLowerInvariant() == "localhost")
        {
            return $"{uri.Scheme}://{uri.Host}/favicon.ico";
        }

        if (this.faviconCache.TryGetValue(baseUri, out string? faviconUrl) &&
            !string.IsNullOrEmpty(faviconUrl))
        {
            return faviconUrl;
        }

        try
        {
            faviconUrl = await this.GetFaviconUrlFromBackend(websiteUrl);

            this.faviconCache[baseUri] = faviconUrl;

            await this.Save();

            return faviconUrl;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Returns a favicon URL representing the icon for a given website.
    /// </summary>
    /// <param name="websiteUrl">website to get favicon URL</param>
    /// <returns>favicon URL</returns>
    private async Task<string> GetFaviconUrlFromBackend(string websiteUrl)
    {
        Debug.WriteLine($"Backend: Retrieving favicon for URL {websiteUrl}");

        websiteUrl = System.Net.WebUtility.UrlEncode(websiteUrl);

        return await this.client.GetStringAsync(
            $"/api/FaviconUrl?websiteUrl={websiteUrl}");
    }

    /// <summary>
    /// Loads the favicon cache
    /// </summary>
    /// <returns>task to wait on</returns>
    public async Task Load()
    {
        if (!File.Exists(this.filename))
        {
            this.faviconCache = await GetDefaultFaviconCache();
            await this.Save();
            return;
        }

        string json = await File.ReadAllTextAsync(this.filename);

        var localDictionary = JsonSerializer.Deserialize(
            json,
            ModelsJsonSerializerContext.Default.DictionaryStringString);

        if (localDictionary != null)
        {
            this.faviconCache = localDictionary;
        }
    }

    /// <summary>
    /// Saves the favicon cache
    /// </summary>
    /// <returns>task to wait on</returns>
    private async Task Save()
    {
        string json = JsonSerializer.Serialize(
            this.faviconCache,
            ModelsJsonSerializerContext.Default.DictionaryStringString);

        await File.WriteAllTextAsync(this.filename, json);
    }

    /// <summary>
    /// Returns a default mapping for the favicon cache, e.g. when the app is initialized
    /// the first time.
    /// </summary>
    /// <returns>mapping from base URL to favicon URL</returns>
    private static async Task<Dictionary<string, string>> GetDefaultFaviconCache()
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(FaviconUrlCacheFilename);
            if (stream == null)
            {
                return [];
            }

            using var reader = new StreamReader(stream);
            string json = await reader.ReadToEndAsync();

            return JsonSerializer.Deserialize(
                json,
                ModelsJsonSerializerContext.Default.DictionaryStringString)
                ?? [];
        }
        catch (Exception)
        {
            // this code path is only used in unit tests
            return [];
        }
    }
}
