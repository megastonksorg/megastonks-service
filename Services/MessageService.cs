﻿using System;
using AutoMapper;
using Megastonks.Entities;
using Megastonks.Entities.Message;
using Megastonks.Helpers;
using Megastonks.Models.Message;
using Microsoft.EntityFrameworkCore;

namespace Megastonks.Services
{
    public interface IMessageService
    {
        MessageResponse PostMessage(Account account, MessageTag messageTag, PostMessageReqest model);
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

        public MessageResponse PostMessage(Account account, MessageTag messageTag, PostMessageReqest model)
        {
            try
            {
                //Ensure the tribe id is valid and the user is a member of that tribe
                var tribe = _context.Tribes
                    .Include(x => x.TribeMembers)
                    .ThenInclude(y => y.Account)
                    .Where(x => x.Id == Guid.Parse(model.TribeId) && x.TribeMembers.Any(y => y.Account == account))
                    .FirstOrDefault();

                Message contextMessage = null;

                if (tribe == null)
                {
                    throw new AppException("Invalid Tribe Id");
                }

                if (model.ContextId != null)
                {
                    contextMessage = _context.Message
                        .Include(x => x.Sender)
                        .Include(x => x.Keys)
                        .Where(y => y.Id == Guid.Parse(model.ContextId))
                        .FirstOrDefault();
                }

                var newMessage = new Message
                {
                    Tribe = tribe,
                    Context = contextMessage,
                    Sender = account,
                    Body = model.Body,
                    Caption = model.Caption,
                    Type = (MessageType) Enum.Parse(typeof(MessageType), model.Type),
                    Tag = messageTag,
                    Expires = messageTag == MessageTag.tea ? DateTime.UtcNow.AddHours(24) : null,
                    TimeStamp = DateTime.UtcNow
                };

                List<MessageKey> keys = model.Keys.Select(_mapper.Map<MessageKeyModel, MessageKey>).ToList();

                newMessage.Keys.AddRange(keys);

                MessageResponse response = _mapper.Map<Message, MessageResponse>(newMessage);
                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
                throw new AppException(e.Message);
            }
        }
    }
}