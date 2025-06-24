using course_service.Data;
using course_service.Data.Entities;
using course_service.Modules.Lesson.DTOs;
using course_service.Modules.Lesson.Interfaces;
using course_service.Modules.Lesson.Mappers;
using course_service.Modules.Modules.Services;
using course_service.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace course_service.Modules.Lesson;

public class LessonService : ILessonService
{
    private readonly AppDbContext _context;
    private readonly ModuleService _moduleService;
    private readonly ILogger<LessonService> _logger;

    public LessonService(AppDbContext context, ModuleService moduleService)
    {
        _context = context;
        _moduleService = moduleService;
        _logger = LoggerHelper.GetLogger<LessonService>();
    }

    public async Task<LessonDto> CreateOneAsync(CreateLessonDto lessonDto)
    {
        try
        {
            var existedModule = await _moduleService.GetOneAsync(lessonDto.ModuleId);
            if (existedModule == null)
            {
                throw new KeyNotFoundException($"Module not found");
            }

            var lessonEntity = new LessonEntity
            {
                LessonId = Guid.NewGuid(),
                LessonName = lessonDto.LessonName,
                LessonDescription = lessonDto.LessonDescription,
                Order = lessonDto.Order,
                Duration = lessonDto.Duration,
                ModuleId = lessonDto.ModuleId
            };

            _context.Lessons.Add(lessonEntity);
            await _context.SaveChangesAsync();
            return lessonEntity.MapToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lesson");
            throw ServiceErrorHelper.GenerateErrorService(ex);
        }
    }

    public async Task<IEnumerable<LessonDto>> GetAllAsync(Guid? moduleId = null)
    {
        try
        {
            var lessons = await _context.Lessons.ToListAsync();
            return lessons.Select(c => c.MapToDto()).Where(l => !moduleId.HasValue || l.ModuleId == moduleId.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error get list of lessons");
            throw ServiceErrorHelper.GenerateErrorService(ex);
        }
    }

    public async Task<LessonDto> GetOneByIdAsync(Guid id)
    {
        try
        {
            var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.LessonId == id);
            if (lesson == null)
            {
                throw new KeyNotFoundException($"Lesson not found");
            }
            return lesson.MapToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting lesson with ID: {id}");
            throw ServiceErrorHelper.GenerateErrorService(ex);
        }
    }

    public async Task<LessonDto> UpdateOneAsync(Guid id, UpdateLessonDto updateLessonDto)
    {
        try
        {
            var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.LessonId == id);
            if (lesson == null)
            {
                throw new KeyNotFoundException("Lesson not found");
            }

            // Update lesson properties
            lesson.LessonName = updateLessonDto.LessonName;
            lesson.LessonDescription = updateLessonDto.LessonDescription;
            lesson.Order = updateLessonDto.Order;
            lesson.Duration = updateLessonDto.Duration;
            lesson.ModuleId = updateLessonDto.ModuleId;
            lesson.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return lesson.MapToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating lesson with ID: {id}");
            throw ServiceErrorHelper.GenerateErrorService(ex);
        }
    }
}
