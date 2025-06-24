using System;
using course_service.Data.Entities;
using course_service.Modules.Course.DTOs;

namespace course_service.Modules.Course.Interfaces;

public interface ICourseService
{
    Task<CourseEntity> CreateCourseAsync(CreateCourseDto course);
    Task<CourseDto> GetCourseByIdAsync(Guid courseId);
    Task<List<CourseDto>> GetAllCoursesAsync();
    Task<CourseDto> UpdateCourseAsync(Guid courseId, UpdateCourseDto course);
}
