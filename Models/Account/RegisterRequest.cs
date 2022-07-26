﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Megastonks.Models.Account
{
    public class RegisterRequest
    {
        [Required]
        public string WalletAddress { get; set; }

        [Required]
        public Uri ProfileImage { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string UserName { get; set; }
    }
}