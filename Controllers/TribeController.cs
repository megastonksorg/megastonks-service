using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Megastonks.Models.Tribe;
using Megastonks.Services;

namespace Megastonks.Controllers
{
	[Authorize]
	[Controller]
	[Route("[controller]")]
	public class TribeController : BaseController
	{
        private readonly TribeService _tribeService;

        public TribeController(TribeService tribeService)
		{
			_tribeService = tribeService;
		}

        [HttpPost("create")]
        public ActionResult<TribeResponse> CreateTribe(string name)
        {
            var result = _tribeService.CreateTribe(Account, name);
            return Ok(result);
        }
    }
}