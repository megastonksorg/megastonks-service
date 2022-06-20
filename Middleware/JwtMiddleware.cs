using Megastonks.Helpers;
using Microsoft.Extensions.Options;

namespace Megastonks.Middleware
{
	public class JwtMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly AppSettings _appSettings;

		public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings)
		{
			_next = next;
			_appSettings = appSettings.Value;
		}

		public JwtMiddleware()
		{
		}
	}
}

