using System;

namespace Megastonks.Models.Message
{
    public class PostMessageReqest
    {
        public string Body { get; set; }
        public string Caption { get; set; }
        public string Type { get; set; }
        public string? ContextId { get; set; }
        public string TribeId { get; set; }
        public string TribeTimestampId { get; set; }
        public List<MessageKeyModel> Keys { get; set; }
    }
}