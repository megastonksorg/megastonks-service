﻿using System.Net;
using Newtonsoft.Json;
using Megastonks.Helpers;

namespace Megastonks.Middleware
{
	public class ErrorHandlerMiddleware
	{
		private readonly RequestDelegate _next;


		public ErrorHandlerMiddleware(RequestDelegate next)
		{
			_next = next;
		}

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";

                switch (error)
                {
                    case AppException e:
                        // custom application error
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;
                    case KeyNotFoundException e:
                        // not found error
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    default:
                        // unhandled error
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                var result = JsonConvert.SerializeObject(new { message = error?.Message });
                await response.WriteAsync(result);
            }
        }
    }
}