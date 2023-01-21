using Newtonsoft.Json;

namespace Megastonks.Models.Account
{
    public class AuthenticateResponse
    {
        public string WalletAddress { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string ProfilePhoto { get; set; }
        public string Currency { get; set; }

        public bool AcceptTerms { get; set; }
        public bool IsOnboarded { get; set; }
        public string JwtToken { get; set; }

        [JsonIgnore] // refresh token is returned in http only cookie
        public string RefreshToken { get; set; }
    }
}