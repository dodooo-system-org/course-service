using System;
using course_service.Data;
using course_service.Data.Entities;
using course_service.Modules.Course.Interfaces;
using course_service.Modules.Modules.DTOs;
using course_service.Modules.Modules.Interfaces;
using course_service.Modules.Modules.Mappers;
using course_service.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace course_service.Modules.Modules.Services;

public class ModuleService : IModuleService
{
    private readonly AppDbContext _context;
    private readonly ICourseService _courseService;
    private readonly ILogger<ModuleService> _logger;
    public ModuleService(AppDbContext context, ICourseService courseService)
    {
        _context = context;
        _courseService = courseService;
        _logger = LoggerHelper.GetLogger<ModuleService>();
    }

    public async Task<ModuleDto> CreateOneAsync(CreateModuleDto module)
    {
        try
        {
            // Check course is valid
            if (module.CourseId == Guid.Empty)
            {
                throw new ArgumentException("Course ID cannot be empty.", nameof(module.CourseId));
            }
            var course = await _courseService.GetCourseByIdAsync(module.CourseId);
            if (course == null)
            {
                throw new KeyNotFoundException("Course not found");
            }
            // Check module order is existing in a course
            bool isExistingOrder = await _context.Modules.AnyAsync(m => m.Order == module.Order && m.CourseId == module.CourseId);
            if (isExistingOrder)
            {
                throw new InvalidOperationException($"Module order {module.Order} already exists");
            }

            // Create new module
            ModuleEntity newModule = new()
            {
                ModuleName = module.ModuleName,
                ModuleDescription = module.ModuleDescription,
                Order = module.Order,
                CourseId = module.CourseId,
                Course = null!
            };
            _context.Modules.Add(newModule);
            await _context.SaveChangesAsync();
            return newModule.MapModuleDto();
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Create module failed");
            throw ServiceErrorHelper.GenerateErrorService(error, "Failed to create module");
        }
    }

    public async Task<IEnumerable<ModuleDto>> GetListAsync(Guid courseId)
    {
        try
        {
            var modules = await _context.Modules.ToListAsync();
            return modules.Where(m => m.CourseId == courseId).Select(module => module.MapModuleDto()).OrderBy(m => m.Order);
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Get list of modules failed");
            throw ServiceErrorHelper.GenerateErrorService(error, "Failed to get list of modules");
        }
    }

    public async Task<ModuleDto> GetOneAsync(Guid id)
    {
        try
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Module ID cannot be empty.", nameof(id));
            }
            var module = await _context.Modules.FirstOrDefaultAsync(c => c.ModuleId == id);
            if (module == null)
            {
                throw new KeyNotFoundException("Module not found");
            }
            return module.MapModuleDto();
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Get module failed");
            throw ServiceErrorHelper.GenerateErrorService(error, "Failed to get module");
        }
    }

    public async Task<ModuleDto> UpdateOneAsync(Guid id, UpdateModuleDto module)
    {
        try
        {
            var existingModule = await _context.Modules.FirstOrDefaultAsync(c => c.ModuleId == id);
            if (existingModule == null)
            {
                throw new KeyNotFoundException("Module not found");
            }

            existingModule.ModuleName = module.ModuleName;
            existingModule.ModuleDescription = module.ModuleDescription;
            existingModule.Order = module.Order;
            existingModule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existingModule.MapModuleDto();
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Update module failed");
            throw ServiceErrorHelper.GenerateErrorService(error, "Failed to update module");
        }
    }
}
