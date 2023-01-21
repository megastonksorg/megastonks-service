using AutoMapper;
using Megastonks.Helpers;
using Megastonks.Models.Tribe;

namespace Megastonks.Services
{
    public interface ITribeService
    {
        TribeResponse CreateTribe(string name);
    }

    public class TribeService : ITribeService
    {
        private readonly ILogger<AccountService> _logger;
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public TribeService(ILogger<AccountService> logger, DataContext context, IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
        }

        public TribeResponse CreateTribe(string name)
        {
            throw new NotImplementedException();
        }
    }
}