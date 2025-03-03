using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BlazorApp.Models
{
    public class AlertPreferences
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string UserEmail { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public bool TemperatureAlert { get; set; } = false;
        public bool WindAlert { get; set; } = false;
        public bool RainAlert { get; set; } = false;
        public bool AirQualityAlert { get; set; } = false;
    }
}
