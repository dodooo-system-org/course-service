using course_service.Shared.RMQ.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace course_service.Shared.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRMQAuthService _rmqAuthService;
    public AuthenticationMiddleware(RequestDelegate next, IRMQAuthService rmqAuthService)
    {
        _next = next;
        _rmqAuthService = rmqAuthService;
    }

    private string ExtractToken(HttpContext context)
    {
        // Extract the token from the Authorization header
        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var token = authHeader.ToString().Substring("Bearer ".Length).Trim();
            return token;
        }
        return String.Empty;
    }



    public async Task InvokeAsync(HttpContext context)
    {
        // Check if the endpoint has AllowAnonymous attribute
        var endpoint = context.GetEndpoint();
        var allowAnonymous = endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null;

        if (allowAnonymous)
        {
            // Skip authentication for anonymous endpoints
            await _next(context);
            return;
        }

        var token = this.ExtractToken(context);
        if (!string.IsNullOrEmpty(token))
        {
            var response = await _rmqAuthService.ValidateTokenAsync(token);
            if (!response.IsValid && response.Error != null)
            {
                throw new ArgumentException(response.Error);
            }
            else
            {
                context.Items["Auth"] = response.Auth;
                await _next(context);
                return;
            }
        }
        throw new UnauthorizedAccessException("Unauthorized access");
    }
}
