using Microsoft.AspNetCore.Mvc;
using Megastonks.Entities;
using Megastonks.Services;
using Megastonks.Models.Account;
using Megastonks.Models;
using Microsoft.AspNetCore.Authorization;

namespace Megastonks.Controllers
{
    [Controller]
    [Route("[controller]")]
    public class AccountController : BaseController
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("refresh")]
        public ActionResult<AuthenticateResponse> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var response = _accountService.RefreshToken(refreshToken, ipAddress());
            return Ok(response);
        }

        [HttpPost("register")]
        public ActionResult<RegisterResponse> Register([FromBody] RegisterRequest model)
        {
            var response = _accountService.Register(model);
            return Ok(response);
        }

        [HttpPost("authenticate")]
        public ActionResult<AuthenticateResponse> Authenticate([FromBody]  AuthenticateRequest model)
        {
            var response = _accountService.Authenticate(model, ipAddress());
            return Ok(response);
        }

        [HttpPost("doesAccountExist")]
        public ActionResult<SuccessResponse> DoesAccountExist(string walletAddress)
        {
            var response = _accountService.DoesAccountExist(walletAddress);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("updateName")]
        public ActionResult<string> UpdateName(string fullName)
        {
            var response = _accountService.UpdateName(Account, fullName);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("updateProfilePhoto")]
        public ActionResult<Uri> UpdateProfilePhoto(string photoUrl)
        {
            var response = _accountService.UpdateProfilePhoto(Account, photoUrl);
            return Ok(response);
        }

        [Authorize]
        [HttpPost("updateAppleDeviceToken")]
        public ActionResult<EmptyResponse> UpdateAppleDeviceToken(string deviceToken)
        {
            var response = _accountService.UpdateDeviceToken(Account, DeviceType.apple, deviceToken);
            return Ok(response);
        }

        private string ipAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}