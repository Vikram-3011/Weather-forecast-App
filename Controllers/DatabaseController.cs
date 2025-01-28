using MongoDB.Driver;
using BlazorApp.Models;
using MongoDB.Bson;
using System.Threading.Tasks;
using System.Collections.Generic;
using BlazorApp.Components.Pages;
using BlazorApp.Singletons; 
namespace BlazorApp.Controllers
{
    public class DatabaseController
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<Favourite> _favoriteCities;

        public DatabaseController(IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("MongoDB");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "MongoDB connection string is missing in appsettings.");
            }
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("authentication");

            _favoriteCities = database.GetCollection<Favourite>("fav cities");
        }

        public async Task AddFavoriteCity(string cityName, string userEmail)
        {
            var city = new Favourite
            {
                CityName = cityName,
                UserEmail = userEmail,
                DateAdded = DateTime.UtcNow
            };
            await _favoriteCities.InsertOneAsync(city);
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
    }
}