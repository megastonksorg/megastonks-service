﻿using AutoMapper;
using Megastonks.Entities;
using Megastonks.Helpers;
using Megastonks.Models.Tribe;

namespace Megastonks.Services
{
    public interface ITribeService
    {
        TribeResponse CreateTribe(Account account, string name);
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
                    throw new AppException("Tribe name is too long. Must be 24 characters or less.");
                }

                var currentTribes = _context.Tribes.Where(x => x.TribeMembers.Any(y => y.Account == account));

                if (currentTribes.Count() >= tribeLimit)
                {
                    throw new AppException($"You are only allowed {tribeLimit} tribes.");
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

                var membersResponse = new List<TribeResponse.Member>();
                foreach (var member in tribe.TribeMembers) {
                    membersResponse.Add(
                        new TribeResponse.Member
                        {
                            Id = member.Account.Id,
                            FullName = member.Account.FullName,
                            ProfilePhoto = member.Account.ProfilePhoto
                        }
                    );
                }

                return new TribeResponse
                {
                    Id = tribe.Id.ToString(),
                    Name = tribe.Name,
                    Members = membersResponse
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw new AppException(e.Message);
            }
        }
    }
}