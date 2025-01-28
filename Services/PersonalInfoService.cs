using MongoDB.Driver;
using BlazorApp.Models;
using MongoDB.Bson;

namespace BlazorApp.Services
{
    public class PersonalInfoService
    {
        private readonly IMongoCollection<PersonalInfo> _personalInfoCollection;

        public PersonalInfoService()
        {
            var client = new MongoClient("mongodb://localhost:27017/"); // MongoDB connection string
            var database = client.GetDatabase("authentication"); // Database name
            _personalInfoCollection = database.GetCollection<PersonalInfo>("user details"); // Collection name
        }

        public async Task<bool> SavePersonalInfoAsync(PersonalInfo personalInfo)
        {
            try
            {
                await _personalInfoCollection.InsertOneAsync(personalInfo);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<PersonalInfo> GetUserByIdAsync(string objectId)
        {
            var filter = Builders<PersonalInfo>.Filter.Eq("_id", ObjectId.Parse(objectId));
            return await _personalInfoCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<PersonalInfo?> GetUserByEmailAsync(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    throw new ArgumentException("Email cannot be null or empty.");
                }

                // Matching case-sensitive email field in MongoDB
                var filter = Builders<PersonalInfo>.Filter.Eq("Email", email);
                return await _personalInfoCollection.Find(filter).FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving user by email: {ex.Message}");
                return null;
            }
        }
        public async Task<bool> UpdateUserAsync(PersonalInfo user)
        {
            var filter = Builders<PersonalInfo>.Filter.Eq(u => u.Email, user.Email);
            var update = Builders<PersonalInfo>.Update
                .Set(u => u.Name, user.Name)
                .Set(u => u.Phone, user.Phone)
                .Set(u => u.City, user.City)
                .Set(u => u.Country, user.Country);

            var result = await _personalInfoCollection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

    }
    

}


