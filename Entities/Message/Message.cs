using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Megastonks.Entities
{
    public class Message
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
    }
}