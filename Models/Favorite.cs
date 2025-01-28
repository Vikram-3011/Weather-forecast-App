using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlazorApp.Models
{
    public class Favourite
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

       
            public string CityName { get; set; }
            public string UserEmail { get; set; }
            public DateTime DateAdded { get; set; }
        }
    }


