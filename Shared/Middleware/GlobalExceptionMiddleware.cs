using System;
using System.Net;
using System.Text.Json;

namespace course_service.Shared.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
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
