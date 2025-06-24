using System;
using course_service.Modules.LessonPart.DTOs;

namespace course_service.Modules.LessonPart.Interfaces;

public interface ILessonPartService
{
    Task<LessonPartDto> CreateOneAsync(CreateLessonPartDto lessonPartDto);
    Task<IEnumerable<LessonPartDto>> GetAllAsync(Guid lessonId);
    Task<LessonPartDto> GetOneAsync(Guid lessonPartId);
    Task<LessonPartDto> UpdateOneAsync(Guid lessonPartId, UpdateLessonPartDto lessonPartDto);
}
