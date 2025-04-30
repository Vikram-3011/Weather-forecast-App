using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace BlazorApp.Models
{
    public class WeatherAlertHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userEmail")]
        public string UserEmail { get; set; }

        [BsonElement("cityName")]
        public string CityName { get; set; }

        [BsonElement("headline")]
        public string Headline { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("severity")]
        public string Severity { get; set; }

        [BsonElement("dateRecorded")]
        public DateTime DateRecorded { get; set; }
    }
}
