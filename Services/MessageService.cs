using System;
using AutoMapper;
using Megastonks.Entities;
using Megastonks.Helpers;
using Megastonks.Hubs;
using Megastonks.Models;
using Megastonks.Models.Message;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Megastonks.Services
{
    public interface IMessageService
    {
        List<MessageResponse> GetMessages(Account account, string tribeId);
        SuccessResponse DeleteMessage(Account account, string messageId);
        Task PostMessage(Account account, PostMessageRequest model);
        Task AddEventMessage(Tribe tribe, string eventTitle);
    }

    public class MessageService : IMessageService
    {
        private readonly int messageExpiryInHours = 24;
        private readonly ILogger<MessageService> _logger;
        private readonly IHubContext<AppHub> _hubContext;
        private readonly IMapper _mapper;
        private readonly IPushNotificationService _pushNotitificationService;
        private readonly DataContext _context;

        public MessageService(
            ILogger<MessageService> logger,
            IHubContext<AppHub> hubContext,
            IMapper mapper,
            IPushNotificationService pushNotitificationService,
            DataContext context)
        {
            _logger = logger;
            _hubContext = hubContext;
            _mapper = mapper;
            _pushNotitificationService = pushNotitificationService;
            _context = context;
        }

        public List<MessageResponse> GetMessages(Account account, string tribeId)
        {
            try
            {
                //Ensure the tribe id is valid and the user is a member of that tribe
                var tribe = _context.Tribes
                    .AsNoTracking()
                    .Include(x => x.TribeMembers)
                    .ThenInclude(y => y.Account)
                    .Where(x => x.Id == Guid.Parse(tribeId) && x.TribeMembers.Any(y => y.Account == account))
                    .FirstOrDefault();

                if (tribe == null)
                {
                    throw new AppException("Invalid Tribe Id");
                }

                var messages = _context.Message
                    .AsNoTracking()
                    .Include(x => x.Sender)
                    .Where(x => x.Tribe == tribe && !x.Deleted && x.TimeStamp > DateTime.UtcNow.AddHours(-messageExpiryInHours))
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

        public async Task PostMessage(Account account, PostMessageRequest model)
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
                    Expires = DateTime.UtcNow.AddHours(messageExpiryInHours),
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

                await sentToHub(tribe, newMessage);

                //Send Push Notifications
                string notificationBody = newMessage.Tag == MessageTag.tea ? "Hot ☕️" : $"Message from {account.FullName}";
                _pushNotitificationService.SendPushToTribe(account, tribe, messageTag, notificationBody);
            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
                throw new AppException(e.Message);
            }
        }

        public async Task AddEventMessage(Tribe tribe, string eventTitle)
        {
            var eventMessage = new Message
            {
                Tribe = tribe,
                Context = null,
                Sender = null,
                Body = eventTitle,
                Caption = null,
                Type = MessageType.systemEvent,
                Tag = MessageTag.chat,
                Keys = new List<MessageKey>(),
                Reactions = new List<MessageReaction>(),
                Expires = null,
                TimeStamp = DateTime.UtcNow
            };

            _context.Add(eventMessage);
            _context.SaveChanges();

            await sentToHub(tribe, eventMessage);
        }

        private MessageResponse mapMessageToMessageResponse(Message message)
        {
            return new MessageResponse
            {
                Id = message.Id.ToString(),
                Body = message.Body,
                Caption = message.Caption,
                Type = message.Type,
                SenderWalletAddress = message.Sender == null ? "server" : message.Sender.WalletAddress,
                Tag = message.Tag,
                Context = message.Context == null ? null : mapMessageToMessageResponse(message.Context),
                Keys = message.Keys.Select(_mapper.Map<MessageKey, MessageKeyModel>).ToList(),
                Reactions = message.Reactions.Select(_mapper.Map<MessageReaction, MessageResponse.Reaction>).ToList(),
                Expires = message.Expires,
                TimeStamp = message.TimeStamp
            };
        }

        private async Task sentToHub(Tribe tribe, Message message)
        {
            //Send to all tribe members
            string tribeId = tribe.Id.ToString();
            MessageResponse messageToSend = mapMessageToMessageResponse(message);
            await _hubContext.Clients.Group(tribeId).SendAsync("ReceiveMessage", tribeId, messageToSend);
        }
    }
}