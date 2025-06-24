using System;
using course_service.Data.Entities;
using course_service.Modules.Lesson.DTOs;

namespace course_service.Modules.Lesson.Interfaces;

public interface ILessonService
{
    Task<LessonDto> CreateOneAsync(CreateLessonDto lessonDto);
    Task<LessonDto> UpdateOneAsync(Guid id, UpdateLessonDto updateLessonDto);
    Task<LessonDto> GetOneByIdAsync(Guid id);
    Task<IEnumerable<LessonDto>> GetAllAsync(Guid? moduleId = null);
}
