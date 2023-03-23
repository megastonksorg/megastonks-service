﻿using System;
using Megastonks.Models;
using Megastonks.Models.Account;
using Megastonks.Models.Message;
using Megastonks.Models.Tribe;
using Megastonks.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Megastonks.Controllers
{
    [Authorize]
    [Controller]
    [Route("[controller]")]
    public class MessageController : BaseController
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        [HttpGet]
        public ActionResult<List<MessageResponse>> GetMessages(string tribeId)
        {
            var result = _messageService.GetMessages(Account, tribeId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<EmptyResponse>> PostMessage([FromBody] PostMessageRequest model)
        {
            await _messageService.PostMessage(Account, model);
            return Ok(new EmptyResponse());
        }

        [HttpPost]
        public ActionResult<EmptyResponse> MarkAsViewed(string messageId)
        {
            var result = _messageService.MarkAsViewed(Account, messageId);
            return Ok(result);
        }

        [HttpDelete]
        public ActionResult<SuccessResponse> DeleteMessage(string messageId)
        {
            var result = _messageService.DeleteMessage(Account, messageId);
            return Ok(result);
        }
    }
}