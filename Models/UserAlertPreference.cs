using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class UserAlertPreference
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string UserEmail { get; set; }
    public string CityName { get; set; }
    public bool WeatherAlert { get; set; } = false;
    public bool EnableTemperatureAlert { get; set; } = false;  // ✅ Add this property
    public double MinTemperature { get; set; }
    public double MaxTemperature { get; set; }
    public bool EnableWindSpeedAlert { get; set; } = false;
    public double WindSpeedThreshold { get; set; }
    public bool EnableRainfallAlert { get; set; } = false;
    public double RainfallThreshold { get; set; }
    public bool EnableAQIAlert { get; set; } = false;
    public int AQIThreshold { get; set; }
}
