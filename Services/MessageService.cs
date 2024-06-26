﻿using System;
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
        MessageResponse GetMessage(Account account, string messageId);
        List<MessageResponse> GetMessages(Account account, string tribeId);
        SuccessResponse DeleteMessage(Account account, string messageId);
        List<string> GetViewers(Account account, string messageId);
        EmptyResponse MarkAsViewed(Account account, string messageId);
        MessageResponse PostMessage(Account account, PostMessageRequest model);
        void AddEventMessage(Account account, Tribe tribe, string eventTitle);
        List<Guid> GetAllowedTeaRecipients(Account account);
        EmptyResponse DeleteExpiredMessages();
    }

    public class MessageService : IMessageService
    {
        private readonly int messageExpiryInHours = 24;
        private readonly string serverSender = "server";
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

                var messages = _context.Messages
                    .AsNoTracking()
                    .Where(x => x.Tribe == tribe && !x.Deleted && x.TimeStamp > DateTime.UtcNow.AddHours(-messageExpiryInHours))
                    .Select(x => new MessageResponse {
                        Id = x.Id,
                        Body = x.Body,
                        Caption = x.Caption,
                        Type = x.Type,
                        SenderWalletAddress = x.Sender == null ? serverSender : x.Sender.WalletAddress,
                        Tag = x.Tag,
                        Context = x.Context == null ? null : x.Context.Id,
                        Keys = x.Keys.Where(y => y.PublicKey == account.PublicKey).ToList(),
                        Expires = x.Expires,
                        TimeStamp = x.TimeStamp
                    }).ToList();

                return messages;
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message} \n {e.StackTrace}");
                throw new AppException(e.Message);
            }
        }

        public MessageResponse GetMessage(Account account, string messageId)
        {
            try
            {
                var message = _context.Messages
                    .AsNoTracking()
                    .Where(x => x.Id == Guid.Parse(messageId))
                    .Select(x => new MessageResponse
                    {
                        Id = x.Id,
                        Body = x.Body,
                        Caption = x.Caption,
                        Type = x.Type,
                        SenderWalletAddress = x.Sender == null ? serverSender : x.Sender.WalletAddress,
                        Tag = x.Tag,
                        Context = x.Context == null ? null : x.Context.Id,
                        Keys = x.Keys.Where(y => y.PublicKey == account.PublicKey).ToList(),
                        Expires = x.Expires,
                        TimeStamp = x.TimeStamp
                    }).FirstOrDefault();

                if (message == null)
                {
                    throw new AppException("Message Not Found");
                }
                return message;
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message} \n {e.StackTrace}");
                throw new AppException(e.Message);
            }
        }

        public SuccessResponse DeleteMessage(Account account, string messageId)
        {
            try
            {
                var messageToDelete = _context.Messages
                    .Include(x => x.Tribe)
                    .Include(x => x.Sender)
                    .Where(x => x.Sender == account && x.Id == Guid.Parse(messageId))
                    .FirstOrDefault();

                if (messageToDelete == null)
                {
                    throw new AppException("Invalid Message Id");
                }

                messageToDelete.Deleted = true;

                _context.Update(messageToDelete);
                _context.SaveChanges();

                sendDeleteTohub(messageToDelete.Tribe, messageToDelete.Id);

                return new SuccessResponse
                {
                    Success = true
                };
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message} \n {e.StackTrace}");
                throw new AppException(e.Message);
            }
        }

        public List<string> GetViewers(Account account, string messageId)
        {
            try
            {
                Guid messageIdGuid = Guid.Parse(messageId);
                var message = _context.Messages
                    .AsNoTracking()
                    .Include(x => x.Viewers)
                    .ThenInclude(x => x.Account)
                    .Where(x => x.Id == messageIdGuid)
                    .FirstOrDefault();

                if (message == null)
                {
                    return new List<string>();
                }

                return message.Viewers.Select(x => x.Account.WalletAddress).ToList();
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message} \n {e.StackTrace}");
                throw new AppException(e.Message);
            }
        }

        public EmptyResponse MarkAsViewed(Account account, string messageId)
        {
            try
            {
                Guid messageIdGuid = Guid.Parse(messageId);
                var message = _context.Messages
                    .Include(x => x.Viewers)
                    .Where(x => x.Id == messageIdGuid)
                    .FirstOrDefault();

                if (message == null)
                {
                    throw new AppException("Invalid Message Id");
                }

                if (message.Viewers == null)
                {
                    List<MessageViewer> newViewers = new List<MessageViewer>();
                    newViewers.Add(
                            new MessageViewer
                            {
                                Account = account,
                                Message = message
                            }
                        );
                    message.Viewers = newViewers;
                    _context.Update(message);
                }
                else
                {
                    if (message.Viewers.Any(x => x.Account == account))
                    {
                        return new EmptyResponse();
                    }
                    else
                    {
                        message.Viewers.Add(
                                new MessageViewer
                                {
                                    Account = account,
                                    Message = message
                                }
                            );
                        _context.Update(message);
                    }
                }

                _context.SaveChanges();

                return new EmptyResponse();
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message} \n {e.StackTrace}");
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
                    contextMessage = _context.Messages
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

                sendTohub(tribe, newMessage);
                
                //Send Push Notifications
                string messageType = "";
                switch (newMessage.Type)
                {
                    case MessageType.image:
                        messageType = "picture 📸";
                        break;
                    case MessageType.video:
                        messageType = "video 🎥";
                        break;
                    case MessageType.note:
                        messageType = "note 📝";
                        break;
                }
                string notificationBody = newMessage.Tag == MessageTag.tea ? $"Someone shared a {messageType}" : $"Message from {account.FullName}";
                _pushNotitificationService.SendPushToTribe(account, tribe, newMessage.Id, messageTag, notificationBody);

                return mapMessageToMessageResponse(newMessage);
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message} \n {e.StackTrace}");
                throw new AppException(e.Message);
            }
        }

        public void AddEventMessage(Account account, Tribe tribe, string eventTitle)
        {
            try
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

                sendTohub(tribe, eventMessage);

                //Send Push Notifications
                _pushNotitificationService.SendPushToTribe(account, tribe, eventMessage.Id, eventMessage.Tag, eventTitle);
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message} \n {e.StackTrace}");
                throw new AppException(e.Message);
            }
        }

        public List<Guid> GetAllowedTeaRecipients(Account account)
        {
            try
            {
                var allowedTribeIds = new List<Guid>();
                var tribes = _context.Tribes.AsNoTracking()
                    .Where(x => x.TribeMembers.Any(y => y.Account == account))
                    .ToList();

                if (tribes.Count() == 0)
                {
                    return allowedTribeIds;
                }
                else
                {
                    foreach(var tribe in tribes)
                    {
                        var teaCount = _context.Messages
                            .AsNoTracking()
                            .Where(x => x.Tribe == tribe && !x.Deleted && x.Tag == MessageTag.tea && x.TimeStamp > DateTime.UtcNow.AddHours(-messageExpiryInHours))
                            .ToList()
                            .Count();
                        if (teaCount < 39) //Tea limit per day is 40 for each Tribe
                        {
                            allowedTribeIds.Add(tribe.Id);
                        }
                    }
                    return allowedTribeIds;
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message} \n {e.StackTrace}");
                throw new AppException(e.Message);
            }
        }

        public EmptyResponse DeleteExpiredMessages()
        {
            try
            {
                var expiredMessages = _context.Messages
                    .Include(x => x.Context)
                    .Where(x => x.TimeStamp <= DateTime.UtcNow.AddHours(-messageExpiryInHours))
                    .ToArray();

                if (expiredMessages == null)
                {
                    throw new AppException("No Expired Messages");
                }

                //Remove context from the message first
                foreach (var message in expiredMessages)
                {
                    if (message.Context != null)
                    {
                        message.Context = null;
                        _context.Update(message);
                    }
                }

                _context.RemoveRange(expiredMessages);
                _context.SaveChanges();

                return new EmptyResponse();
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message} \n {e.StackTrace}");
                throw new AppException(e.Message);
            }
        }

        private MessageResponse mapMessageToMessageResponse(Message message)
        {
            return new MessageResponse
            {
                Id = message.Id,
                Body = message.Body,
                Caption = message.Caption,
                Type = message.Type,
                SenderWalletAddress = message.Sender == null ? serverSender : message.Sender.WalletAddress,
                Tag = message.Tag,
                Context = message.Context == null ? null : message.Context.Id,
                Keys = message.Keys,
                Expires = message.Expires,
                TimeStamp = message.TimeStamp
            };
        }

        private async void sendTohub(Tribe tribe, Message message)
        {
            //Send to all tribe members
            string tribeId = tribe.Id.ToString();
            MessageResponse messageToSend = mapMessageToMessageResponse(message);
            await _hubContext.Clients.Group(tribeId).SendAsync("ReceiveMessage", tribeId, messageToSend);
        }

        private async void sendDeleteTohub(Tribe tribe, Guid messageId)
        {
            //Send to all tribe members
            string tribeId = tribe.Id.ToString();
            await _hubContext.Clients.Group(tribeId).SendAsync("DeleteMessage", tribeId, messageId);
        }
    }
}