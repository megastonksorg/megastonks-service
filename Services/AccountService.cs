﻿using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Megastonks.Helpers;
using Megastonks.Models.Account;
using Megastonks.Entities;

namespace Megastonks.Services
{
	public interface IAccountService
    {
        string RequestAuthentication();
		AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress);
        RegisterResponse Register(RegisterRequest model);
    }

	public class AccountService : IAccountService
	{
        private readonly ILogger<AccountService> _logger;
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;

        public AccountService(
            ILogger<AccountService> logger,
            DataContext context,
            IMapper mapper,
            IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        public string RequestAuthentication()
        {
            return _appSettings.MessageToSign;
        }

        public RegisterResponse Register(RegisterRequest model)
        {
            try
            {
               if (isRegisterModelValid(model))
               {
                    if (_context.Accounts.Any(x => x.WalletAddress == model.WalletAddress))
                    {
                        throw new AppException(message: "User Exists: Please login");
                    }

                    // map model to new account object
                    var account = _mapper.Map<Account>(model);

                    // first registered account is an admin
                    var isFirstAccount = _context.Accounts.Count() == 0;
                    account.Role = isFirstAccount ? Role.Admin : Role.User;
                    account.Created = DateTime.UtcNow;

                    // save account
                    _context.Accounts.Add(account);
                    _context.SaveChanges();

                    return _mapper.Map<RegisterResponse>(_context.Accounts.FirstOrDefault(x => x.WalletAddress == account.WalletAddress));
                }
               else
               {
                    throw new AppException(message: "Invalid User Data: Please ensure the wallet address is a valid ethereum address and the names are not empty");
               }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw new AppException(message: e.Message);
            }
        }
        public AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress)
        {
            try
            {
                if (EthereumSigner.IsSignatureValid(_appSettings.MessageToSign, model.WalletAddress, model.Signature))
                {
                    var account = _context.Accounts.SingleOrDefault(x => x.WalletAddress == model.WalletAddress);

                    if (account == null)
                        throw new AppException("Account Not Found");

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
                else
                {
                    throw new AppException("Invalid Signature");
                }
            }
            catch(Exception e)
            {
                throw new AppException(e.Message);
            }
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
        }

        private bool isRegisterModelValid(RegisterRequest model)
        {
            return
                EthereumSigner.IsAddressValid(model.WalletAddress) &&
                !string.IsNullOrEmpty(model.FullName) &&
                !string.IsNullOrEmpty(model.UserName);
        }
    }
}