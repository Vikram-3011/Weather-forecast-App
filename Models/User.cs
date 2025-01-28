using BlazorApp.Components.Pages;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlazorApp.Models
{
    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Name { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }
        [BsonElement("password")]
        public string Password { get; set; }
        [BsonElement("favourites")]
        public List<Favourite> FavouriteCities { get; set; }
        public User(string email, string password, List<Favourite> favourites)
        {
            this.Email = email;
            this.Password = password;
            this.FavouriteCities = favourites;
        }

    }
}
