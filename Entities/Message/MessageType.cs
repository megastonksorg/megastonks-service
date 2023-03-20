using System;
using System.Text.Json.Serialization;

namespace Megastonks.Entities
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MessageType
    {
        text,
        image,
        video,
        systemEvent
    }
}