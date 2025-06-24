using System;
using course_service.Data.Entities;
using course_service.Modules.Modules.DTOs;

namespace course_service.Modules.Modules.Mappers;

public static class ModuleMapper
{
    public static ModuleDto MapModuleDto(this ModuleEntity module)
    {
        if (module == null)
        {
            throw new ArgumentNullException(nameof(module), "Module cannot be null");
        }

        return new ModuleDto
        {
            ModuleId = module.ModuleId,
            ModuleName = module.ModuleName,
            ModuleDescription = module.ModuleDescription,
            Order = module.Order,
            CourseId = module.CourseId,
            CreatedAt = module.CreatedAt,
            UpdatedAt = module.UpdatedAt,
            DeletedAt = module.DeletedAt
        };
    }
}
