using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Megastonks.Models.Tribe;
using Megastonks.Services;
using Megastonks.Entities;
using Megastonks.Models;

namespace Megastonks.Controllers
{
    [Authorize]
    [Controller]
    [Route("[controller]")]
    public class TribeController : BaseController
    {
        private readonly ITribeService _tribeService;

        public TribeController(ITribeService tribeService)
        {
            _tribeService = tribeService;
        }

        [HttpGet]
        public ActionResult<List<TribeResponse>> Get()
        {
            var result = _tribeService.GetTribes(Account);
            return Ok(result);
        }

        [HttpPost("create")]
        public ActionResult<TribeResponse> CreateTribe(string name)
        {
            var result = _tribeService.CreateTribe(Account, name);
            return Ok(result);
        }

        [HttpPost("invite")]
        public ActionResult<SuccessResponse> InviteToTribe(string tribeId, string code)
        {
            var result = _tribeService.InviteToTribe(Account, tribeId, code);
            return Ok(result);
        }

        [HttpPost("join")]
        public ActionResult<TribeResponse> JoinTribe(string pin, string code)
        {
            var result = _tribeService.JoinTribe(Account, pin, code);
            return Ok(result);
        }

        [HttpPost("leave")]
        public ActionResult<SuccessResponse> LeaveTribe(string id)
        {
            var result = _tribeService.LeaveTribe(Account, id);
            return Ok(result);
        }

        [HttpPost("remove")]
        public ActionResult<SuccessResponse> RemoveFromTribe(string id, string memberId)
        {
            var result = _tribeService.RemoveFromTribe(Account, id, memberId);
            return Ok(result);
        }

        [HttpPatch("name")]
        public ActionResult<string> UpdateTribeName(string id, string name)
        {
            var result = _tribeService.UpdateTribeName(Account, id, name);
            return Ok(result);
        }
    }
}