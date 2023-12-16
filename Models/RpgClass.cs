using System.Text.Json.Serialization;

namespace dotnet_rpg.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum RpgClass
    {
        Tank = 1,
        Knight = 2,
        Dodger = 3,
        Mage = 4,
        Healer = 5
    }
}
