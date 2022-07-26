namespace Megastonks.Entities
{
	public class Account
	{
        public int Id { get; set; }
        public string WalletAddress { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public Uri ProfilePhoto { get; set; }
        public string Currency { get; set; }

        public bool AcceptTerms { get; set; }
        public bool IsOnboarded { get; set; }
        public Role Role { get; set; }
        public DateTime? Verified { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; }

        public bool OwnsToken(string token)
        {
            return this.RefreshTokens?.Find(x => x.Token == token) != null;
        }
    }
}