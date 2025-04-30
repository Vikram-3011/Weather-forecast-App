using System;

namespace BlazorApp.Models
{
    public class WeatherData
    {
        public string City { get; set; }  // City name
        public double Temperature { get; set; }  // Temperature in Kelvin (Convert to Celsius in UI)
        public double WindSpeed { get; set; }  // Wind speed in m/s
        public double Humidity { get; set; }  // Humidity in percentage
        public DateTime Date { get; set; }  // Date of the weather data
    }
}
