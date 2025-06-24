using System;
using course_service.Data.Entities;
using course_service.Modules.Lesson.DTOs;

namespace course_service.Modules.Lesson.Mappers;

public static class LessonMapper
{
    public static LessonDto MapToDto(this LessonEntity lessonEntity)
    {
        return new LessonDto
        {
            LessonId = lessonEntity.LessonId,
            LessonName = lessonEntity.LessonName,
            LessonDescription = lessonEntity.LessonDescription,
            Order = lessonEntity.Order,
            ModuleId = lessonEntity.ModuleId
        };
    }
}
