using System.Text.Json.Serialization;

namespace dotnet_rpg.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BaseAttribute
    {
        Strength = 1,
        Agility = 2,
        Intelligence = 3
    }
}
