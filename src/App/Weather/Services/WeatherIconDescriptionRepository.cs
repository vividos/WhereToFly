using System.Diagnostics;
using System.Text.Json;
using WhereToFly.App.Weather.Models;
using WhereToFly.App.Weather.Serializers;

namespace WhereToFly.App.Weather.Services;

/// <summary>
/// Repository providing all weather icon descriptions available
/// </summary>
internal sealed class WeatherIconDescriptionRepository
{
    /// <summary>
    /// Filename of the JSON file that stores the weather description list
    /// </summary>
    private readonly string filename;

    /// <summary>
    /// Task that is completed when the service is initialized
    /// </summary>
    private readonly Task initCompletedTask;

    /// <summary>
    /// List of weather icon descriptions
    /// </summary>
    private List<WeatherIconDescription> weatherIconDescriptionList = [];

    /// <summary>
    /// Creates a new weather icon description data service
    /// </summary>
    public WeatherIconDescriptionRepository()
    {
        this.filename = Path.Combine(
            FileSystem.AppDataDirectory,
            "weatherIconDescriptions.json");

        this.initCompletedTask = Task.Run(this.Load);
    }

    /// <summary>
    /// Adds a new weather icon description to the weather icon description list
    /// </summary>
    /// <param name="weatherIconDescriptionToAdd">weather icon description to add</param>
    /// <returns>task to wait on</returns>
    public async Task Add(WeatherIconDescription weatherIconDescriptionToAdd)
    {
        await this.initCompletedTask;

        this.weatherIconDescriptionList.Add(weatherIconDescriptionToAdd);
        await this.Save();
    }

    /// <summary>
    /// Retrieves a specific weather icon description
    /// </summary>
    /// <param name="weatherIconDescriptionId">weather icon description ID</param>
    /// <returns>weather icon description from list, or null when none was found</returns>
    public async Task<WeatherIconDescription?> Get(string weatherIconDescriptionId)
    {
        await this.initCompletedTask;

        var weatherIconDescription =
            this.weatherIconDescriptionList.FirstOrDefault(
                weatherIconDescription =>
                weatherIconDescription.Id == weatherIconDescriptionId);

        return weatherIconDescription;
    }

    /// <summary>
    /// Removes a specific weather icon description
    /// </summary>
    /// <param name="weatherIconDescriptionIdToRemove">weather icon description ID</param>
    /// <returns>task to wait on</returns>
    public async Task Remove(string weatherIconDescriptionIdToRemove)
    {
        await this.initCompletedTask;

        this.weatherIconDescriptionList.RemoveAll(
            weatherIconDescription =>
            weatherIconDescription.Id == weatherIconDescriptionIdToRemove);

        await this.Save();
    }

    /// <summary>
    /// Returns a list of all weather icon descriptions
    /// </summary>
    /// <returns>list of weather icon descriptions</returns>
    public async Task<IEnumerable<WeatherIconDescription>> GetList()
    {
        await this.initCompletedTask;

        return this.weatherIconDescriptionList;
    }

    /// <summary>
    /// Loads the weather icon description list
    /// </summary>
    /// <returns>task to wait on</returns>
    public async Task Load()
    {
        if (!File.Exists(this.filename))
        {
            this.weatherIconDescriptionList =
                await GetWeatherIconDescriptionRepository();

            await this.Save();
            return;
        };

        string json =
            await File.ReadAllTextAsync(this.filename);

        var localList = JsonSerializer.Deserialize(
            json,
            ModelsJsonSerializerContext.Default.ListWeatherIconDescription);

        if (localList != null)
        {
            this.weatherIconDescriptionList = localList;
        }
    }

    /// <summary>
    /// Saves the weather icon description list
    /// </summary>
    /// <returns>task to wait on</returns>
    private async Task Save()
    {
        string json = JsonSerializer.Serialize(
            this.weatherIconDescriptionList,
            ModelsJsonSerializerContext.Default.ListWeatherIconDescription);

        await File.WriteAllTextAsync(this.filename, json);
    }

    /// <summary>
    /// Returns the repository of all available weather icon descriptions that can be used
    /// to select weather icons for the customized list
    /// </summary>
    /// <returns>list of all weather icons</returns>
    public static async Task<List<WeatherIconDescription>> GetWeatherIconDescriptionRepository()
    {
        try
        {
            using var stream =
                await FileSystem.OpenAppPackageFileAsync(
                    "weathericons.json");

            if (stream == null)
            {
                return [];
            }

            using var reader = new StreamReader(stream);
            string json = await reader.ReadToEndAsync();

            var weatherIconList = JsonSerializer.Deserialize(
                json,
                ModelsJsonSerializerContext.Default.ListWeatherIconDescription);

            return weatherIconList ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            return new List<WeatherIconDescription>();
        }
    }
}
