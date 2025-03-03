using MongoDB.Driver;
using BlazorApp.Models;
using MongoDB.Bson;
using System.Threading.Tasks;
using System.Collections.Generic;
using BlazorApp.Singletons; 
namespace BlazorApp.Controllers
{
    public class DatabaseController
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<Favourite> _favoriteCities;
        private readonly IMongoCollection<AlertPreferences> _alertPreferencesCollection;


        public DatabaseController(IConfiguration configuration)
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

            // ✅ Store default alert preferences when a city is favorited
            var defaultPreferences = new AlertPreferences
            {
                UserEmail = userEmail,
                CityName = cityName,
                TemperatureAlert = false,
                WindAlert = false,
                RainAlert = false,
                AirQualityAlert = false
            };
            await _alertPreferencesCollection.InsertOneAsync(defaultPreferences);

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

            // ✅ Remove alert preferences when a city is removed
            var alertFilter = Builders<AlertPreferences>.Filter.And(
                Builders<AlertPreferences>.Filter.Eq(a => a.CityName, cityName),
                Builders<AlertPreferences>.Filter.Eq(a => a.UserEmail, userEmail)
            );
            await _alertPreferencesCollection.DeleteOneAsync(alertFilter);

        }

        public async Task<List<Favourite>> GetAllFavoriteCities()
        {
            return await _favoriteCities.Find(_ => true).ToListAsync();
        }
        public async Task<AlertPreferences> GetUserAlertPreferences(string userEmail, string cityName)
        {
            var result = await _alertPreferencesCollection
                .Find(x => x.UserEmail == userEmail && x.CityName == cityName)
                .FirstOrDefaultAsync();

            return result ?? new AlertPreferences
            {
                UserEmail = userEmail,
                CityName = cityName,
                TemperatureAlert = false,
                WindAlert = false,
                RainAlert = false,
                AirQualityAlert = false
            };
        }


        public async Task SaveUserAlertPreferences(string userEmail, string cityName, AlertPreferences updatedPreferences)
        {
            var filter = Builders<AlertPreferences>.Filter.And(
                Builders<AlertPreferences>.Filter.Eq(p => p.UserEmail, userEmail),
                Builders<AlertPreferences>.Filter.Eq(p => p.CityName, cityName)
            );

            var update = Builders<AlertPreferences>.Update
                .Set("TemperatureAlert", updatedPreferences.TemperatureAlert)
                .Set("WindAlert", updatedPreferences.WindAlert)
                .Set("RainAlert", updatedPreferences.RainAlert)
                .Set("AirQualityAlert", updatedPreferences.AirQualityAlert);

            var options = new UpdateOptions { IsUpsert = true };

            var result = await _alertPreferencesCollection.UpdateOneAsync(filter, update, options);

            if (result.MatchedCount == 0 && result.UpsertedId != null)
            {
                Console.WriteLine("New alert preferences document created.");
            }
            else
            {
                Console.WriteLine("Existing alert preferences updated.");
            }
        }


        public async Task SaveHistoricalAlert(string cityName, string userEmail, WeatherAlert alert)
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

            //    await _historicalAlerts.InsertOneAsync(history);//
        }

        //public async Task<List<WeatherAlertHistory>> GetHistoricalAlertsByUser(string userEmail)
        //{
        //    var filter = Builders<WeatherAlertHistory>.Filter.Eq(h => h.UserEmail, userEmail);
        //    return await _historicalAlerts.Find(filter).SortByDescending(h => h.DateRecorded).ToListAsync();
        //}
    }

    public class WeatherAlertHistory
    {
        public string UserEmail { get; set; } = "";
        public string CityName { get; set; } = "";
        public string Headline { get; set; } = "";
        public string Description { get; set; } = "";
        public string Severity { get; set; } = "";
        public DateTime DateRecorded { get; set; }
    }
}






