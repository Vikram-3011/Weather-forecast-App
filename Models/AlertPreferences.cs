using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlazorApp.Models
{
    public class AlertPreferences
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("UserEmail")]
        public string UserEmail { get; set; } = "";

        [BsonElement("CityName")]
        public string CityName { get; set; } = ""; // Added CityName to support per-city preferences

        [BsonElement("Preferences")]
        public List<string> Preferences { get; set; } = new();

    }
}
