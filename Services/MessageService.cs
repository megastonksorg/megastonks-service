using System;
using AutoMapper;
using Megastonks.Helpers;

namespace Megastonks.Services
{
    public interface IMessageService
    {

    }

    public class MessageService : IMessageService
    {
        private readonly ILogger<MessageService> _logger;
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public MessageService(ILogger<MessageService> logger, DataContext context, IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
        }
    }
}