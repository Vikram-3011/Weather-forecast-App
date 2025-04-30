using MongoDB.Driver;
using BlazorApp.Models;
using MongoDB.Bson;
using System.Threading.Tasks;
using System.Collections.Generic;
using BlazorApp.Singletons;
using Microsoft.Extensions.Logging;
namespace BlazorApp.Controllers
{
    public class DatabaseController
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<Favourite> _favoriteCities;
        private readonly IMongoCollection<AlertPreferences> _alertPreferencesCollection;
        private readonly IMongoCollection<WeatherAlertHistory> _historicalAlerts;

        public DatabaseController(IConfiguration configuration, ILogger<DatabaseController> logger)
        {
           
            string connectionString = configuration.GetConnectionString("MongoDB"); // MongoBD connection string path to appsetting file 

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "MongoDB connection string is missing in appsettings.");
            }
            var client = new MongoClient(connectionString);

            _database = client.GetDatabase("authentication");
            _favoriteCities = _database.GetCollection<Favourite>("fav cities");
            _alertPreferencesCollection = _database.GetCollection<AlertPreferences>("Alert city");
            _historicalAlerts = _database.GetCollection<WeatherAlertHistory>("historicalalert");

        }

        public async Task AddFavoriteCity(string cityName, string userEmail)
        {
            if (string.IsNullOrEmpty(cityName) || string.IsNullOrEmpty(userEmail))
                return;

            var filter = Builders<Favourite>.Filter.And(
            Builders<Favourite>.Filter.Eq(f => f.CityName, cityName),
            Builders<Favourite>.Filter.Eq(f => f.UserEmail, userEmail)
            );
            var existingCity = await _favoriteCities.Find(filter).FirstOrDefaultAsync();

            if (existingCity == null) // Only insert if not already present
            {
                var city = new Favourite
                {
                    CityName = cityName,
                    UserEmail = userEmail,
                    DateAdded = DateTime.UtcNow
                };
                await _favoriteCities.InsertOneAsync(city);
            }


        }

        public async Task<List<Favourite>> GetFavoriteCitiesByUser(string userEmail)
        {
            if (string.IsNullOrEmpty(userEmail))
                return new List<Favourite>(); // Return an empty list if email is null/empty

            var filter = Builders<Favourite>.Filter.Eq(f => f.UserEmail, userEmail);
            return await _favoriteCities.Find(filter).ToListAsync();
        }



        public async Task RemoveFavoriteCity(string cityName, string userEmail)
        {
            var filter = Builders<Favourite>.Filter.And(
                Builders<Favourite>.Filter.Eq(f => f.CityName, cityName),
                Builders<Favourite>.Filter.Eq(f => f.UserEmail, userEmail)
            );
            await _favoriteCities.DeleteOneAsync(filter);


        }

        public async Task<List<Favourite>> GetAllFavoriteCities()
        {
            return await _favoriteCities.Find(_ => true).ToListAsync();
        }
        public async Task SaveUserAlertPreferences(string userEmail, List<string> preferences)
        {
            var filter = Builders<AlertPreferences>.Filter.Eq(p => p.UserEmail, userEmail);
            var update = Builders<AlertPreferences>.Update.Set(p => p.Preferences, preferences);
            var options = new UpdateOptions { IsUpsert = true };

            await _alertPreferencesCollection.UpdateOneAsync(filter, update, options);
        }

        public async Task<List<string>> GetUserAlertPreferences(string userEmail)
        {
            var filter = Builders<AlertPreferences>.Filter.Eq(p => p.UserEmail, userEmail);
            var result = await _alertPreferencesCollection.Find(filter).FirstOrDefaultAsync();
            return result?.Preferences ?? new List<string>();
        }



        public async Task SaveHistoricalAlert(string cityName, string userEmail, WeatherAlert alert)
        {
            var filter = Builders<WeatherAlertHistory>.Filter.And(
                Builders<WeatherAlertHistory>.Filter.Eq(h => h.UserEmail, userEmail),
                Builders<WeatherAlertHistory>.Filter.Eq(h => h.CityName, cityName),
                Builders<WeatherAlertHistory>.Filter.Eq(h => h.Headline, alert.Headline),
                Builders<WeatherAlertHistory>.Filter.Eq(h => h.Description, alert.Desc),
                Builders<WeatherAlertHistory>.Filter.Eq(h => h.Severity, alert.Severity)
            );

            // Check if the same alert already exists
            bool alertExists = await _historicalAlerts.Find(filter).AnyAsync();

            if (!alertExists)
            {
                var history = new WeatherAlertHistory
                {
                    UserEmail = userEmail,
                    CityName = cityName,
                    Headline = alert.Headline,
                    Description = alert.Desc,
                    Severity = alert.Severity,
                    DateRecorded = DateTime.UtcNow
                };

                await _historicalAlerts.InsertOneAsync(history);
            }
        }


        public async Task<List<WeatherAlertHistory>> GetHistoricalAlertsByUser(string userEmail)
        {
            var filter = Builders<WeatherAlertHistory>.Filter.Eq(h => h.UserEmail, userEmail);
            return await _historicalAlerts.Find(filter).SortByDescending(h => h.DateRecorded).ToListAsync();
        }
    }
}



