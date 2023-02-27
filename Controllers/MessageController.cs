using System;
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

        [HttpPost("tea")]
        public ActionResult<MessageResponse> PostMessage([FromBody] PostMessageReqest model)
        {
            var result = _messageService.PostMessage(Account, Entities.Message.MessageTag.tea, model);
            return Ok(result);
        }
    }
}