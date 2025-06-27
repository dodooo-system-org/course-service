using System;
using course_service.Shared.RMQ.Interfaces;
using course_service.Shared.Services.RabbitMQ.DTOs;
using Microsoft.AspNetCore.Authorization;
using RabbitMQ.Client;

namespace course_service.Shared.Middleware;

public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRMQService _rmqService;
    public AuthenticationMiddleware(RequestDelegate next, IRMQService rmqService)
    {
        _next = next;
        _rmqService = rmqService;
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

    private TokenValidationRequest CreateTokenValidationRequest(string token)
    {
        string correlationId = Guid.NewGuid().ToString();
        return new TokenValidationRequest
        {
            CorrelationId = correlationId,
            Token = token
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        throw new UnauthorizedAccessException("Unauthorized access");
    }
}
