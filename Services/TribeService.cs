using AutoMapper;
using Megastonks.Entities;
using Megastonks.Helpers;
using Megastonks.Models;
using Megastonks.Models.Tribe;
using Microsoft.EntityFrameworkCore;

namespace Megastonks.Services
{
    public interface ITribeService
    {
        TribeResponse CreateTribe(Account account, string name);
        List<TribeResponse> GetTribes(Account account);
        SuccessResponse InviteToTribe(Account account, string tribeId, string code);
        TribeResponse JoinTribe(Account account, string pin, string code);
        SuccessResponse LeaveTribe(Account account, string tribeId);
    }

    public class TribeService : ITribeService
    {
        private readonly ILogger<TribeService> _logger;
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public TribeService(ILogger<TribeService> logger, DataContext context, IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
        }

        public TribeResponse CreateTribe(Account account, string name)
        {
            try
            {
                int tribeLimit = 5;
                if (name == null || name.Trim().Length == 0)
                {
                    throw new AppException("Tribe name cannot be empty or null");
                }

                if (name.Length > 24)
                {
                    throw new AppException("Tribe name is too long. Must be 24 characters or less");
                }

                int tribesCount = _context.Tribes.Where(x => x.TribeMembers.Any(y => y.Account == account)).Count();

                if (tribesCount >= tribeLimit)
                {
                    throw new AppException($"You are only allowed {tribeLimit} tribes");
                }

                var tribeToAdd = new Tribe {
                    Name = name,
                    Created = DateTime.UtcNow
                };

                _context.Tribes.Add(tribeToAdd);
                _context.SaveChanges();

                Tribe tribe = _context.Tribes.Where(x => x.Id == tribeToAdd.Id).FirstOrDefault();
                var firstTribeMember = new TribeMember
                {
                    Tribe = tribe!,
                    Account = account
                };

                tribe.TribeMembers.Add(firstTribeMember);
                _context.Update(tribe);
                _context.SaveChanges();

                var tribeMembers = mapTribeMembersForResponse(tribe.TribeMembers);

                return new TribeResponse
                {
                    Id = tribe.Id.ToString(),
                    Name = tribe.Name,
                    Members = tribeMembers
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw new AppException(e.Message);
            }
        }

        public List<TribeResponse> GetTribes(Account account)
        {
            try
            {
                List<TribeResponse> tribesResponse = new List<TribeResponse>();
                var tribes = _context.Tribes.Where(x => x.TribeMembers.Any(y => y.Account == account)).ToList();
                foreach(var tribe in tribes)
                {
                    tribesResponse.Add(
                        new TribeResponse
                        {
                            Id = tribe.Id.ToString(),
                            Name = tribe.Name,
                            Members = mapTribeMembersForResponse(tribe.TribeMembers)
                        }
                    );
                }
                return tribesResponse;
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message);
                throw new AppException(e.Message);
            }
        }

        public SuccessResponse InviteToTribe(Account account, string tribeId, string code)
        {
            try
            {
                if (string.IsNullOrEmpty(tribeId) || string.IsNullOrEmpty(code))
                {
                    throw new AppException("Invalid tribe ID or code");
                }

                if (_context.TribeInviteCodes.Where(x => x.Code == code).Any())
                {
                    throw new AppException("Error creating your invite code. Please try that again");
                }

                Tribe tribe = _context.Tribes.Find(Guid.Parse(tribeId));

                if (tribe == null)
                {
                    throw new AppException("Invalid Tribe");
                }

                if (!tribe.TribeMembers.Where(x => x.Account == account).Any())
                {
                    throw new AppException("You cannot send invites for a tribe you are not a member of");
                }

                TribeInviteCode inviteCode = new TribeInviteCode
                {
                    Code = code,
                    Account = account,
                    Tribe = tribe,
                    Created = DateTime.UtcNow,
                    Expires = DateTime.UtcNow.AddMinutes(6)
                };

                //Remove All expired invite codes
                var expiredInviteCodes = _context.TribeInviteCodes.Where(x => x.Expires < DateTime.UtcNow).ToList();
                _context.TribeInviteCodes.RemoveRange(expiredInviteCodes);

                _context.TribeInviteCodes.Add(inviteCode);
                _context.SaveChanges();

                return new SuccessResponse
                {
                    Success = true
                };

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw new AppException(e.Message);
            }
        }

        public TribeResponse JoinTribe(Account account, string pin, string code)
        {
            try
            {
                if (string.IsNullOrEmpty(pin) || string.IsNullOrEmpty(code))
                {
                    throw new AppException("Invalid pin code");
                }

                string incomingInviteCodeHashed = EthereumSigner.HashMessage($"{pin}:{code}");
                TribeInviteCode tribeInviteCode = _context.TribeInviteCodes
                    .Where(x => x.Code == incomingInviteCodeHashed)
                    .Include(x => x.Tribe)
                    .FirstOrDefault();

                if (tribeInviteCode == null)
                {
                    throw new AppException("Invalid Invite Code. Please try again");
                }

                if (tribeInviteCode.Expires < DateTime.UtcNow)
                {
                    _context.TribeInviteCodes.Remove(tribeInviteCode);
                    _context.SaveChanges();
                    throw new AppException("Pin Code has expired. Please ask the sender for a new one");
                }

                Tribe tribe = _context.Tribes.Find(tribeInviteCode.Tribe.Id);

                if (tribe == null)
                {
                    throw new AppException("Something went wrong! Invalid Tribe");
                }

                if (tribe.TribeMembers.Where(x => x.Account == account).Any())
                {
                    throw new AppException("Cannot join a tribe you are already a member of");
                }

                TribeMember tribeMember = new TribeMember
                {
                    Tribe = tribe,
                    Account = account
                };

                tribe.TribeMembers.Add(tribeMember);

                _context.TribeInviteCodes.Remove(tribeInviteCode);
                _context.Tribes.Update(tribe);
                _context.SaveChanges();

                var tribeMembers = mapTribeMembersForResponse(tribe.TribeMembers);

                return new TribeResponse
                {
                    Id = tribe.Id.ToString(),
                    Name = tribe.Name,
                    Members = tribeMembers
                };
            }
            catch(Exception e)
            {
                _logger.LogError(e.Message);
                throw new AppException(e.Message);
            }
        }

        public SuccessResponse LeaveTribe(Account account, string tribeId)
        {
            try
            {
                if (tribeId == null)
                {
                    throw new AppException("Null Tribe Id");
                }

                var tribeToLeave = _context.Tribes.Find(Guid.Parse(tribeId));
                if (tribeToLeave != null)
                {
                    var member = tribeToLeave.TribeMembers.Where(x => x.Account == account).FirstOrDefault();
                    if (member == null)
                    {
                        throw new AppException("Cannot leave a tribe you are not a member of");
                    }

                    tribeToLeave.TribeMembers.Remove(member);

                    _context.Update(tribeToLeave);
                    _context.SaveChanges();

                    return new SuccessResponse
                    {
                        Success = true
                    };
                }
                else
                {
                    throw new AppException("Could not find tribe");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw new AppException(e.Message);
            }
        }

        private List<TribeResponse.Member> mapTribeMembersForResponse(List<TribeMember> tribeMembers)
        {
            var membersResponse = new List<TribeResponse.Member>();
            foreach (var member in tribeMembers)
            {
                membersResponse.Add(
                    new TribeResponse.Member
                    {
                        FullName = member.Account.FullName,
                        ProfilePhoto = member.Account.ProfilePhoto,
                        WalletAddress = member.Account.WalletAddress
                    }
                );
            }
            return membersResponse;
        }
    }
}