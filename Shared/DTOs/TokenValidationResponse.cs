using System;
using System.Text.Json.Serialization;
using course_service.Shared.DTOs;

namespace course_service.Shared.Services.RabbitMQ.DTOs;

public class TokenValidationResponse
{
    [JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; } = string.Empty;

    [JsonPropertyName("isValid")]
    public bool IsValid { get; set; }

    [JsonPropertyName("auth")]
    public AuthInfoDto? Auth { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; } = DateTime.UtcNow;
}
