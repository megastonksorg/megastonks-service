using System.ComponentModel.DataAnnotations;

namespace Megastonks.Models.Account
{
	public class AuthenticateRequest
	{
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}

