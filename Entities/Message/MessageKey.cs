using System;
using Microsoft.EntityFrameworkCore;

namespace Megastonks.Entities.Message
{
    [Owned]
    public class MessageKey
    {
        public Message Message { get; set; }
        public string PublicKey { get; set; }
        public string EncryptionKey { get; set; }
    }
}