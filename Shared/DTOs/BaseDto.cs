using System;

namespace course_service.Shared.DTOs;

public class BaseDto
{
    public bool? IsActive { get; set; }
    public bool? IsDeleted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
