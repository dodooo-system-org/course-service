using System;
using course_service.Data.Entities;
using course_service.Shared.DTOs;

namespace course_service.Modules.Course.DTOs;

public class CourseDto : BaseDto
{
    public Guid CourseId { get; set; } = Guid.NewGuid();
    public string CourseName { get; set; } = string.Empty;
    public string CourseDescription { get; set; } = string.Empty;
    public CourseLevel Level { get; set; } = CourseLevel.Beginner;
    public string? CourseImageUrl { get; set; }
    public required CategoryEntity Category { get; set; }
    public int ModuleCount { get; set; } = 0;
    public int LessonCount { get; set; } = 0;
}
