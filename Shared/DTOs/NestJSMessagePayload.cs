using System;
using System.Text.Json.Serialization;

namespace course_service.Shared.DTOs;

public class NestJSMessagePayload<T>
{
    [JsonPropertyName("pattern")]
    public required string Pattern { get; set; }

    [JsonPropertyName("data")]
    public required T Data { get; set; }
}
