﻿using AutoMapper;
using BC = BCrypt.Net.BCrypt;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Megastonks.Helpers;
using Megastonks.Models.Account;
using Megastonks.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Megastonks.Services
{
	public interface IAccountService
    {
		AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress);
    }

	public class AccountService : IAccountService
	{
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;

        public AccountService(
            DataContext context,
            IMapper mapper,
            IOptions<AppSettings> appSettings)
        {
            _context = context;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }


        public AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress)
        {
            var account = _context.Accounts.SingleOrDefault(x => x.Email == model.Email);

            if (account == null || !account.IsVerified)
                throw new AppException("Email or password is incorrect");

            //Check if account is locked
            if (account.AccountLocked && account.AccountLockTimeout <= DateTime.UtcNow)
            {
                account.AccountLocked = false;
                account.LoginAttempt = 0;
                // save changes to db
                _context.Update(account);
                _context.SaveChanges();
            }
            else if (account.AccountLocked)
            {
                throw new AppException("Your account has been locked. You are Unable to Login at this time. Please wait 1 hour before trying again.");
            }

            // authentication successful so generate jwt and refresh tokens
            var jwtToken = generateJwtToken(account);
            var refreshToken = generateRefreshToken(ipAddress);


            // remove old refresh tokens from account
            removeOldRefreshTokens(account);

            account.RefreshTokens.Add(refreshToken);



            // save changes to db
            _context.Update(account);
            _context.SaveChanges();

            var response = _mapper.Map<AuthenticateResponse>(account);
            response.JwtToken = jwtToken;
            response.RefreshToken = refreshToken.Token;
            return response;
        }

        private string generateJwtToken(Account account)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", account.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private RefreshToken generateRefreshToken(string ipAddress)
        {
            return new RefreshToken
            {
                Token = randomTokenString(),
                Expires = DateTime.UtcNow.AddDays(10),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
        }

        private string randomTokenString()
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[40];
            rngCryptoServiceProvider.GetBytes(randomBytes);
            // convert random bytes to hex string
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }

        private void removeOldRefreshTokens(Account account)
        {
            //Remove all refresh tokens and not only inactive old ones. This should prevent the user from having two active sessions after the jwt tokens have expired
            account.RefreshTokens.RemoveAll(x =>
                x.IsActive &&
                x.Created <= DateTime.UtcNow);

            // account.RefreshTokens.RemoveAll(x =>
            //!x.IsActive &&
            //x.Created.AddDays(_appSettings.RefreshTokenTTL) <= DateTime.UtcNow);
        }
    }
}

