using System;

namespace Megastonks.Entities
{
    public class MessageViewer
    {
        public int Id { get; set; }
        public Message Message { get; set; }
        public List<Account> Viewers { get; set; }
    }
}