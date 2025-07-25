using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace course_service.Shared.DTOs;

public enum PaginationOrderBy
{
    ASC,
    DESC
}

public class PaginationDto
{
    [JsonPropertyName("page")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid page number.")]
    public int Page { get; set; } = 1;

    [JsonPropertyName("size")]
    [Range(1, 100, ErrorMessage = "Size must be between 1 and 100.")]
    public int Size { get; set; } = 10;

    [JsonPropertyName("query")]
    [StringLength(100, ErrorMessage = "Query cannot exceed 100 characters.")]
    public string Query { get; set; } = string.Empty;

    [JsonPropertyName("orderBy")]
    [EnumDataType(typeof(PaginationOrderBy), ErrorMessage = "Invalid order by value.")]
    public PaginationOrderBy OrderBy { get; set; } = PaginationOrderBy.ASC;
}
