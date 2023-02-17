using System;
namespace Megastonks.Models.Tribe
{
    public class TribeResponse
    {
        public class Member
        {
            public string FullName { get; set; }
            public Uri ProfilePhoto { get; set; }
            public string PublicKey { get; set; }
            public string WalletAddress { get; set; }
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string TimestampId { get; set; }
        public List<Member> Members { get; set; }
    }
}