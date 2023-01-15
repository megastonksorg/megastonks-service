using System;
using System.ComponentModel.DataAnnotations;

namespace Megastonks.Models.Account
{
    public class RegisterRequest
    {
        [Required]
        public string WalletAddress { get; set; }

        [Required]
        public Uri ProfilePhoto { get; set; }

        [Required]
        public string FullName { get; set; }

        public bool AcceptTerms { get; set; }
    }
}