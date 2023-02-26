namespace Megastonks.Entities
{
    public class TribeInviteCode
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public Account Account { get; set; }
        public Tribe Tribe { get; set; }
        public DateTime Created { get; set; }
        public DateTime Expires { get; set; }

        //TODO: When a user deletes their account, we need to invalidate all the codes they sent out here
    }
}