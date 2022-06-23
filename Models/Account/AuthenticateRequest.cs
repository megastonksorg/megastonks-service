using System.ComponentModel.DataAnnotations;

namespace Megastonks.Models.Account
{
	public class AuthenticateRequest
	{
        [Required]
        public string WalletAddress { get; set; }

        [Required]
        public string Signature { get; set; }
    }
}

