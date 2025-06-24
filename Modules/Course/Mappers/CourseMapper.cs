using System;
using course_service.Data.Entities;
using course_service.Modules.Course.DTOs;

namespace course_service.Modules.Course.Mappers;

public static class CourseMapper
{
    static public CourseDto MapToDto(this CourseEntity courseEntity)
    {
        return new CourseDto
        {
            CourseId = courseEntity.CourseId,
            CourseName = courseEntity.CourseName,
            CourseDescription = courseEntity.CourseDescription,
            Level = courseEntity.Level,
            CourseImageUrl = courseEntity.CourseImageUrl,
            Category = courseEntity.Category,
            CreatedAt = courseEntity.CreatedAt,
            UpdatedAt = courseEntity.UpdatedAt,
            DeletedAt = courseEntity.DeletedAt
        };
    }
}
