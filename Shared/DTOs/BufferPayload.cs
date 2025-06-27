using System.Text.Json.Serialization;

namespace course_service.Shared.DTOs;

public class BufferPayload
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public int[]? Data { get; set; }
}
