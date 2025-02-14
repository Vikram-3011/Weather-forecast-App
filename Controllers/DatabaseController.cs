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
        private readonly IMongoCollection<UserFCMToken> _fcmTokens;
        private readonly FirebaseMessagingService _firebaseService;
        

        public DatabaseController(IConfiguration configuration, FirebaseMessagingService firebaseService)
        {
            string connectionString = configuration.GetConnectionString("MongoDB");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "MongoDB connection string is missing in appsettings.");
            }
            var client = new MongoClient(connectionString);
             _database = client.GetDatabase("authentication");

            _fcmTokens = _database.GetCollection<UserFCMToken>("fcm tokens");
            _firebaseService = firebaseService;
            _favoriteCities = _database.GetCollection<Favourite>("fav cities");
            _firebaseService = firebaseService;  // ✅ Fixed: Assign the injected dependency

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

        public async Task StoreFCMToken(string userEmail, string fcmToken)
        {
            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(fcmToken))
            {
                Console.WriteLine("❌ Invalid FCM token or user email");
                return;
            }

            var filter = Builders<UserFCMToken>.Filter.Eq(t => t.UserEmail, userEmail);
            var update = Builders<UserFCMToken>.Update.Set(t => t.FCMToken, fcmToken);
            var options = new UpdateOptions { IsUpsert = true };

            await _fcmTokens.UpdateOneAsync(filter, update, options);
            Console.WriteLine($"✅ FCM token stored for {userEmail}");
        }


        public async Task<string?> GetFCMTokenByUser(string userEmail)
        {
            var filter = Builders<UserFCMToken>.Filter.Eq(t => t.UserEmail, userEmail);
            var userToken = await _fcmTokens.Find(filter).FirstOrDefaultAsync();
            return userToken?.FCMToken;
        }

        public async Task<bool> SendPushNotification(string userEmail, string title, string message)
        {
            string? userToken = await GetFCMTokenByUser(userEmail);
            if (string.IsNullOrEmpty(userToken))
            {
                Console.WriteLine($"❌ No FCM token found for {userEmail}");
                return false;
            }

            try
            {
                await _firebaseService.SendNotification(userToken, title, message);
                Console.WriteLine($"✅ Push notification sent to {userEmail}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to send push notification: {ex.Message}");
                return false;
            }
        }

    }

    public class UserFCMToken
    {
        public string Id { get; set; }
        public string UserEmail { get; set; }
        public string FCMToken { get; set; }
    }
}


