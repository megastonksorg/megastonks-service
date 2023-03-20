using System;
using System.Text.Json.Serialization;

namespace Megastonks.Entities
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MessageTag
    {
        tea,
        chat
    }
}