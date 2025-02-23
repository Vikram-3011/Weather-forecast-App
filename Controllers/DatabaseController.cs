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
            string connectionString = configuration.GetConnectionString("MongoDB");

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
        public async Task<AlertPreferences?> GetUserAlertPreferences(string userEmail)
        {
            var filter = Builders<AlertPreferences>.Filter.Eq("UserEmail", userEmail);
            return await _alertPreferencesCollection.Find(filter).FirstOrDefaultAsync();
        }


        public async Task SaveUserAlertPreferences(string userEmail, AlertPreferences preferences)
        {
            var filter = Builders<AlertPreferences>.Filter.Eq(p => p.UserEmail, userEmail);
            var update = Builders<AlertPreferences>.Update
                .Set(p => p.TemperatureAlert, preferences.TemperatureAlert)
                .Set(p => p.WindAlert, preferences.WindAlert)
                .Set(p => p.RainAlert, preferences.RainAlert)
                .Set(p => p.AirQualityAlert, preferences.AirQualityAlert);

            await _alertPreferencesCollection.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
        }



    }


}


