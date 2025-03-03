using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

public class WeatherDataService
{
    private readonly IHttpClientFactory _clientFactory;
    public List<CityWeather> GlobalCityWeather { get; private set; } = new List<CityWeather>();

    private readonly List<(string CityName, string Country)> predefinedCities = new()
    {
        ("Tokyo", "Japan"),
        ("Delhi", "India"),
        ("Shanghai", "China"),
        ("Dhaka", "Bangladesh"),
        ("London", "England")
    };

    public WeatherDataService(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task FetchGlobalCityWeather()
    {
        var client = _clientFactory.CreateClient();
        GlobalCityWeather.Clear();

        foreach (var (city, country) in predefinedCities)
        {
            var requestUri = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid=596a1e71b480c1efbdbdd4da71b6f8da&units=metric";

            try
            {
                var response = await client.GetAsync(requestUri);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Response for {city}: {json}"); // Debugging

                    var weatherResponse = JsonSerializer.Deserialize<CityWeatherResponse>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (weatherResponse != null)
                    {
                        GlobalCityWeather.Add(new CityWeather
                        {
                            CityName = city,
                            Country = country,
                            TempMin = weatherResponse.Main.TempMin,
                            TempMax = weatherResponse.Main.TempMax,
                            Description = weatherResponse.Weather[0].Description
                        });
                    }
                }
                else
                {
                    Console.WriteLine($"Failed to fetch weather data for {city}: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching weather data for {city}: {ex.Message}");
            }
        }
    }


}

public class CityWeather
{
    public string CityName { get; set; }
    public string Country { get; set; }
    public float TempMin { get; set; }
    public float TempMax { get; set; }
    public string Description { get; set; }

}

public class CityWeatherResponse
{
    public Main Main { get; set; }
    public List<WeatherDetail> Weather { get; set; }
}

public class Main
{
    [JsonPropertyName("temp_min")]
    public float TempMin { get; set; }

    [JsonPropertyName("temp_max")]
    public float TempMax { get; set; }
}

public class WeatherDetail
{
    [JsonPropertyName("description")]
    public string Description { get; set; }
}