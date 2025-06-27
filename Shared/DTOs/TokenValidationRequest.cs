using System;
using System.Text.Json.Serialization;

namespace course_service.Shared.Services.RabbitMQ.DTOs;

public class TokenValidationRequest
{
    // Match Typescript convention
    [JsonPropertyName("token")]
    public required string Token { get; set; }
    [JsonPropertyName("correlationId")]
    public required string CorrelationId { get; set; }
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
