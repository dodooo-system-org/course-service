using System;
using course_service.Shared.DTOs;

namespace course_service.Modules.Modules.DTOs;

public class ModuleDto : BaseDto
{
    public required Guid ModuleId { get; set; }
    public required string ModuleName { get; set; }
    public required string ModuleDescription { get; set; }
    public required decimal Order { get; set; }
    public required Guid CourseId { get; set; }
}
