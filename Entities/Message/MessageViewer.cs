using System;
using Microsoft.EntityFrameworkCore;

namespace Megastonks.Entities
{
    [Owned]
    public class MessageViewer
    {
        public Message Message { get; set; }
        public Account Account { get; set; }
    }
}