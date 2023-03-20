using System.Text.Json.Serialization;

namespace Megastonks.Entities
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Role
    {
        Admin,
        User
    }
}