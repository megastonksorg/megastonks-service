using System.ComponentModel.DataAnnotations.Schema;

namespace Megastonks.Entities
{
    public class Tribe
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public List<TribeMember> TribeMembers { get; set; }
    }
}