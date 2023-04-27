using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Megastonks.Helpers;
using Megastonks.Models.Account;
using Megastonks.Entities;
using Megastonks.Models;
using Microsoft.EntityFrameworkCore;

namespace Megastonks.Services
{
    public interface IAccountService
    {
        AuthenticateResponse RefreshToken(string token, string ipAddress);
        AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress);
        RegisterResponse Register(RegisterRequest model);
        SuccessResponse DoesAccountExist(string walletAddress);
        string UpdateName(Account account, string fullName);
        Uri UpdateProfilePhoto(Account account, string photoUrl);
        EmptyResponse UpdateDeviceToken(Account account, DeviceType deviceType, string deviceToken);
        EmptyResponse DeleteAccount(Account account);
    }

    public class AccountService : IAccountService
    {
        private readonly ILogger<AccountService> _logger;
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly ITribeService _tribeService;
        private readonly AppSettings _appSettings;

        public AccountService(
            ILogger<AccountService> logger,
            DataContext context,
            IMapper mapper,
            ITribeService tribeService,
            IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _tribeService = tribeService;
            _appSettings = appSettings.Value;
        }

        public AuthenticateResponse RefreshToken(string token, string ipAddress)
        {
            try
            {
                var (refreshToken, account) = getRefreshToken(token);

                // replace old refresh token with a new one and save
                var newRefreshToken = generateRefreshToken(ipAddress);

                var newRefreshTokenHashed = new RefreshToken
                {
                    Id = newRefreshToken.Id,
                    Account = newRefreshToken.Account,
                    Token = EthereumSigner.HashMessage(newRefreshToken.Token),
                    Expires = newRefreshToken.Expires,
                    Created = newRefreshToken.Created,
                    CreatedByIp = newRefreshToken.CreatedByIp,
                    Revoked = newRefreshToken.Revoked,
                    RevokedByIp = newRefreshToken.RevokedByIp,
                    ReplacedByToken = newRefreshToken.ReplacedByToken
                };

                refreshToken.Revoked = DateTime.UtcNow;
                refreshToken.RevokedByIp = ipAddress;
                refreshToken.ReplacedByToken = newRefreshTokenHashed.Token;

                removeOldRefreshTokens(account);

                account.RefreshTokens.Add(newRefreshTokenHashed);

                _context.Update(account);
                _context.SaveChanges();

                // generate new jwt
                var jwtToken = generateJwtToken(account);

                var response = _mapper.Map<AuthenticateResponse>(account);
                response.JwtToken = jwtToken;
                response.RefreshToken = newRefreshToken.Token;
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message} \n {e.StackTrace}");
                throw new AppException(message: e.Message);
            }
        }

        public RegisterResponse Register(RegisterRequest model)
        {
            try
            {
                if (isRegisterModelValid(model))
                {
                    if (!model.AcceptTerms)
                    {
                        throw new AppException(message: "User must accept terms to register");
                    }
                    if (_context.Accounts.Any(x => x.WalletAddress == model.WalletAddress))
                    {
                        _logger.LogError($"Error Code: {ErrorCodes.Thanos}");
                        throw new AppException("Oops something went wrong");
                    }

                    // map model to new account object
                    var account = _mapper.Map<Account>(model);

                    // first registered account is an admin
                    var isFirstAccount = _context.Accounts.Count() == 0;
                    account.Currency = "USD";
                    account.Role = isFirstAccount ? Role.admin : Role.user;
                    account.Created = DateTime.UtcNow;
                    account.FullName.Trim();
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
                _logger.LogError($"{e.Message} \n {e.StackTrace}");
                throw new AppException(message: e.Message);
            }
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress)
        {
            try
            {
                if (EthereumSigner.IsSignatureValid(model.PublicKey, model.WalletAddress, model.Signature))
                {
                    var account = _context.Accounts.SingleOrDefault(x => x.WalletAddress == model.WalletAddress);

                    if (account == null)
                        throw new AppException("Account Not Found");

                    // authentication successful so generate jwt and refresh tokens
                    var jwtToken = generateJwtToken(account);
                    var refreshToken = generateRefreshToken(ipAddress);

                    var hashedRefreshToken = new RefreshToken
                    {
                        Id = refreshToken.Id,
                        Account = refreshToken.Account,
                        Token = EthereumSigner.HashMessage(refreshToken.Token),
                        Expires = refreshToken.Expires,
                        Created = refreshToken.Created,
                        CreatedByIp = refreshToken.CreatedByIp,
                        Revoked = refreshToken.Revoked,
                        RevokedByIp = refreshToken.RevokedByIp,
                        ReplacedByToken = refreshToken.ReplacedByToken
                    };

                    //Update the user's public key evertime they login
                    account.PublicKey = model.PublicKey;

                    // remove old refresh tokens from account
                    removeOldRefreshTokens(account);

                    account.RefreshTokens.Add(hashedRefreshToken);

                    //Update the user's tribeTimestampId so all the other tribe members can grab his latest public key
                    updateAllTribeTimestamps(account);
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
            catch (Exception e)
            {
                _logger.LogError($"{e.Message} \n {e.StackTrace}");
                throw new AppException(e.Message);
            }
        }

        public SuccessResponse DoesAccountExist(string walletAddress)
        {
            try
            {
                if (walletAddress != null && EthereumSigner.IsAddressValid(walletAddress))
                {
                    var account = _context.Accounts.SingleOrDefault(x => x.WalletAddress == walletAddress);
                    return new SuccessResponse
                    {
                        Success = account != null
                    };
                }
                else
                {
                    throw new AppException("Invalid Address");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message} \n {e.StackTrace}");
                throw new AppException(e.Message);
            }
        }

        public string UpdateName(Account account, string fullName)
        {
            try
            {
                var user = _context.Accounts.Find(account.Id);
                if (user == null)
                {
                    throw new AppException("Invalid User");
                }
                if (!string.IsNullOrEmpty(fullName))
                {
                    user.FullName = fullName.Trim();
                    _context.Update(user);
                    _context.SaveChanges();
                    return fullName;
                }
                throw new AppException("Name cannot be null or empty");
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message} \n {e.StackTrace}");
                throw new AppException(e.Message);
            }
        }

        public Uri UpdateProfilePhoto(Account account, string photoUrl)
        {
            try
            {
                var user = _context.Accounts.Find(account.Id);
                Uri profilePhotoUrl;
                if (!Uri.TryCreate(photoUrl, UriKind.Absolute, out profilePhotoUrl))
                {
                    throw new AppException("Invalid Photo Url");
                }
                if (user == null)
                {
                    throw new AppException("Invalid User");
                }
                user.ProfilePhoto = profilePhotoUrl;
                _context.Update(user);
                _context.SaveChanges();
                return profilePhotoUrl;
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message} \n {e.StackTrace}");
                throw new AppException(e.Message);
            }
        }

        public EmptyResponse UpdateDeviceToken(Account account, DeviceType deviceType, string deviceToken)
        {
            try
            {
                var user = _context.Accounts.Find(account.Id);

                if (user == null)
                {
                    throw new AppException("Account not found");
                }

                user.DeviceType = DeviceType.apple;
                user.DeviceToken = deviceToken;

                _context.Update(user);
                _context.SaveChanges();

                return new EmptyResponse();
            }
            catch(Exception e)
            {
                _logger.LogError($"{e.Message} \n {e.StackTrace}");
                throw new AppException(e.Message);
            }
        }

        public EmptyResponse DeleteAccount(Account account)
        {
            try
            {
                if (account == null)
                {
                    throw new AppException("Account not found");
                }

                /*
                STEPS TO DELETE A USER
                    1. Remove from Tribe
                    2. Delete Tribe Invite Codes
                    3. Delete all Messages
                    4. Delete Account
                 */
                var user = _context.Accounts.Find(account.Id);

                if (user == null)
                {
                    throw new AppException("Account not found");
                }

                var messages = _context.Messages
                    .Include(x => x.Sender)
                    .Where(x => x.Sender == account)
                    .ToArray();

                var tribeInviteCodes = _context.TribeInviteCodes
                    .Where(x => x.Account == account)
                    .ToArray();

                var tribes = _context.Tribes
                    .Where(x => x.TribeMembers.Any(y => y.Account == account))
                    .ToArray();

                //1. Remove from Tribe
                foreach (var tribe in tribes)
                {
                    _tribeService.LeaveTribe(account, tribe.Id.ToString());
                }

                //2. Delete all Tribe Invite Codes
                _context.RemoveRange(tribeInviteCodes);

                //3. Delete all messages
                foreach (var message in messages)
                {
                    var messageContext = _context.Messages
                        .Include(x => x.Context)
                        .Where(x => x.Context.Id == message.Id)
                        .FirstOrDefault();
                    if (messageContext != null)
                    {
                        messageContext.Context = null;
                        _context.Update(messageContext);
                    }
                }
                _context.RemoveRange(messages);

                //4. Delete Account
                _context.Remove(user);

                _context.SaveChanges();

                return new EmptyResponse();
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message} \n {e.StackTrace}");
                throw new AppException(e.Message);
            }
        }

        private string generateJwtToken(Account account)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim("id", account.Id.ToString()),
                    new Claim(ClaimTypes.Role, account.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(10),
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
                Expires = DateTime.UtcNow.AddDays(365),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                RevokedByIp = "",
                ReplacedByToken = ""
            };
        }

        private (RefreshToken, Account) getRefreshToken(string token)
        {
            string invalidTokenMessage = "Invalid token";
            string tokenHashed = EthereumSigner.HashMessage(token);
            var account = _context.Accounts.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == tokenHashed));
            if (account == null) throw new AppException(invalidTokenMessage);
            var refreshToken = account.RefreshTokens.Single(x => x.Token == tokenHashed);
            if (!refreshToken.IsActive) throw new AppException(invalidTokenMessage);
            return (refreshToken, account);
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
            //Remove all active refresh tokens. This should prevent the user from having two active sessions after the jwt tokens have expired
            account.RefreshTokens.RemoveAll(x =>
                x.IsActive &&
                x.Created <= DateTime.UtcNow);
        }

        private void updateAllTribeTimestamps(Account account)
        {
            //Update the Tribe Timestamp for all of this user's Tribes
            var tribes = _context.Tribes.Where(x => x.TribeMembers.Any(y => y.Account == account));
            foreach (var tribe in tribes)
            {
                tribe.TimestampId = Guid.NewGuid();
                _context.Update(tribe);
            }
        }

        private bool isRegisterModelValid(RegisterRequest model)
        {
            return EthereumSigner.IsAddressValid(model.WalletAddress) && !string.IsNullOrEmpty(model.FullName)
                && isValidUrl(model.ProfilePhoto.OriginalString);
        }

        private static bool isValidUrl(string url)
        {
            Uri? uriResult;
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult) &&
                    (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}