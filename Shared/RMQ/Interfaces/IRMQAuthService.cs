using System;
using course_service.Shared.Services.RabbitMQ.DTOs;

namespace course_service.Shared.RMQ.Interfaces;

public interface IRMQAuthService
{
    public Task<TokenValidationResponse> ValidateTokenAsync(string token);
}
