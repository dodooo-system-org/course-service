using System;
using System.Net;
using System.Text.Json;
using course_service.Shared.Exceptions;
using Microsoft.Extensions.Logging;

namespace course_service.Shared.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        private int GetStatusCode(Exception ex)
        {
            switch (ex)
            {
                case UnauthorizedAccessException:
                    return (int)HttpStatusCode.Unauthorized;
                case ArgumentException:
                    return (int)HttpStatusCode.BadRequest;
                case KeyNotFoundException:
                    return (int)HttpStatusCode.NotFound;
                case InvalidOperationException:
                    return (int)HttpStatusCode.Conflict;
                case ForbiddenAccessException:
                    return (int)HttpStatusCode.Forbidden;
                default:
                    return (int)HttpStatusCode.InternalServerError;
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log the exception with full details
                _logger.LogError(ex, "An unhandled exception occurred. Request Path: {RequestPath}, Method: {RequestMethod}",
                    context.Request.Path, context.Request.Method);

                int statusCode = this.GetStatusCode(ex);

                string errorMessage = ex.GetType() != typeof(Exception) ? ex.Message : "An unexpected error occurred";
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new { error = errorMessage, timestamp = DateTime.UtcNow });
                await context.Response.WriteAsync(result);
            }
        }


    }
}
