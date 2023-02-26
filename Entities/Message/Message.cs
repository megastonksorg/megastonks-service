﻿using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Megastonks.Entities.Message
{
    public class Message
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public Tribe Tribe { get; set; }
        public MessageContent Content { get; set; } 
        public Message? Context { get; set; } 
        public Account? Sender { get; set; }

        [Column(TypeName = "nvarchar(24)")]
        public MessageTag Tag { get; set; }

        public List<MessageKey> Keys { get; set; }
        public List<MessageReaction> Reactions { get; set; }

        public DateTime? Expires { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}