﻿using Microsoft.AspNetCore.Mvc;
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
            setTokenCookie(response.RefreshToken);
            return Ok(response);
        }

        [HttpGet("requestAuthentication")]
        public ActionResult<string> RequestAuthentication()
        {
            var response = _accountService.RequestAuthentication();
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
            setTokenCookie(response.RefreshToken);
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
            var response = _accountService.UpdateAccountName(Account, fullName);
            return Ok(response);
        }

        private void setTokenCookie(string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(4)
            };
            Response.Cookies.Append("refreshToken", token, cookieOptions);
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