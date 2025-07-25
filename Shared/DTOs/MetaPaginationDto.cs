using System;

namespace course_service.Shared.DTOs;

public class MetaDto
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Size);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

public class MetaPaginationDto<T>
{
    public required MetaDto Meta { get; set; }
    public required T Data { get; set; }
}