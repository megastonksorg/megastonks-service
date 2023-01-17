using System;
using Megastonks.Entities;

namespace Megastonks.Models.Account
{
    public class RegisterResponse
    {
        public string WalletAddress { get; set; }
        public string FullName { get; set; }
        public Uri ProfilePhoto { get; set; }
        public string Currency { get; set; }

        public bool AcceptTerms { get; set; }
        public bool IsOnboarded { get; set; }
        public Role Role { get; set; }
        public DateTime? Verified { get; set; }
    }
}