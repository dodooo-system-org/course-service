using System;
using course_service.Data.Entities;
using course_service.Modules.Course.DTOs;

namespace course_service.Modules.Course.Mappers;

public static class CourseMapper
{
    static public CourseDto MapToDto(this CourseEntity courseEntity)
    {
        if (courseEntity == null)
        {
            throw new ArgumentNullException(nameof(courseEntity), "Course entity cannot be null");
        }
        if (courseEntity.CourseId == Guid.Empty)
        {
            throw new ArgumentException("CourseId cannot be empty", nameof(courseEntity));
        }
        return new CourseDto
        {
            CourseId = courseEntity.CourseId,
            CourseName = courseEntity.CourseName,
            CourseDescription = courseEntity.CourseDescription,
            Level = courseEntity.Level,
            CourseImageUrl = courseEntity.CourseImageUrl,
            Category = courseEntity.Category ?? throw new InvalidOperationException("Course category cannot be null"),
            CreatedAt = courseEntity.CreatedAt,
            UpdatedAt = courseEntity.UpdatedAt,
            DeletedAt = courseEntity.DeletedAt,
            IsActive = courseEntity.IsActive,
            IsDeleted = courseEntity.IsDeleted,
            ModuleCount = 0,
            LessonCount = 0
        };
    }

    static public CourseDto MapToDto(this CourseEntity courseEntity, int moduleCount, int lessonCount)
    {
        if (courseEntity == null)
        {
            throw new ArgumentNullException(nameof(courseEntity), "Course entity cannot be null");
        }
        if (courseEntity.CourseId == Guid.Empty)
        {
            throw new ArgumentException("CourseId cannot be empty", nameof(courseEntity));
        }
        return new CourseDto
        {
            CourseId = courseEntity.CourseId,
            CourseName = courseEntity.CourseName,
            CourseDescription = courseEntity.CourseDescription,
            Level = courseEntity.Level,
            CourseImageUrl = courseEntity.CourseImageUrl,
            Category = courseEntity.Category ?? throw new InvalidOperationException("Course category cannot be null"),
            CreatedAt = courseEntity.CreatedAt,
            UpdatedAt = courseEntity.UpdatedAt,
            DeletedAt = courseEntity.DeletedAt,
            IsActive = courseEntity.IsActive,
            IsDeleted = courseEntity.IsDeleted,
            ModuleCount = moduleCount,
            LessonCount = lessonCount
        };
    }
}
