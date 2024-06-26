﻿using System;
using Microsoft.EntityFrameworkCore;

namespace Megastonks.Entities
{
    [Owned]
    public class MessageReaction
    {
        public Message Message { get; set; }
        public Account Sender { get; set; }
        public string Content { get; set; }
    }
}