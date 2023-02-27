using System;
using AutoMapper;
using Megastonks.Entities;
using Megastonks.Helpers;
using Megastonks.Models.Message;

namespace Megastonks.Services
{
    public interface IMessageService
    {
        MessageResponse PostMessage(Account account, PostMessageReqest model);
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

        public MessageResponse PostMessage(Account account, PostMessageReqest model)
        {

        }
    }
}