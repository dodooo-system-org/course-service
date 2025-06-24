using System;

namespace course_service.Modules.Lesson.DTOs;

public class LessonDto
{
    public Guid LessonId { get; set; }
    public required string LessonName { get; set; }
    public required string LessonDescription { get; set; }
    public decimal Order { get; set; }
    public int Duration { get; set; }
    public Guid? ModuleId { get; set; } = null;
}
