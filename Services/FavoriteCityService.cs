using MongoDB.Driver;
using BlazorApp.Models;

namespace BlazorApp.Services
{
    public class FavoriteCityService
    {
        private readonly IMongoCollection<Favourite> _favoriteCityCollection;

        public FavoriteCityService()
        {
            var client = new MongoClient("mongodb://localhost:27017/"); // MongoDB connection
            var database = client.GetDatabase("authentication"); // Database name
            _favoriteCityCollection = database.GetCollection<Favourite>("fav cities"); // Collection name
        }

        public async Task<bool> AddFavoriteCityAsync(string userEmail, string cityName)
        {
            try
            {
                if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(cityName))
                    return false;

                var city = new Favourite { UserEmail = userEmail, CityName = cityName };
                await _favoriteCityCollection.InsertOneAsync(city);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding favorite city: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Favourite>> GetFavoriteCitiesByUserAsync(string userEmail)
        {
            try
            {
                return await _favoriteCityCollection.Find(c => c.UserEmail == userEmail).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving favorite cities: {ex.Message}");
                return new List<Favourite>();
            }
        }

        public async Task<bool> RemoveFavoriteCityAsync(string userEmail, string cityName)
        {
            try
            {
                var filter = Builders<Favourite>.Filter.Eq("UserEmail", userEmail) &
                             Builders<Favourite>.Filter.Eq("CityName", cityName);
                var result = await _favoriteCityCollection.DeleteOneAsync(filter);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing favorite city: {ex.Message}");
                return false;
            }
        }
    }
}
