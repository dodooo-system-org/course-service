using System;
using course_service.Data;
using course_service.Data.Entities;
using course_service.Modules.Lesson.Interfaces;
using course_service.Modules.LessonPart.DTOs;
using course_service.Modules.LessonPart.Interfaces;
using course_service.Modules.LessonPart.Mappers;
using course_service.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace course_service.Modules.LessonPart.Services;

public class LessonPartService : ILessonPartService
{
    private readonly AppDbContext _context;
    private readonly ILessonService _lessonService;
    private readonly ILogger<LessonPartService> _logger;
    public LessonPartService(AppDbContext context, ILessonService lessonService)
    {
        _context = context;
        _lessonService = lessonService;
        _logger = LoggerHelper.GetLogger<LessonPartService>();
    }
    public async Task<LessonPartDto> CreateOneAsync(CreateLessonPartDto lessonPartDto)
    {
        try
        {
            var existedLesson = await _lessonService.GetOneByIdAsync(lessonPartDto.LessonId);
            if (existedLesson == null)
            {
                throw new KeyNotFoundException("Lesson not found");
            }

            // Check for duplicate order within the same lesson
            var existedOrderLessonPart = await _context.LessonParts
                .Where(lp => lp.LessonId == lessonPartDto.LessonId && lp.Order == lessonPartDto.Order)
                .FirstOrDefaultAsync();
            if (existedOrderLessonPart != null)
            {
                throw new ArgumentException("Order was existed");
            }

            var lessonPartEntity = new LessonPartEntity
            {
                LessonPartName = lessonPartDto.LessonPartName,
                LessonPartContent = lessonPartDto.LessonPartContent,
                Order = lessonPartDto.Order,
                LessonVideoUrl = lessonPartDto.LessonVideoUrl,
                LessonId = lessonPartDto.LessonId
            };

            _context.LessonParts.Add(lessonPartEntity);
            await _context.SaveChangesAsync();
            return lessonPartEntity.MapToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating lesson part");
            throw ServiceErrorHelper.GenerateErrorService(ex);
        }
    }

    public async Task<IEnumerable<LessonPartDto>> GetAllAsync(Guid lessonId)
    {
        try
        {
            var existedLesson = await _lessonService.GetOneByIdAsync(lessonId);
            if (existedLesson == null)
            {
                throw new KeyNotFoundException("Lesson not found");
            }
            var lessonParts = await _context.LessonParts.ToListAsync();

            return lessonParts
                .Where(lp => lp.LessonId == lessonId)
                .Select(lp => lp.MapToDto())
                .OrderBy(lp => lp.Order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error get list of lesson parts");
            throw ServiceErrorHelper.GenerateErrorService(ex);
        }
    }

    public async Task<LessonPartDto> GetOneAsync(Guid lessonPartId)
    {
        try
        {
            if (lessonPartId == Guid.Empty)
            {
                throw new ArgumentException("Lesson part ID cannot be empty");
            }

            var lessonPart = await _context.LessonParts.FirstOrDefaultAsync(lp => lp.LessonPartId == lessonPartId);
            if (lessonPart == null)
            {
                throw new KeyNotFoundException("Lesson part not found");
            }
            return lessonPart.MapToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting lesson part by id");
            throw ServiceErrorHelper.GenerateErrorService(ex);
        }
    }

    public async Task<LessonPartDto> UpdateOneAsync(Guid lessonPartId, UpdateLessonPartDto lessonPartDto)
    {
        try
        {
            if (lessonPartId == Guid.Empty)
            {
                throw new ArgumentException("Lesson part ID cannot be empty");
            }

            var lessonPart = await _context.LessonParts.FirstOrDefaultAsync(lp => lp.LessonPartId == lessonPartId);

            if (lessonPart == null)
            {
                throw new KeyNotFoundException("Lesson part not found");
            }

            // Update the lesson part properties without modifying lessonId
            lessonPart.LessonPartName = lessonPartDto.LessonPartName;
            lessonPart.LessonPartContent = lessonPartDto.LessonPartContent;
            lessonPart.Order = lessonPartDto.Order;
            lessonPart.LessonVideoUrl = lessonPartDto.LessonVideoUrl;

            await _context.SaveChangesAsync();
            return lessonPart.MapToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating lesson part");
            throw ServiceErrorHelper.GenerateErrorService(ex);
        }
    }
}
