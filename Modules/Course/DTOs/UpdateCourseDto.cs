using course_service.Shared.DTOs;

namespace course_service.Modules.Course.DTOs;

public class UpdateCourseDto : CreateCourseDto
{
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}
