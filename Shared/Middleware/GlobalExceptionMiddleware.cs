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

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                string errorMessage = ex.GetType() != typeof(Exception) ? ex.Message : "An unexpected error occurred";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new { error = errorMessage });
                await context.Response.WriteAsync(result);
            }
        }
    }
}
