using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MudBlazor;
using BlazorApp.Controllers;

public class WeatherAPIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WeatherAPIService> _logger;
    private readonly ISnackbar _snackbar;
    private readonly DatabaseController _databaseController;

    private const string ApiKey = "b9693035c52f49d390164524240110"; // weather API key 
    private const string BaseUrl = "https://api.weatherapi.com/v1/alerts.json";
    
    public WeatherAPIService(HttpClient httpClient, ILogger<WeatherAPIService> logger, ISnackbar snackbar, DatabaseController databaseController)
    {
        _httpClient = httpClient;
        _logger = logger;
        _snackbar = snackbar;
        _databaseController = databaseController;
    }

    public async Task CheckWeatherAlertsForFavoritesAsync()
    {
        var allFavorites = await _databaseController.GetAllFavoriteCities();

        foreach (var fav in allFavorites)
        {
            await CheckWeatherAlertsAsync(fav.CityName, fav.UserEmail);
        }
    }

    public async Task CheckWeatherAlertsAsync(string city, string userEmail)
    {
        try
        {
            var url = $"{BaseUrl}?key={ApiKey}&q={city}";
            var response = await _httpClient.GetFromJsonAsync<WeatherApiResponse>(url);
            var alerts = response?.Alerts?.Alert;

            if (alerts != null && alerts.Count > 0)
            {
                foreach (var alert in alerts)
                {
                    _snackbar.Add($"**City:** {city}\n**Alert:** {alert.Headline}\n**Description:** {alert.Desc}", Severity.Warning);

                   }
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Error fetching weather alerts for {city}: {ex.Message}");
        }
    }

   


    public async Task<List<WeatherAlert>> CheckWeatherAlertsAsync(string city)
    {
        var alertsList = new List<WeatherAlert>();

        try
        {
            var url = $"{BaseUrl}?key={ApiKey}&q={city}";
            var response = await _httpClient.GetFromJsonAsync<WeatherApiResponse>(url);
            var alerts = response?.Alerts?.Alert;

            if (alerts != null && alerts.Count > 0)
            {
                alertsList.AddRange(alerts);

                foreach (var alert in alerts)
                {
                    _snackbar.Add($"**City:** {city}\n**Alert:** {alert.Headline}\n**Description:** {alert.Desc}", Severity.Warning);
                }
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"Error fetching weather alerts for {city}: {ex.Message}");
        }

        return alertsList;
    }
}

public class WeatherApiResponse
{
    public WeatherAlertContainer? Alerts { get; set; }
}

public class WeatherAlertContainer
{
    public List<WeatherAlert>? Alert { get; set; }
}

public class WeatherAlert
{
    public string Headline { get; set; } = "";
    public string Severity { get; set; } = "";
    public string Urgency { get; set; } = "";
    public string Areas { get; set; } = "";
    public string Event { get; set; } = "";
    public string Effective { get; set; } = "";
    public string Expires { get; set; } = "";
    public string Desc { get; set; } = "";
    public string Instruction { get; set; } = "";
    public string Type { get; set; }
}
