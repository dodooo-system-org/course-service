using System;
using course_service.Data.Entities;
using course_service.Modules.Course.DTOs;
using course_service.Shared.DTOs;

namespace course_service.Modules.Course.Interfaces;

public interface ICourseService
{
    Task<CourseEntity> CreateCourseAsync(CreateCourseDto course);
    Task<CourseDto> GetCourseByIdAsync(Guid courseId);
    Task<MetaPaginationDto<List<CourseDto>>> GetAllCoursesAsync(AllCourseQueryDto queryDto);
    Task<CourseDto> UpdateCourseAsync(Guid courseId, UpdateCourseDto course);
    Task<MetaPaginationDto<List<CourseDto>>> GetDeletedCoursesAsync(GetDeletedCourseDto queryDto);
}
