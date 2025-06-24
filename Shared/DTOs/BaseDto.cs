using System;

namespace course_service.Shared.DTOs;

public class BaseDto
{
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
