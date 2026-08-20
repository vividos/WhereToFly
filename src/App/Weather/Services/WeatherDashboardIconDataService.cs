using WhereToFly.App.Weather.Abstractions;
using WhereToFly.App.Weather.Models;

namespace WhereToFly.App.Weather.Services;

/// <summary>
/// Weather dashboard icon data service
/// </summary>
internal sealed class WeatherDashboardIconDataService : IWeatherDashboardIconDataService
{
    /// <summary>
    /// Weather icon description repository
    /// </summary>
    private readonly WeatherIconDescriptionRepository weatherIconDescriptionRepository;

    /// <summary>
    /// Filename of the JSON file that stores the weather description list
    /// </summary>
    private readonly string filename;

    /// <summary>
    /// Task that is completed when service was initialized
    /// </summary>
    private readonly Task initTask;

    /// <summary>
    /// List of all weather icon IDs
    /// </summary>
    private readonly List<string> weatherIconIdList = [];


    /// <summary>
    /// Creates a new weather dashboard icon data service
    /// </summary>
    /// <param name="weatherIconDescriptionRepository">weather icon description repository</param>
    public WeatherDashboardIconDataService(
        WeatherIconDescriptionRepository weatherIconDescriptionRepository)
    {
        this.weatherIconDescriptionRepository = weatherIconDescriptionRepository;

        this.filename = Path.Combine(
            FileSystem.AppDataDirectory,
            "weatherIconIds.json");

        this.initTask = Task.Run(this.Load);
    }

    /// <summary>
    /// Adds a new weather icon description to the weather dashboard icon list
    /// </summary>
    /// <param name="weatherIconDescriptionToAdd">weather icon description to add</param>
    /// <returns>task to wait on</returns>
    public async Task Add(WeatherIconDescription weatherIconDescriptionToAdd)
    {
        await this.initTask;

        this.weatherIconIdList.Add(
            weatherIconDescriptionToAdd.Id);

        await this.Save();
    }

    /// <summary>
    /// Returns list of all weather dashboard icons
    /// </summary>
    /// <returns>list of weather icon description objects on the dashboard</returns>
    public async Task<IEnumerable<WeatherIconDescription>> GetList()
    {
        await this.initTask;

        var weatherIconDescriptionList =
            await this.weatherIconDescriptionRepository.GetList();

        var weatherIconDescriptionMap =
            weatherIconDescriptionList.ToDictionary(
                weatherIconDescription => weatherIconDescription.Id,
                weatherIconDescription => weatherIconDescription);

        var selectedWeatherIconDescriptionList =
            this.weatherIconIdList
            .Where(weatherIconId => weatherIconDescriptionMap.ContainsKey(weatherIconId))
            .Select(weatherIconId => weatherIconDescriptionMap[weatherIconId]);

        return selectedWeatherIconDescriptionList;
    }

    /// <summary>
    /// Adds new weather icon description list
    /// </summary>
    /// <param name="weatherIconDescriptionList">weather icon description list to add</param>
    /// <returns>task to wait on</returns>
    public async Task AddList(IEnumerable<WeatherIconDescription> weatherIconDescriptionList)
    {
        if (!weatherIconDescriptionList.Any())
        {
            return;
        }

        await this.initTask;

        this.weatherIconIdList.AddRange(
            weatherIconDescriptionList.Select(
                weatherIconDescription =>
                weatherIconDescription.Id));

        await this.Save();
    }

    /// <summary>
    /// Clears list of weather dashboard icons
    /// </summary>
    /// <returns>task to wait on</returns>
    public async Task ClearList()
    {
        await this.initTask;

        this.weatherIconIdList.Clear();

        await this.Save();
    }

    /// <summary>
    /// Saves the weather icon ID list
    /// </summary>
    /// <returns>task to wait on</returns>
    private async Task Save()
    {
        string list = string.Join(
            ',',
            this.weatherIconIdList);

        await File.WriteAllTextAsync(this.filename, list);
    }

    /// <summary>
    /// Loads the weather icon ID list
    /// </summary>
    /// <returns>task to wait on</returns>
    private async Task Load()
    {
        if (!File.Exists(this.filename))
        {
            return;
        }

        string list = await File.ReadAllTextAsync(this.filename);

        this.weatherIconIdList.Clear();

        this.weatherIconIdList.AddRange(
            list.Split(','));
    }
}
