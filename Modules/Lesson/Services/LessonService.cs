using course_service.Data;
using course_service.Data.Entities;
using course_service.Modules.Lesson.DTOs;
using course_service.Modules.Lesson.Interfaces;
using course_service.Modules.Lesson.Mappers;
using course_service.Modules.Modules.Interfaces;
using course_service.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace course_service.Modules.Lesson;

public class LessonService : ILessonService
{
    private readonly AppDbContext _context;
    private readonly IModuleService _moduleService;
    private readonly ILogger<LessonService> _logger;

    public LessonService(AppDbContext context, IModuleService moduleService)
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

            var existedOrderLesson = await _context.Lessons
                .Where(l => l.ModuleId == lessonDto.ModuleId && l.Order == lessonDto.Order)
                .FirstOrDefaultAsync();
            if (existedOrderLesson != null)
            {
                throw new ArgumentException("Order was existed");
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
            throw ServiceErrorHelper.GenerateErrorService(ex, "Failed to create lesson");
        }
    }

    public async Task<IEnumerable<LessonDto>> GetAllAsync(Guid moduleId)
    {
        try
        {
            // Check if module exists
            var existedModule = await _moduleService.GetOneAsync(moduleId);
            if (existedModule == null)
            {
                throw new KeyNotFoundException($"Module not found");
            }

            var lessons = await _context.Lessons.Where(l => l.ModuleId == moduleId)
                .Select(l => l.MapToDto())
                .ToListAsync();
            return lessons;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error get list of lessons");
            throw ServiceErrorHelper.GenerateErrorService(ex, "Failed to get lessons");
        }
    }

    public async Task<LessonDto> GetOneByIdAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Lesson ID cannot be empty");
            }

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
            throw ServiceErrorHelper.GenerateErrorService(ex, "Failed to get lesson");
        }
    }

    public async Task<LessonDto> UpdateOneAsync(Guid id, UpdateLessonDto updateLessonDto)
    {
        try
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Lesson ID cannot be empty");
            }

            var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.LessonId == id);
            if (lesson == null)
            {
                throw new KeyNotFoundException("Lesson not found");
            }

            // Check for duplicate order within the same module (excluding current lesson)
            var existedOrderLesson = await _context.Lessons
                .Where(l => l.ModuleId == updateLessonDto.ModuleId && l.Order == updateLessonDto.Order && l.LessonId != id)
                .FirstOrDefaultAsync();
            if (existedOrderLesson != null)
            {
                throw new ArgumentException("Order was existed");
            }

            // Update lesson properties without modifying ModuleId
            lesson.LessonName = updateLessonDto.LessonName;
            lesson.LessonDescription = updateLessonDto.LessonDescription;
            lesson.Order = updateLessonDto.Order;
            lesson.Duration = updateLessonDto.Duration;
            lesson.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return lesson.MapToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating lesson with ID: {id}");
            throw ServiceErrorHelper.GenerateErrorService(ex, "Failed to update lesson");
        }
    }
}
