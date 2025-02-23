using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlazorApp.Models
{
    public class AlertPreferences
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("UserEmail")]
        public string UserEmail { get; set; } = string.Empty; // ✅ Fix: Add UserEmail

        [BsonElement("TemperatureAlert")]
        public bool TemperatureAlert { get; set; }

        [BsonElement("WindAlert")]
        public bool WindAlert { get; set; }

        [BsonElement("RainAlert")]
        public bool RainAlert { get; set; }

        [BsonElement("AirQualityAlert")]
        public bool AirQualityAlert { get; set; }
    }
}
