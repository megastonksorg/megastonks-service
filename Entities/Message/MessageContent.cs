using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Megastonks.Entities.Message
{
    [Owned]
    public class MessageContent
    {
        public Message Message { get; set; }

        [Column(TypeName = "nvarchar(24)")]
        public MessageType Type { get; set; }

        public string Body { get; set; }
        public string Caption { get; set; }
    }
}