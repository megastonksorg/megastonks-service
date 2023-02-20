using Microsoft.EntityFrameworkCore;

namespace Megastonks.Entities
{
    [Owned]
    public class TribeMember
    {
        public Tribe Tribe { get; set; }
        public Account Account { get; set; }
        public DateTime Joined { get; set; }
    }
}