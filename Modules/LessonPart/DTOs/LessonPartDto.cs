using System;

namespace course_service.Modules.LessonPart.DTOs;

public class LessonPartDto
{
    public required Guid LessonPartId { get; set; }
    public required string LessonPartName { get; set; }
    public required string LessonPartContent { get; set; }
    public required decimal Order { get; set; }
    public string? LessonVideoUrl { get; set; } = null;
    public Guid? LessonId { get; set; }
}
