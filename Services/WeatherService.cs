using System.Net.Http.Json;
using System.Text.Json.Serialization;
using BlazorApp.Models;

namespace BlazorApp.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private const string ApiKey = "b9693035c52f49d390164524240110"; // Replace with your WeatherAPI key
        private const string BaseUrl = "https://api.weatherapi.com/v1/history.json";

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<WeatherData>> GetHistoricalWeatherData(string city, int days)
        {
            var weatherDataList = new List<WeatherData>();

            try
            {
                for (int i = 0; i < days; i++) // Loop based on selected days
                {
                    string date = DateTime.UtcNow.AddDays(-i).ToString("yyyy-MM-dd");
                    string requestUrl = $"{BaseUrl}?key={ApiKey}&q={city}&dt={date}";

                    var response = await _httpClient.GetFromJsonAsync<WeatherApiResponse>(requestUrl);

                    if (response?.Forecast?.Forecastday != null && response.Forecast.Forecastday.Count > 0)
                    {
                        var dayData = response.Forecast.Forecastday[0].Day;

                        weatherDataList.Add(new WeatherData
                        {
                            City = city,
                            Temperature = dayData.AvgTempC,
                            WindSpeed = dayData.MaxWindKph / 3.6, // Convert km/h to m/s
                            Humidity = dayData.AvgHumidity,
                            Date = DateTime.UtcNow.AddDays(-i)
                        });
                    }
                    else
                    {
                        Console.WriteLine($"Warning: No weather data available for {city} on {date}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching weather data: {ex.Message}");
            }

            return weatherDataList;
        }
    }

    public class WeatherApiResponse
    {
        public Forecast Forecast { get; set; }
    }

    public class Forecast
    {
        public List<ForecastDay> Forecastday { get; set; }
    }

    public class ForecastDay
    {
        public string Date { get; set; }
        public DayData Day { get; set; }
    }

    public class DayData
    {
        [JsonPropertyName("avgtemp_c")]
        public double AvgTempC { get; set; } // Ensure correct property mapping

        [JsonPropertyName("maxwind_kph")]
        public double MaxWindKph { get; set; } // Ensure correct property mapping

        [JsonPropertyName("avghumidity")]
        public int AvgHumidity { get; set; }
    }
}
