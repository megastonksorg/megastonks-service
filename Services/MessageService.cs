using System;
using AutoMapper;
using Megastonks.Entities;
using Megastonks.Entities.Message;
using Megastonks.Helpers;
using Megastonks.Models;
using Megastonks.Models.Message;
using Microsoft.EntityFrameworkCore;

namespace Megastonks.Services
{
    public interface IMessageService
    {
        List<MessageResponse> GetMessages(Account account, string tribeId);
        SuccessResponse DeleteMessage(Account account, string messageId);
        MessageResponse PostMessage(Account account, PostMessageRequest model);
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

        public List<MessageResponse> GetMessages(Account account, string tribeId)
        {
            try
            {
                //Ensure the tribe id is valid and the user is a member of that tribe
                var tribe = _context.Tribes
                    .Include(x => x.TribeMembers)
                    .ThenInclude(y => y.Account)
                    .Where(x => x.Id == Guid.Parse(tribeId) && x.TribeMembers.Any(y => y.Account == account))
                    .FirstOrDefault();

                if (tribe == null)
                {
                    throw new AppException("Invalid Tribe Id");
                }

                var messages = _context.Message
                    .Where(x => x.Tribe == tribe && !x.Deleted && x.TimeStamp > DateTime.UtcNow.AddHours(-24))
                    .ToList();

                List<MessageResponse> messagesResponse = new List<MessageResponse>();
                foreach (var message in messages)
                {
                    messagesResponse.Add(mapMessageToMessageResponse(message));
                }

                return messagesResponse;
            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
                throw new AppException(e.Message);
            }
        }

        public SuccessResponse DeleteMessage(Account account, string messageId)
        {
            try
            {
                var messageToDelete = _context.Message
                    .Include(x => x.Sender)
                    .Where(x => x.Sender == account && x.Id == Guid.Parse(messageId))
                    .FirstOrDefault();

                if (messageToDelete == null)
                {
                    throw new AppException("Invalid Message Id");
                }

                var messages = messageToDelete.Deleted = true;

                _context.Update(messageToDelete);
                _context.SaveChanges();

                return new SuccessResponse
                {
                    Success = true
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
                throw new AppException(e.Message);
            }
        }

        public MessageResponse PostMessage(Account account, PostMessageRequest model)
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

                if (tribe.TimestampId != Guid.Parse(model.TribeTimestampId))
                {
                    throw new AppException("Invalid Tribe TimestampId");
                }

                if (model.ContextId != null)
                {
                    contextMessage = _context.Message
                        .Include(x => x.Sender)
                        .Include(x => x.Keys)
                        .Where(y => y.Id == Guid.Parse(model.ContextId))
                        .FirstOrDefault();
                }

                MessageTag messageTag = (MessageTag)Enum.Parse(typeof(MessageTag), model.Tag);

                var newMessage = new Message
                {
                    Tribe = tribe,
                    Context = contextMessage,
                    Sender = account,
                    Body = model.Body,
                    Caption = model.Caption,
                    Type = (MessageType)Enum.Parse(typeof(MessageType), model.Type),
                    Tag = messageTag,
                    Keys = new List<MessageKey>(),
                    Reactions = new List<MessageReaction>(),
                    Expires = messageTag == MessageTag.tea ? DateTime.UtcNow.AddHours(24) : null,
                    TimeStamp = DateTime.UtcNow
                };

                foreach (var key in model.Keys)
                {
                    newMessage
                        .Keys
                        .Add(
                            new MessageKey
                            {
                                Message = newMessage,
                                PublicKey = key.PublicKey,
                                EncryptionKey = key.EncryptionKey
                            }
                        );
                }

                _context.Add(newMessage);
                _context.SaveChanges();

                return mapMessageToMessageResponse(newMessage);
            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
                throw new AppException(e.Message);
            }
        }

        private MessageResponse mapMessageToMessageResponse(Message message)
        {
            return new MessageResponse
            {
                Id = message.Id.ToString(),
                Body = message.Body,
                Caption = message.Caption,
                Type = message.Type.ToString(),
                SenderWalletAddress = message.Sender.WalletAddress,
                Tag = message.Tag.ToString(),
                Context = message.Context == null ? null : mapMessageToMessageResponse(message.Context),
                Keys = message.Keys.Select(_mapper.Map<MessageKey, MessageKeyModel>).ToList(),
                Reactions = message.Reactions.Select(_mapper.Map<MessageReaction, MessageResponse.Reaction>).ToList(),
                Expires = message.Expires,
                TimeStamp = message.TimeStamp
            };
        }
    }
}