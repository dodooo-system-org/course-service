using System;
using course_service.Data.Entities;
using course_service.Modules.LessonPart.DTOs;

namespace course_service.Modules.LessonPart.Mappers;

public static class LessonPartMapper
{
    public static LessonPartDto MapToDto(this LessonPartEntity lessonPartEntity)
    {
        return new LessonPartDto
        {
            LessonPartId = lessonPartEntity.LessonPartId,
            LessonPartName = lessonPartEntity.LessonPartName,
            LessonPartContent = lessonPartEntity.LessonPartContent,
            Order = lessonPartEntity.Order,
            LessonVideoUrl = lessonPartEntity.LessonVideoUrl,
            LessonId = lessonPartEntity.LessonId
        };
    }
}
