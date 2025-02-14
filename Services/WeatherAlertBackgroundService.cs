using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

public class WeatherAlertBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public WeatherAlertBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                //var weatherService = scope.ServiceProvider.GetRequiredService<WeatherAPIService>();

                // Ensure WeatherAPIService does not use NavigationManager
                //await weatherService.CheckWeatherAlertsForFavoritesAsync();
            }

            await Task.Delay(TimeSpan.FromHours(3), stoppingToken); // Adjust delay as needed
        }
    }
}
